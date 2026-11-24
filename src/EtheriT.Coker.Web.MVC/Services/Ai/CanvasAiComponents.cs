using HtmlAgilityPack;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public sealed partial class CanvasAiService
{
    private static string CompactClassReview(CanvasAiRequest input, string content, string? html)
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
        plan["ReviewInstructions"] = "Review these exact class changes against original Html and current ResourceKnowledge. Return the complete original HTML with only the verified class changes, preserving all other nodes, attributes and text.";
        return plan.ToJsonString();
    }
    private static string CompleteSourceEvidence(CanvasAiRequest input, string content)
    {
        var plan = JsonNode.Parse(content) as JsonObject;
        if (plan == null || plan["sourceEvidence"] is JsonArray { Count: > 0 } || plan["html"] is not JsonValue value || !value.TryGetValue<string>(out var html)) return content;
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(html ?? "");
        var citations = new JsonArray();
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
                    var range = source.Evidence.FirstOrDefault(e => e.Text.Contains("." + cls, StringComparison.Ordinal) && e.End - e.Start <= 100);
                    if (range != null) { citations.Add(JsonSerializer.SerializeToNode(new { key = source.Key, start = range.Start, end = range.End })); break; }
                }
            }
        }
        input.SourceQueries = input.SourceQueries.Concat(queries).Distinct().Take(8).ToArray();
        if (citations.Count == 0) return content;
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
        return evidence.EnumerateArray().Take(8).Any(item => item.ValueKind == JsonValueKind.Object &&
            item.TryGetProperty("key", out var key) && key.ValueKind == JsonValueKind.String &&
            item.TryGetProperty("start", out var start) && start.ValueKind == JsonValueKind.Number && start.TryGetInt32(out var first) &&
            item.TryGetProperty("end", out var end) && end.ValueKind == JsonValueKind.Number && end.TryGetInt32(out var last) && first > 0 && last >= first && last - first <= 100 &&
            input.KnowledgeSources.Any(s => s.Key == key.GetString() && s.Evidence.Any(e => first >= e.Start && last <= e.End)));
    }
    // Existing arbitrary metadata is retained byte-for-value; only source-backed class
    // changes are permitted by this exception. It never admits new scripts/attributes.
    internal static bool TrySourceBackedClassEdit(CanvasAiRequest input, string proposed)
    {
        if (input.KnowledgeClasses.Count == 0) return false;
        var before = new HtmlDocument(); before.LoadHtml(input.Html);
        var after = new HtmlDocument(); after.LoadHtml(proposed);
        var changed = false;
        bool Compare(HtmlNode a, HtmlNode b)
        {
            if (a.NodeType != b.NodeType || a.Name != b.Name || a.ChildNodes.Count != b.ChildNodes.Count) return false;
            if (a.NodeType is HtmlNodeType.Text or HtmlNodeType.Comment) return a.InnerHtml == b.InnerHtml;
            string Attributes(HtmlNode node) => JsonSerializer.Serialize(node.Attributes.Where(attr => attr.Name != "class")
                .OrderBy(attr => attr.Name, StringComparer.Ordinal).Select(attr => new { attr.Name, attr.Value }));
            if (Attributes(a) != Attributes(b)) return false;
            var oldClasses = a.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
            var newClasses = b.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
            if (!oldClasses.SetEquals(newClasses))
            {
                if (newClasses.Count > 100 || newClasses.Any(c => !Regex.IsMatch(c, @"\A[a-zA-Z_][a-zA-Z0-9_-]{0,99}\z")) ||
                    newClasses.Except(oldClasses).Any(c => !input.KnowledgeClasses.Contains(c))) return false;
                changed = true;
            }
            return a.ChildNodes.Zip(b.ChildNodes).All(pair => Compare(pair.First, pair.Second));
        }
        return Compare(before.DocumentNode, after.DocumentNode) && changed;
    }

}
