using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed record SiteArticleInfo(string Key, string Title);
// ImageUrl／Description 取自列表項目：訊息公告是 .album_img img、#myNewsConten；職缺是 img.mainImg、.imgCont。
internal sealed record SiteArticleItem(string Key, Uri Url, DateTime? Date, string? ImageUrl = null, string? Description = null);
internal sealed record SiteArticleList(string Name, IReadOnlyList<SiteArticleItem> Items);

// 舊站（同一套舊 Cocker）的文章判斷，2026-11-11 對 988job.com.tw 實測：
// 詳細頁＝#cont 的 menu_id 有值且有「上一則／回列表頁／下一則」(#list_ctrl)；只看 menu_id 不夠，首頁加 id 參數也會有值。
// 列表頁＝#cont 裡有連到詳細頁的 a.newsItem（訊息公告型）或 a.prod_link（職缺型，JS 捲動載入）。
// 同一篇從不同列表點進去 au_id 不同，但 id 相同、內容相同，所以用 id／menu_id 當文章識別。
// 列表卡片也可能連到只有 sub_id 的子分類頁（2026-11-16 ayawan「同天入住」→「台東慢活」）：
// 目標頁本身不是列表時當成文章（識別碼加 sub: 前綴，避免和 id 撞號）；是列表就是真正的子分類，不當文章（使用者決定）。
internal static partial class SiteArticleDetector
{
    private const string SubPagePrefix = "sub:";
    private const string Container = "//*[@id='cont']";
    private const string ArticleLinks =
        ".//a[@href][contains(concat(' ',normalize-space(@class),' '),' newsItem ') or contains(concat(' ',normalize-space(@class),' '),' prod_link ')]";
    private const string TitleNode = ".//*[contains(concat(' ',normalize-space(@class),' '),' menu_title ')]";
    private const string DateNode = ".//*[contains(concat(' ',normalize-space(@class),' '),' menu_date ')]";
    private const string ImageNode =
        ".//*[contains(concat(' ',normalize-space(@class),' '),' album_img ')]//img | .//img[contains(concat(' ',normalize-space(@class),' '),' mainImg ')]";
    private const string DescriptionNode =
        ".//*[@id='myNewsConten'] | .//*[contains(concat(' ',normalize-space(@class),' '),' imgCont ')]";
    private const string BreadcrumbNode =
        "//*[@id='road'] | //nav[contains(translate(@aria-label,'BREADCRUMB','breadcrumb'),'breadcrumb')] | //*[contains(concat(' ',normalize-space(@class),' '),' breadcrumb ')]";
    private const int MaxTitleLength = 150;
    private const int MaxListNameLength = 50;

    [GeneratedRegex(@"(?<y>\d{4})\s*[/.\-年]\s*(?<m>\d{1,2})\s*[/.\-月]\s*(?<d>\d{1,2})")]
    private static partial Regex DatePattern();

    internal static SiteArticleInfo? ReadArticle(HtmlDocument document)
    {
        var container = document.DocumentNode.SelectSingleNode(Container);
        var key = container?.GetAttributeValue("menu_id", "").Trim();
        if (string.IsNullOrEmpty(key) || container!.SelectSingleNode(".//*[@id='list_ctrl']") == null) return null;
        var title = Text(container.SelectSingleNode(TitleNode)) ?? PageName(document) ?? key;
        return new SiteArticleInfo(key, Truncate(title, MaxTitleLength));
    }

    internal static SiteArticleList? ReadList(HtmlDocument document, Uri pageUrl, string host)
    {
        var links = document.DocumentNode.SelectSingleNode(Container)?.SelectNodes(ArticleLinks);
        if (links == null) return null;
        var items = new Dictionary<string, SiteArticleItem>(StringComparer.Ordinal);
        foreach (var link in links)
        {
            if (!Uri.TryCreate(pageUrl, HtmlEntity.DeEntitize(link.GetAttributeValue("href", "")), out var target)
                || !SiteCrawlHttp.IsAllowed(target, host)
                || ArticleKey(target) is not { } key
                || SiteLayoutScanner.PageKey(target) == SiteLayoutScanner.PageKey(pageUrl))
                continue;
            var item = new SiteArticleItem(key, target, ParseDate(Text(link.SelectSingleNode(DateNode))),
                ImageUrl(link, pageUrl), Text(link.SelectSingleNode(DescriptionNode)));
            items[key] = items.TryGetValue(key, out var existing)
                ? existing with
                {
                    Date = existing.Date ?? item.Date,
                    ImageUrl = existing.ImageUrl ?? item.ImageUrl,
                    Description = existing.Description ?? item.Description
                }
                : item;
        }
        if (items.Count == 0) return null;
        return new SiteArticleList(Truncate(PageName(document) ?? pageUrl.PathAndQuery, MaxListNameLength), items.Values.ToList());
    }

