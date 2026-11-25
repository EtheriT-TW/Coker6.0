using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

// Resolve static CSS targets only inside the supplied fragment; never publish global class/tag rules.
internal static class CanvasAiStyleNormalizer
{
    internal static bool TryNormalize(List<CanvasAiStyle>? rules, string html, out string normalizedHtml, out List<CanvasAiStyle> normalized)
        => TryNormalize(rules, html, out normalizedHtml, out normalized, out _);

    internal static bool TryNormalize(List<CanvasAiStyle>? rules, string html, out string normalizedHtml, out List<CanvasAiStyle> normalized, out string failure)
    {
        failure = "targets-rule-limit";
        normalizedHtml = html;
        normalized = [];
        if (rules == null || rules.Count > 100) return false;
        if (rules.Count == 0) return true;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var ids = doc.DocumentNode.Descendants().Select(n => n.GetAttributeValue("id", "")).Where(id => id.Length > 0).ToList();
        failure = "targets-duplicate-ids";
        if (ids.Count != ids.Distinct(StringComparer.Ordinal).Count()) return false;
        var changes = new Dictionary<HtmlNode, Dictionary<string, (string Value, int Specificity)>>();
        var directoryRules = new Dictionary<string, CanvasAiStyle>(StringComparer.Ordinal);
        foreach (var rule in rules)
        {
            failure = "targets-rule-format";
            if (rule == null || string.IsNullOrWhiteSpace(rule.Selector) || rule.Selector.Length > 500 || rule.Properties == null ||
                rule.Properties.Count is < 1 or > 40) return false;
            foreach (var selector in rule.Selector.Split(','))
            {
                // Runtime directory cards are cloned after page load. Keep this narrowly
                // scoped structural selector rather than converting it to a template's ID.
                if (IsDirectoryItemsSelector(selector.Trim(), html))
                {
                    var key = selector.Trim();
                    if (!directoryRules.TryGetValue(key, out var directoryRule))
                        directoryRules[key] = directoryRule = new() { Selector = key };
                    foreach (var property in rule.Properties) directoryRule.Properties[property.Key] = property.Value;
                    continue;
                }
                failure = "targets-selector-format";
                if (!TryXPath(selector, out var xpath, out var specificity)) return false;
                var matches = doc.DocumentNode.SelectNodes(xpath);
                // AI may copy a page-level ancestor path from CSS. The request contains
                // only a fragment; resolve its suffix only when anchored to an existing
                // unique ID or the fragment root, never to arbitrary inner class matches.
                if (matches == null || matches.Count == 0)
                    matches = ResolveFragmentSuffix(doc, selector);
                failure = "targets-unmatched";
                if (matches == null || matches.Count == 0) return false;
                foreach (var node in matches)
                {
                    if (!changes.TryGetValue(node, out var properties)) changes[node] = properties = new(StringComparer.Ordinal);
                    failure = "targets-match-limit";
                    if (changes.Count > 100) return false;
                    foreach (var property in rule.Properties)
                    {
                        if (!properties.TryGetValue(property.Key, out var previous) || specificity >= previous.Specificity)
                            properties[property.Key] = (property.Value, specificity);
                    }
                }
            }
        }
        var assignedIds = false;
        foreach (var change in changes)
        {
            var id = change.Key.GetAttributeValue("id", "");
            if (id.Length == 0)
            {
                do { id = "ai-style-" + Guid.NewGuid().ToString("N"); } while (ids.Contains(id));
                ids.Add(id);
                change.Key.SetAttributeValue("id", id);
                assignedIds = true;
            }
            normalized.Add(new CanvasAiStyle { Selector = "#" + id,
                Properties = change.Value.ToDictionary(p => p.Key, p => p.Value.Value, StringComparer.Ordinal) });
        }
        if (assignedIds) normalizedHtml = doc.DocumentNode.InnerHtml;
        normalized.AddRange(directoryRules.Values);
        if (normalized.Count > 100) { failure = "targets-rule-limit"; return false; }
        failure = "";
        return true;
    }

