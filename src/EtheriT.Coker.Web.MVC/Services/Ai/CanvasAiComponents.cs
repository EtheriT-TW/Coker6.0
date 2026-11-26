using HtmlAgilityPack;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public sealed partial class CanvasAiService
{
    internal static string MaterializeClassChanges(CanvasAiRequest input, string content)
    {
        var plan = JsonNode.Parse(content) as JsonObject;
        if (plan == null || plan["classChanges"] == null || plan["classChanges"] is JsonArray { Count: 0 }) return content;
        if (input.Scope != "selected" || plan["classChanges"] is not JsonArray { Count: > 0 and <= 8 } changes ||
            plan["html"] is JsonValue htmlValue && htmlValue.TryGetValue<string>(out var suppliedHtml) && !string.IsNullOrEmpty(suppliedHtml) ||
            plan["styles"] is JsonArray { Count: > 0 } || plan["textStyles"] is JsonArray { Count: > 0 })
            throw new FormatException("Class changes must be an isolated layout operation on the current selected boundary.");
        var document = new HtmlDocument(); document.LoadHtml(input.Html);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in changes)
        {
            if (value is not JsonObject change || change["targetId"] is not JsonValue idValue || !idValue.TryGetValue<string>(out var id) || string.IsNullOrEmpty(id))
                throw new FormatException("A class change needs an actual target ID.");
            if (input.ExecutionGoal is { } goal && id != goal.TargetId)
                throw new FormatException("Class changes must target the classified execution goal.");
            var matches = document.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element && n.GetAttributeValue("id", "") == id).Take(2).ToArray();
            if (matches.Length != 1 || !seen.Add(id)) throw new FormatException("Class targets must be unique and inside the current boundary.");
            string[] Tokens(string name)
            {
                if (change[name] is not JsonArray { Count: <= 100 } list) throw new FormatException("Class deltas require add and remove arrays.");
                return list.Select(v => v is JsonValue token && token.TryGetValue<string>(out var text) && text != null && Regex.IsMatch(text, @"\A[a-zA-Z_][a-zA-Z0-9_-]{0,99}\z")
                    ? text : throw new FormatException("Invalid class token.")).ToArray();
            }
            var remove = Tokens("remove"); var add = Tokens("add");
            var existing = matches[0].GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (remove.Any(c => !existing.Contains(c)) || add.Any(c => !input.KnowledgeClasses.Contains(c))) throw new FormatException("Class deltas require current tokens and deployed source choices.");
            matches[0].SetAttributeValue("class", string.Join(" ", existing.Except(remove, StringComparer.Ordinal).Concat(add).Distinct(StringComparer.Ordinal)));
        }
        var html = document.DocumentNode.OuterHtml;
        if (!TrySourceBackedClassEdit(input, html)) throw new FormatException("Class deltas must preserve the tree and produce a real difference.");
        plan["editMode"] = "content"; plan["html"] = html; plan.Remove("classChanges");
        return plan.ToJsonString();
    }

    // Creating a link is a structure operation. Its label is existing content,
    // not a placeholder for the missing destination handled by the settings UI.
    internal static bool PreservesLinkConversionText(CanvasAiRequest input, string proposed)
    {
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(proposed);
        var existingLinks = before.DocumentNode.Descendants("a").Select(n => n.GetAttributeValue("id", "")).ToHashSet(StringComparer.Ordinal);
        var newLinks = after.DocumentNode.Descendants("a").Where(n => !existingLinks.Contains(n.GetAttributeValue("id", ""))).ToArray();
        if (newLinks.Length == 0) return true;
        static string Text(HtmlNode node) => Regex.Replace(HtmlEntity.DeEntitize(node.InnerText), @"\s+", " ").Trim();
        var originalText = Text(before.DocumentNode);
        if (originalText.Length == 0) return true;
        if (Text(after.DocumentNode) != originalText ||
            !after.DocumentNode.Descendants().Any(n => n.GetAttributeValue("id", "") == input.SelectedId)) return false;
        return newLinks.All(n => n.Attributes["data-text"] == null ||
            Text(n).Length == 0 || Regex.Replace(HtmlEntity.DeEntitize(n.GetAttributeValue("data-text", "")), @"\s+", " ").Trim() == Text(n));
    }

    private static string CompactClassReview(CanvasAiRequest input, string content, string? html, bool allowClassChanges)
    {
        if (html == null || !TrySourceBackedClassEdit(input, html)) return content;
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(html);
        var originals = before.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element && n.Attributes["id"] != null)
            .GroupBy(n => n.GetAttributeValue("id", "")).Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => g.Single());
        var changes = after.DocumentNode.Descendants().Where(n => originals.TryGetValue(n.GetAttributeValue("id", ""), out var original) &&
            n.GetAttributeValue("class", "") != original.GetAttributeValue("class", ""))
            .Select(n => new { TargetId = n.GetAttributeValue("id", ""), Classes = n.GetAttributeValue("class", "") }).ToArray();
        var plan = JsonNode.Parse(content)!.AsObject();
        plan["html"] = "";
        plan["ClassChangesToReview"] = JsonSerializer.SerializeToNode(changes);
        if (allowClassChanges) plan["classChanges"] = JsonSerializer.SerializeToNode(after.DocumentNode.Descendants().Where(n => originals.TryGetValue(n.GetAttributeValue("id", ""), out var original) &&
            n.GetAttributeValue("class", "") != original.GetAttributeValue("class", "")).Select(n => {
                var id = n.GetAttributeValue("id", "");
                var old = originals[id].GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                var current = n.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                return new { targetId = id, remove = old.Except(current).ToArray(), add = current.Except(old).ToArray() };
            }).ToArray());
        plan["ReviewInstructions"] = allowClassChanges
            ? "Review these exact class changes against original Html and current ResourceKnowledge. Return classChanges with html empty and styles empty; the server materializes only those token changes on the original tree, preserving every other node, attribute and text."
            : "Review these exact class changes against original Html and current ResourceKnowledge. Return the complete original HTML with only the verified class changes, preserving every other node, attribute and text. Do not return classChanges: the gateway uses its existing HTML/styles envelope.";
        return plan.ToJsonString();
    }
    private static string CompleteSourceEvidence(CanvasAiRequest input, string content)
    {
        var plan = JsonNode.Parse(content) as JsonObject;
        if (plan == null || plan["html"] is not JsonValue value || !value.TryGetValue<string>(out var html) ||
            html == null || !TrySourceBackedClassEdit(input, html)) return content;
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(html ?? "");
        var citations = new JsonArray();
        // Nonempty is not equivalent to valid. Discard stale/invented citations
        // before adding citations derived from this request's actual source ranges.
        if (plan["sourceEvidence"] is JsonArray supplied)
            foreach (var item in supplied.Take(8))
                if (item != null && IsCurrentSourceCitation(input, JsonSerializer.SerializeToElement(item))) citations.Add(item.DeepClone());
        var queries = new List<string>();
        foreach (var node in after.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element))
        {
            if (citations.Count >= 8) break;
            var id = node.GetAttributeValue("id", "");
            var original = before.DocumentNode.Descendants().FirstOrDefault(n => id.Length > 0 && n.GetAttributeValue("id", "") == id);
            if (original == null) continue;
            var oldClasses = original.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var classes = node.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            foreach (var cls in classes.Except(oldClasses).Concat(oldClasses.Except(classes)).Take(8))
            {
                if (citations.Count >= 8) break;
                if (!Regex.IsMatch(cls, @"\A[a-zA-Z_][a-zA-Z0-9_-]{0,99}\z")) continue;
                queries.Add(cls);
                foreach (var source in input.KnowledgeSources.Where(s => s.Classes.Contains(cls)))
                {
                    var range = source.Evidence.FirstOrDefault(e => e.Text.Contains("." + cls, StringComparison.Ordinal) && e.End - e.Start <= CanvasSourceRange.MaxLineSpan);
                    if (range != null)
                    {
                        var citation = JsonSerializer.SerializeToNode(new { key = source.Key, start = range.Start, end = range.End });
                        if (!citations.Any(item => JsonNode.DeepEquals(item, citation))) citations.Add(citation);
                        break;
                    }
                }
            }
        }
        input.SourceQueries = queries.Concat(input.SourceQueries).Distinct().Take(8).ToArray();
        plan["sourceEvidence"] = citations;
        return plan.ToJsonString();
    }
    private static void UpdateSourceQueries(CanvasAiRequest input, string content)
    {
        try
        {
            using var json = JsonDocument.Parse(content);
            if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("sourceQueries", out var queries) && queries.ValueKind == JsonValueKind.Array)
                input.SourceQueries = queries.EnumerateArray().Take(8).Where(q => q.ValueKind == JsonValueKind.String)
                    .Select(q => q.GetString()!).Where(q => Regex.IsMatch(q, @"\A[a-zA-Z_][a-zA-Z0-9_-]{0,99}\z")).ToArray();
        }
        catch (JsonException) { }
    }
    private static bool IsRuntimeCarouselRoot(HtmlNode node) => node.ChildNodes.Any(child =>
        child.NodeType == HtmlNodeType.Element && child.GetAttributeValue("class", "")
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Contains("swiper"));
    private static bool IsRuntimeLayoutTarget(HtmlNode node) => IsRuntimeCarouselRoot(node) ||
        node.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Any(cls => cls is "swiper" or "swiper-wrapper" or "swiper-slide") || node.Descendants().Count(IsRuntimeCarouselRoot) == 1 ||
        (node.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Any(cls => cls is "catalog" or "template") &&
            node.Ancestors().Any(IsRuntimeCarouselRoot));

    private static bool IsRuntimeAppearanceProperty(string name) => name is
        "color" or "background-color" or "font-family" or "font-size" or "font-weight" or "font-style" or "line-height" or
        "letter-spacing" or "text-align" or "text-decoration" or "text-shadow" or "-webkit-text-stroke" or
        "-webkit-text-stroke-width" or "-webkit-text-stroke-color" or "paint-order" or "border" or "border-radius";
    private static bool HasRuntimeGeometryStyle(CanvasAiStyle rule) => rule.Properties.Keys.Any(name => !IsRuntimeAppearanceProperty(name));

    private static bool TryReadRuntimeAssertions(CanvasAiRequest input, JsonElement plan, out List<CanvasAiRuntimeAssertion> assertions)
    {
        assertions = [];
        if (!plan.TryGetProperty("runtimeAssertions", out var value)) return input.RuntimeContext == "[]";
        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() > 8) return false;
        var parsedAssertions = JsonSerializer.Deserialize<List<CanvasAiRuntimeAssertion>>(value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        if (input.ExecutionGoal is { } goal && !parsedAssertions.Any(item => item != null && item.TargetId == goal.TargetId &&
            double.TryParse(item.Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var actual) &&
            double.TryParse(goal.Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var expected) && actual == expected)) return false;
        assertions = parsedAssertions;
        var document = new HtmlDocument(); document.LoadHtml(input.Html);
        var owners = document.DocumentNode.Descendants().Where(IsRuntimeCarouselRoot).Select(node => node.GetAttributeValue("id", "")).Where(id => id.Length > 0).ToArray();
        return (input.RuntimeContext == "[]" || owners.All(id => parsedAssertions.Any(assertion => assertion?.TargetId == id))) && parsedAssertions.All(assertion => assertion != null && assertion.MinWidth is >= 0 and <= 20000 &&
            !string.IsNullOrEmpty(assertion.TargetId) && document.DocumentNode.Descendants().Any(node => IsRuntimeCarouselRoot(node) && node.GetAttributeValue("id", "") == assertion.TargetId) &&
            (assertion.Value == "auto" || double.TryParse(assertion.Value, System.Globalization.NumberStyles.AllowDecimalPoint,
                System.Globalization.CultureInfo.InvariantCulture, out var number) && double.IsFinite(number) && number > 0 && number <= 100));
    }
    private static bool HasRuntimeClassChange(string original, string proposed)
    {
        var before = new HtmlDocument(); before.LoadHtml(original);
        var after = new HtmlDocument(); after.LoadHtml(proposed);
        return before.DocumentNode.Descendants().Where(IsRuntimeCarouselRoot).Any(node =>
            after.DocumentNode.Descendants().Any(other => other.GetAttributeValue("id", "") == node.GetAttributeValue("id", "") &&
                other.GetAttributeValue("class", "") != node.GetAttributeValue("class", "")));
    }
    private static bool HasSupportedSourceEvidence(CanvasAiRequest input, JsonElement plan)
    {
        if (!plan.TryGetProperty("sourceEvidence", out var evidence) || evidence.ValueKind != JsonValueKind.Array) return false;
        return evidence.EnumerateArray().Take(8).Any(item => IsCurrentSourceCitation(input, item));
    }
    private static bool IsCurrentSourceCitation(CanvasAiRequest input, JsonElement item) => item.ValueKind == JsonValueKind.Object &&
            item.TryGetProperty("key", out var key) && key.ValueKind == JsonValueKind.String &&
            item.TryGetProperty("start", out var start) && start.ValueKind == JsonValueKind.Number && start.TryGetInt32(out var first) &&
            item.TryGetProperty("end", out var end) && end.ValueKind == JsonValueKind.Number && end.TryGetInt32(out var last) && first > 0 && last >= first && last - first <= CanvasSourceRange.MaxLineSpan &&
            input.KnowledgeSources.Any(s => s.Key == key.GetString() && s.Evidence.Any(e => first >= e.Start && last <= e.End));

    private static string DescribeMissingClassEvidence(CanvasAiRequest input, string html)
    {
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(html);
        var added = after.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element).SelectMany(node => {
            var original = before.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "") == node.GetAttributeValue("id", ""));
            return node.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Except((original?.GetAttributeValue("class", "") ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }).Distinct().Take(8);
        return "\nCurrent source availability: " + string.Join("; ", added.Select(cls => {
            var sources = input.KnowledgeSources.Where(source => source.Classes.Contains(cls)).ToArray();
            return cls + ": sources=" + sources.Length + ", matching excerpts=" + sources.Sum(source => source.Evidence.Count(range =>
                range.End - range.Start <= CanvasSourceRange.MaxLineSpan && range.Text.Contains("." + cls, StringComparison.Ordinal)));
        }));
    }
    // Existing arbitrary metadata is retained byte-for-value; only source-backed class
    // changes are permitted by this exception. It never admits new scripts/attributes.
    internal static bool TrySourceBackedClassEdit(CanvasAiRequest input, string proposed)
        => TrySourceBackedClassEdit(input, proposed, out _);

    internal static bool TrySourceBackedClassEdit(CanvasAiRequest input, string proposed, out string failure)
    {
        failure = "";
        if (input.KnowledgeClasses.Count == 0) { failure = "No deployed source classes were available."; return false; }
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(proposed);
        var changed = false;
        var reason = "";
        bool Compare(HtmlNode a, HtmlNode b)
        {
            var target = a.GetAttributeValue("id", "");
            if (a.NodeType != b.NodeType || a.Name != b.Name) { reason = $"Node type/tag changed near {a.Name} #{target}."; return false; }
            if (a.ChildNodes.Count != b.ChildNodes.Count) { reason = $"Child node count changed near {a.Name} #{target} (including formatting whitespace)."; return false; }
            if (a.NodeType is HtmlNodeType.Text or HtmlNodeType.Comment)
            {
                if (a.InnerHtml == b.InnerHtml) return true;
                reason = "Text, comment or formatting whitespace changed outside class tokens."; return false;
            }
            string Attributes(HtmlNode node) => JsonSerializer.Serialize(node.Attributes.Where(attr => attr.Name != "class")
                .OrderBy(attr => attr.Name, StringComparer.Ordinal).Select(attr => new { attr.Name, attr.Value }));
            if (Attributes(a) != Attributes(b)) { reason = $"Non-class attributes changed near {a.Name} #{target}."; return false; }
            var oldClasses = a.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
            var newClasses = b.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
            if (!oldClasses.SetEquals(newClasses))
            {
                if (newClasses.Count > 100 || newClasses.Any(c => !Regex.IsMatch(c, @"\A[a-zA-Z_][a-zA-Z0-9_-]{0,99}\z")) ||
                    newClasses.Except(oldClasses).Any(c => !input.KnowledgeClasses.Contains(c)))
                { reason = $"Invalid or undeployed new class near {a.Name} #{target}."; return false; }
                changed = true;
            }
            return a.ChildNodes.Zip(b.ChildNodes).All(pair => Compare(pair.First, pair.Second));
        }
        var valid = Compare(before.DocumentNode, after.DocumentNode) && changed;
        failure = valid ? "" : reason.Length > 0 ? reason : "No class token difference was found.";
        return valid;
    }

}
