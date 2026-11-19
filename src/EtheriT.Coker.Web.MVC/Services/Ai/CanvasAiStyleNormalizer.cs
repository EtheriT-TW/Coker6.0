using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

// Resolve static CSS targets only inside the supplied fragment; never publish global class/tag rules.
internal static class CanvasAiStyleNormalizer
{
    internal static bool TryNormalize(List<CanvasAiStyle>? rules, string html, out string normalizedHtml, out List<CanvasAiStyle> normalized)
    {
        normalizedHtml = html;
        normalized = [];
        if (rules == null || rules.Count > 100) return false;
        if (rules.Count == 0) return true;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var ids = doc.DocumentNode.Descendants().Select(n => n.GetAttributeValue("id", "")).Where(id => id.Length > 0).ToList();
        if (ids.Count != ids.Distinct(StringComparer.Ordinal).Count()) return false;
        var changes = new Dictionary<HtmlNode, Dictionary<string, (string Value, int Specificity)>>();
        foreach (var rule in rules)
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.Selector) || rule.Selector.Length > 500 || rule.Properties == null ||
                rule.Properties.Count is < 1 or > 40) return false;
            foreach (var selector in rule.Selector.Split(','))
            {
                if (!TryXPath(selector, out var xpath, out var specificity)) return false;
                var matches = doc.DocumentNode.SelectNodes(xpath);
                if (matches == null || matches.Count == 0) return false;
                foreach (var node in matches)
                {
                    if (!changes.TryGetValue(node, out var properties)) changes[node] = properties = new(StringComparer.Ordinal);
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
        return true;
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
