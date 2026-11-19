using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.SiteCrawl;

// 確認畫面上的一個舊站主選單項目。Key＝從第一層到自己的標題路徑；偵測與匯入都重新讀主選單，用 Key 對回同一項。
// ListKey：這頁就是偵測到的列表時填列表 key，列表類型步驟用它對回選單。
// RouterName：不改時匯入後的路徑名稱；ExistingRoute＝沿用網站既有選單的路徑名稱；Duplicate＝和上方選單同一頁，匯入時不另建。
// Section＝左欄子頁；LinkToParent＝左欄子頁就是上層那一頁，匯入成連到上層的選單，沒有自己的路徑名稱。
// Category＝分類頁的子分類（舊站用 JS 列出的子分類卡片），匯入成不顯示於主選單、但選單目錄列得出來的選單。
public sealed record SiteMenuCandidate(string Key, string Title, string? Url, bool External, bool IsHome,
    string? ListKey, IReadOnlyList<SiteMenuCandidate> Children, string? RouterName = null, bool ExistingRoute = false,
    bool Duplicate = false, bool Section = false, bool LinkToParent = false, bool Category = false);

// 偵測時推算的路徑名稱（SiteMenuImporter.SuggestRoutesAsync）。
internal sealed record SiteMenuRoute(string? Name, bool Existing, bool Duplicate, bool LinkToParent = false);

// 偵測時從主選單各頁讀到的子頁：Sections＝左欄子頁、Categories＝子分類樹，都以所屬頁面的 PageKey 為索引。
internal sealed record SitePageExtras(
    IReadOnlyDictionary<string, IReadOnlyList<SiteMenuLink>> Sections,
    IReadOnlyDictionary<string, IReadOnlyList<SiteMenuLink>> Categories);

// Include＝是否匯入，不匯入時整支略過；RouterName＝路徑名稱，空白＝沿用既有選單或自動產生。
public sealed record SiteMenuChoice(string Key, bool Include, string? RouterName);

