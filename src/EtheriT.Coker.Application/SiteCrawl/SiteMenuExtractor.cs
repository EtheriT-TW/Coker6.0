using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

// IsCategory：舊站分類頁子分類列表裡的一項（主選單沒有），建成「不顯示於主選單」但選單目錄列得出來的選單。
// RouterName：使用者在確認畫面填的路徑名稱（null＝沿用既有或自動產生）。
// SelectionKey：確認畫面上的 Key（SiteMenuSelection.Apply 填），匯入左欄子頁時用來找它們的選擇。
// IsSection：偵測時接到主選單項目底下的左欄子頁（只給確認畫面用，匯入時左欄子頁另外由 ImportSectionMenusAsync 建）。
internal sealed record SiteMenuLink(string Title, Uri? Url, bool OpensNewTab, IReadOnlyList<SiteMenuLink> Children,
    bool IsCategory = false, string? RouterName = null, string? SelectionKey = null, bool IsSection = false);

internal static class SiteMenuExtractor
{
    private const string Navigation = "//nav | //*[@role='navigation'] | //*[" +
        "contains(concat(' ',normalize-space(@class),' '),' aumenu ') or @id='aumenu' or " +
        "contains(concat(' ',normalize-space(@class),' '),' main-menu ') or @id='main-menu' or " +
        "contains(concat(' ',normalize-space(@class),' '),' navbar-nav ')]";

    internal static IReadOnlyList<SiteMenuLink> Extract(HtmlDocument document, Uri url)
    {
        var root = document.DocumentNode;
        var candidates = root.SelectNodes(Navigation) ?? Enumerable.Empty<HtmlNode>();
        var navigation = candidates.Where(node => !node.Ancestors().Any(IsPeripheral))
            .Select(node => ReadNavigation(node, url)).OrderByDescending(items => items.Count).FirstOrDefault();
        if (navigation is { Count: > 0 }) return navigation;
        var header = root.SelectSingleNode("//header[not(ancestor::article or ancestor::main)]");
        return header == null ? [] : ReadNavigation(header, url);
    }

    private static bool IsPeripheral(HtmlNode node) =>
        node.Name is "aside" or "footer" || node.GetAttributeValue("role", "") == "contentinfo";

    private static IReadOnlyList<SiteMenuLink> ReadNavigation(HtmlNode navigation, Uri url)
    {
        var items = navigation.Descendants("li")
            .Where(item => !item.Ancestors().TakeWhile(parent => parent != navigation).Any(parent => parent.Name == "li"))
            .ToList();
        return items.Count > 0
            ? ReadList(items, url, 0)
            : navigation.Descendants("a").Select(anchor => ReadAnchor(anchor, url, []))
                .OfType<SiteMenuLink>().DistinctBy(item => (item.Title, item.Url)).ToList();
    }

    private static List<SiteMenuLink> ReadList(IEnumerable<HtmlNode> items, Uri url, int depth)
    {
        if (depth >= 12) return [];
        var result = new List<SiteMenuLink>();
        foreach (var item in items)
        {
            var children = ReadList(item.Descendants("li")
                .Where(child => child.Ancestors("li").FirstOrDefault() == item), url, depth + 1);
            var anchor = item.Descendants().FirstOrDefault(node => node.Name is "a" or "button"
                && node.Ancestors("li").FirstOrDefault() == item);
            if (anchor != null && ReadAnchor(anchor, url, children) is { } link) result.Add(link);
            else if (children.Count > 0) result.AddRange(children);
        }
        return result.DistinctBy(item => (item.Title, item.Url)).ToList();
    }

    private static SiteMenuLink? ReadAnchor(HtmlNode anchor, Uri pageUrl, IReadOnlyList<SiteMenuLink> children)
    {
        var title = Regex.Replace(HtmlEntity.DeEntitize(anchor.InnerText), @"\s+", " ").Trim();
        if (title.Length == 0) title = HtmlEntity.DeEntitize(anchor.GetAttributeValue("aria-label", "")).Trim();
        if (title.Length == 0 || title.All(character => char.IsPunctuation(character) || char.IsSymbol(character)))
            return null;
        var href = HtmlEntity.DeEntitize(anchor.GetAttributeValue("href", "")).Trim();
        Uri? target = null;
        if (href.Length > 0 && !href.StartsWith('#') && Uri.TryCreate(pageUrl, href, out var parsed) &&
            parsed.Scheme is "https" or "http" && parsed.UserInfo.Length == 0)
            target = parsed;
        return target == null && children.Count == 0
            ? null : new SiteMenuLink(title, target, anchor.GetAttributeValue("target", "") == "_blank", children);
    }
}