    private static string? ImageUrl(HtmlNode link, Uri pageUrl)
    {
        var image = link.SelectSingleNode(ImageNode);
        var src = image == null ? "" : HtmlEntity.DeEntitize(SiteCrawlHtml.ImageSource(image)).Trim();
        return src.Length > 0 && !src.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            && Uri.TryCreate(pageUrl, src, out var url) && url.Scheme is "http" or "https"
            ? url.AbsoluteUri : null;
    }

    // 麵包屑的上層名稱（不含首頁與本頁），例如舊站 #road「HOME > 地區類別專區 > 三民區/左營區」回傳 ["地區類別專區"]。
    internal static IReadOnlyList<string> ReadBreadcrumb(HtmlDocument document, Uri pageUrl, string host)
    {
        var trail = document.DocumentNode.SelectSingleNode(BreadcrumbNode)?.SelectNodes(".//a[@href]");
        if (trail == null || trail.Count < 2) return [];
        var self = SiteLayoutScanner.PageKey(pageUrl);
        var title = PageName(document);
        var names = new List<string>();
        foreach (var crumb in trail.Skip(1))
        {
            var name = Text(crumb);
            if (name == null || name == title) continue;
            if (Uri.TryCreate(pageUrl, HtmlEntity.DeEntitize(crumb.GetAttributeValue("href", "")), out var target)
                && SiteCrawlHttp.IsAllowed(target, host) && SiteLayoutScanner.PageKey(target) == self)
                continue;
            names.Add(Truncate(name, MaxListNameLength));
        }
        return names;
    }

    internal static string? ArticleKey(Uri url)
    {
        var query = HttpUtility.ParseQueryString(url.Query);
        var id = query["id"]?.Trim();
        if (!string.IsNullOrEmpty(id)) return id;
        var subId = query["sub_id"]?.Trim();
        return string.IsNullOrEmpty(subId) ? null : SubPagePrefix + subId;
    }

    internal static bool IsSubPageKey(string key) => key.StartsWith(SubPagePrefix, StringComparison.Ordinal);

    // 只有 sub_id 的頁面沒有 #list_ctrl／menu_id，不能用 ReadArticle 判斷；頁面本身是列表時回傳 null（子分類，不是文章）。
    internal static SiteArticleInfo? ReadSubPageArticle(HtmlDocument document, Uri pageUrl, string host, string key)
    {
        if (!IsSubPageKey(key) || ReadList(document, pageUrl, host) != null) return null;
        var title = Text(document.DocumentNode.SelectSingleNode(Container)?.SelectSingleNode(TitleNode)) ?? PageName(document) ?? key;
        return new SiteArticleInfo(key, Truncate(title, MaxTitleLength));
    }

    // 舊站內容區上方的分類標題（#titleBlock 裡的 h2#sub_title，例 泡湯游泳、花鹿三追一），在 #cont 之外。
    internal static string? ReadSubTitle(HtmlDocument document) =>
        Text(document.DocumentNode.SelectSingleNode("//*[@id='sub_title']"));

    // 舊站標題格式是「名稱-上層-站名」，第一段就是這頁自己的名稱。
    internal static string? PageName(HtmlDocument document)
    {
        var title = Text(document.DocumentNode.SelectSingleNode("//title"));
        var name = title?.Split('-')[0].Trim();
        return string.IsNullOrEmpty(name) ? null : name;
    }

    private static DateTime? ParseDate(string? text)
    {
        var match = DatePattern().Match(text ?? "");
        if (!match.Success) return null;
        var value = $"{match.Groups["y"].Value}-{match.Groups["m"].Value.PadLeft(2, '0')}-{match.Groups["d"].Value.PadLeft(2, '0')}";
        return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date : null;
    }

    private static string? Text(HtmlNode? node)
    {
        if (node == null) return null;
        var text = Regex.Replace(HtmlEntity.DeEntitize(node.InnerText), @"\s+", " ").Trim();
        return text.Length == 0 ? null : text;
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];
}