    internal static bool IsDirectoryItemsSelector(string selector, string html)
    {
        var match = Regex.Match(selector, @"\A#([a-zA-Z_][a-zA-Z0-9_-]*) > \.template\z");
        if (!match.Success) return false;
        var doc = new HtmlDocument(); doc.LoadHtml(html);
        var matches = doc.DocumentNode.Descendants().Where(n => n.GetAttributeValue("id", "") == match.Groups[1].Value).ToList();
        static bool HasClass(HtmlNode n, string c) => n.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Contains(c);
        return matches.Count == 1 && HasClass(matches[0], "catalog") && matches[0].Ancestors().Any(n => HasClass(n, "catalog_frame"));
    }

    private static HtmlNodeCollection? ResolveFragmentSuffix(HtmlDocument doc, string selector)
    {
        var parts = Regex.Split(selector.Trim(), @"\s*(>)\s*|\s+").Where(p => p.Length > 0).ToArray();
        for (var start = 1; start < parts.Length; start++)
        {
            // Only remove a wholly external prefix. A broken path below an already
            // matched local element is an error, not permission to skip that element.
            if (parts.Take(start).Any(part => part != ">" && TryXPath(part, out var prefixPath, out _) &&
                doc.DocumentNode.SelectNodes(prefixPath) is { Count: > 0 })) continue;
            if (parts[start] == ">" || !TryXPath(parts[start], out var anchorPath, out _)) continue;
            var anchors = doc.DocumentNode.SelectNodes(anchorPath);
            if (anchors == null || anchors.Count != 1) continue;
            var anchor = anchors[0];
            var hasId = Regex.IsMatch(parts[start], @"#[a-zA-Z_][a-zA-Z0-9_-]*");
            if (!hasId && anchor.ParentNode != doc.DocumentNode) continue;
            if (!TryXPath(string.Join(" ", parts.Skip(start)), out var localPath, out _)) continue;
            var matches = doc.DocumentNode.SelectNodes(localPath);
            if (matches != null && matches.Count > 0) return matches;
        }
        return null;
    }

    private static bool TryXPath(string selector, out string xpath, out int specificity)
    {
        xpath = "";
        specificity = 0;
        var parts = Regex.Split(selector.Trim(), @"\s*(>)\s*|\s+").Where(p => p.Length > 0).ToArray();
        var child = false;
        var hasPart = false;
        foreach (var part in parts)
        {
            if (part == ">")
            {
                if (!hasPart || child) return false;
                child = true;
                continue;
            }
            if (part == ":scope" && !hasPart)
            {
                xpath = "/*"; // Fragment roots only; never the surrounding page.
                specificity += 1000;
                hasPart = true;
                continue;
            }
            if (!Regex.IsMatch(part, @"\A(?:\*|[a-zA-Z][a-zA-Z0-9_-]*)?(?:[.#][a-zA-Z_][a-zA-Z0-9_-]*)*\z")) return false;
            var tag = Regex.Match(part, @"\A(?:\*|[a-zA-Z][a-zA-Z0-9_-]*)");
            var name = tag.Success ? tag.Value.ToLowerInvariant() : "*";
            xpath += child ? "/" : "//";
            xpath += name;
            if (name != "*") specificity++;
            foreach (Match token in Regex.Matches(part, @"([.#])([a-zA-Z_][a-zA-Z0-9_-]*)"))
            {
                var value = token.Groups[2].Value;
                var isId = token.Groups[1].Value == "#";
                // The selector length bound keeps each lower-order component below its next tier.
                specificity += isId ? 1_000_000 : 1000;
                xpath += isId ? $"[@id='{value}']" : $"[contains(concat(' ', normalize-space(@class), ' '), ' {value} ')]";
            }
            hasPart = true;
            child = false;
        }
        return hasPart && !child;
    }
}