// 匯入前確認畫面的主選單選擇（使用者 2026-11-18 要求：列出舊站選單結構、可選要匯入的頁面與改路徑名稱；
// 左欄子頁也要列出、可改路徑名稱）。
public static partial class SiteMenuSelection
{
    private const string KeySeparator = "\u001f";

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9_-]{0,49}$")]
    private static partial Regex RouterNamePattern();

    // 前台網址是 /{OrgName}/{RouterName}：只收英數與 - _，Home 保留給首頁。
    public static bool IsValidRouterName(string value) =>
        RouterNamePattern().IsMatch(value) && !value.Equals("Home", StringComparison.OrdinalIgnoreCase);

    internal static string Key(string? parentKey, string title) =>
        parentKey == null ? title.Trim() : parentKey + KeySeparator + title.Trim();

    // 把左欄子頁接到主選單項目底下（只給確認畫面用），條件和 SiteMenuImporter.ImportSectionMenusAsync 相同：
    // 只接沒有下拉的一般頁面；左欄列出其他主選單的頁面（兄弟清單）、已被前面的項目接走、只有自己、
    // 或自己已在下拉裡又出現在左欄時都不接。和前面選單同網址的項目匯入時不建，也不接。
    internal static IReadOnlyList<SiteMenuLink> AttachSections(IReadOnlyList<SiteMenuLink> links, Uri entry,
        IReadOnlyDictionary<string, IReadOnlyList<SiteMenuLink>> sections)
    {
        if (sections.Count == 0) return links;
        var host = entry.IdnHost;
        var home = SiteMenuImporter.FindHomeLink(links, entry);
        var menuKeys = new HashSet<string>(StringComparer.Ordinal) { SiteLayoutScanner.PageKey(entry) };
        CollectKeys(links, host, menuKeys);
        var claimed = new HashSet<string>(StringComparer.Ordinal);
        var seenUrls = new HashSet<string>(StringComparer.Ordinal);
        return AttachLevel(links, false);

        List<SiteMenuLink> AttachLevel(IReadOnlyList<SiteMenuLink> level, bool hasParent)
        {
            var output = new List<SiteMenuLink>();
            foreach (var link in level)
            {
                var local = link.Url != null && SiteCrawlHttp.IsAllowed(link.Url, host);
                var ownKey = local ? SiteLayoutScanner.PageKey(link.Url!) : null;
                var duplicate = ownKey != null && !seenUrls.Add(ownKey);
                if (link.Children.Count > 0)
                {
                    output.Add(link with { Children = AttachLevel(link.Children, true) });
                    continue;
                }
                if (ownKey == null || duplicate || ReferenceEquals(link, home)
                    || !sections.TryGetValue(ownKey, out var section) || section.Count == 0)
                {
                    output.Add(link);
                    continue;
                }
                var keys = section.Select(item => SiteLayoutScanner.PageKey(item.Url!)).ToList();
                if (keys.Any(key => key != ownKey && menuKeys.Contains(key)) || keys.Any(claimed.Contains)
                    || (hasParent && keys.Contains(ownKey)) || keys.All(key => key == ownKey))
                {
                    output.Add(link);
                    continue;
                }
                claimed.UnionWith(keys);
                output.Add(link with { Children = section.Select(item => item with { IsSection = true }).ToList() });
            }
            return output;
        }
    }

    // 把分類頁的子分類樹接到該頁的選單底下（只給確認畫面用）。匯入時子分類是隱藏選單（ImportHiddenPagesAsync），
    // 依麵包屑放在同名的主選單下（例 產品資訊 → 乙太網路光電轉換器 → 10/100M乙太網路），所以 Key 和這裡接出來的路徑一致。
    // 同名的子項目已經在下面時不重複接。
    internal static IReadOnlyList<SiteMenuLink> AttachCategories(IReadOnlyList<SiteMenuLink> links, Uri entry,
        IReadOnlyDictionary<string, IReadOnlyList<SiteMenuLink>> categories)
    {
        if (categories.Count == 0) return links;
        var host = entry.IdnHost;
        var used = new HashSet<string>(StringComparer.Ordinal);
        return AttachLevel(links);

        List<SiteMenuLink> AttachLevel(IReadOnlyList<SiteMenuLink> level) => level.Select(link =>
        {
            IReadOnlyList<SiteMenuLink> children = AttachLevel(link.Children);
            if (link.Url != null && SiteCrawlHttp.IsAllowed(link.Url, host)
                && used.Add(SiteLayoutScanner.PageKey(link.Url))
                && categories.TryGetValue(SiteLayoutScanner.PageKey(link.Url), out var found))
            {
                var titles = children.Select(child => child.Title.Trim()).ToHashSet(StringComparer.Ordinal);
                children = [.. children, .. found.Where(child => !titles.Contains(child.Title.Trim()))];
            }
            return link with { Children = children };
        }).ToList();
    }

    private static void CollectKeys(IReadOnlyList<SiteMenuLink> links, string host, HashSet<string> keys)
    {
        foreach (var link in links)
        {
            if (link.Url != null && SiteCrawlHttp.IsAllowed(link.Url, host)) keys.Add(SiteLayoutScanner.PageKey(link.Url));
            CollectKeys(link.Children, host, keys);
        }
    }

    internal static List<SiteMenuCandidate> Describe(IReadOnlyList<SiteMenuLink> links, Uri entry, ISet<string> listKeys,
        IReadOnlyDictionary<string, SiteMenuRoute> routes) =>
        DescribeLevel(links, entry.IdnHost, SiteMenuImporter.FindHomeLink(links, entry), listKeys, routes, null);

    // 同一層同名的項目匯入時只會建第一個（CreateChildrenAsync 依標題去重），畫面也只列第一個。
    private static List<SiteMenuCandidate> DescribeLevel(IReadOnlyList<SiteMenuLink> links, string host,
        SiteMenuLink? home, ISet<string> listKeys, IReadOnlyDictionary<string, SiteMenuRoute> routes, string? parentKey)
    {
        var result = new List<SiteMenuCandidate>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var link in links)
        {
            var key = Key(parentKey, link.Title);
            if (!seen.Add(key)) continue;
            var local = link.Url != null && SiteCrawlHttp.IsAllowed(link.Url, host);
            var listKey = local ? SiteListDetector.ListKey(link.Url!) : null;
            var route = routes.GetValueOrDefault(key);
            result.Add(new SiteMenuCandidate(key, link.Title, link.Url?.AbsoluteUri, link.Url != null && !local,
                ReferenceEquals(link, home), listKey != null && listKeys.Contains(listKey) ? listKey : null,
                DescribeLevel(link.Children, host, home, listKeys, routes, key),
                route?.Name, route?.Existing ?? false, route?.Duplicate ?? false, link.IsSection, route?.LinkToParent ?? false,
                link.IsCategory));
        }
        return result;
    }

    // 依使用者的選擇過濾主選單：不匯入的整支拿掉並記下頁面；確認畫面沒有的項目（偵測後舊站改了）照常匯入。
    // 同一頁同時出現在要匯入的項目裡（例 上層和第一個子頁同網址）時不算排除。
    internal static IReadOnlyList<SiteMenuLink> Apply(IReadOnlyList<SiteMenuLink> links, SiteLayoutResult result)
    {
        if (result.MenuChoices.Count == 0) return links;
        var excluded = new HashSet<string>(StringComparer.Ordinal);
        var included = new HashSet<string>(StringComparer.Ordinal);
        var kept = ApplyLevel(links, result, null, excluded, included);
        excluded.ExceptWith(included);
        result.ExcludedPageKeys.UnionWith(excluded);
        return kept;
    }

    // 隱藏選單樹（含分類頁的子分類，ImportHiddenPagesAsync 依麵包屑排出）套用確認畫面的勾選與路徑名稱。
    // 樹根是麵包屑上的名稱，和同名的主選單是同一個選單，所以 Key 從樹根算起就和確認畫面一致。
    internal static IReadOnlyList<SiteMenuLink> ApplyHidden(IReadOnlyList<SiteMenuLink> links, SiteLayoutResult result)
    {
        if (result.MenuChoices.Count == 0) return links;
        var excluded = new HashSet<string>(StringComparer.Ordinal);
        var kept = ApplyLevel(links, result, null, excluded, new HashSet<string>(StringComparer.Ordinal));
        result.ExcludedPageKeys.UnionWith(excluded);
        return kept;
    }

    // 左欄子頁（匯入時才由掃描結果得知）套用確認畫面的勾選與路徑名稱；parentKey＝上層主選單在確認畫面的 Key。
    // 和上層同一頁的子頁取消勾選時，上層仍要匯入，所以不記成排除頁面。
    internal static IReadOnlyList<SiteMenuLink> ApplySection(IReadOnlyList<SiteMenuLink> links, string parentKey,
        string ownKey, SiteLayoutResult result)
    {
        if (result.MenuChoices.Count == 0) return links;
        var kept = new List<SiteMenuLink>();
        foreach (var link in links)
        {
            var key = Key(parentKey, link.Title);
            result.MenuChoices.TryGetValue(key, out var choice);
            if (choice is { Include: false })
            {
                result.MenusExcluded++;
                var pageKey = SiteLayoutScanner.PageKey(link.Url!);
                if (pageKey != ownKey) result.ExcludedPageKeys.Add(pageKey);
                continue;
            }
            kept.Add(link with { RouterName = RouterNameOf(choice), SelectionKey = key });
        }
        return kept;
    }

    private static List<SiteMenuLink> ApplyLevel(IReadOnlyList<SiteMenuLink> links, SiteLayoutResult result,
        string? parentKey, HashSet<string> excluded, HashSet<string> included)
    {
        var kept = new List<SiteMenuLink>();
        foreach (var link in links)
        {
            var key = Key(parentKey, link.Title);
            result.MenuChoices.TryGetValue(key, out var choice);
            if (choice is { Include: false })
            {
                Exclude(link, excluded, result);
                continue;
            }
            if (link.Url != null) included.Add(SiteLayoutScanner.PageKey(link.Url));
            kept.Add(link with
            {
                RouterName = RouterNameOf(choice),
                SelectionKey = key,
                Children = ApplyLevel(link.Children, result, key, excluded, included)
            });
        }
        return kept;
    }

    private static string? RouterNameOf(SiteMenuChoice? choice) =>
        choice is { RouterName: { } name } && !string.IsNullOrWhiteSpace(name) ? name.Trim() : null;

    private static void Exclude(SiteMenuLink link, HashSet<string> excluded, SiteLayoutResult result)
    {
        result.MenusExcluded++;
        if (link.Url != null) excluded.Add(SiteLayoutScanner.PageKey(link.Url));
        foreach (var child in link.Children) Exclude(child, excluded, result);
    }
}
