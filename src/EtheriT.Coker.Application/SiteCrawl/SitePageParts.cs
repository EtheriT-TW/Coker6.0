using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed record SiteMarqueeItem(string Title, Uri? Link);

// 舊站版面上「不在內文裡」的零件，2026-11-13 對 ayawan.com.tw 實測（兩者都是 JS 載入，要用渲染後的頁面讀）：
// 跑馬燈＝#hd_marquee ul.marquee 的 li；分類子選單＝左欄 #left_main_menu 的連結（主選單沒有下拉，子頁只列在這裡）。
internal static partial class SitePageParts
{
    private const string MarqueeItems =
        "//*[@id='hd_marquee']//li | //ul[contains(concat(' ',normalize-space(@class),' '),' marquee ')]/li | //marquee";
    private const string SectionLinks = "//*[@id='left_main_menu']//a[@href]";
    // 網站 Logo（2026-11-18 superiorlink、ayawan 實測）：舊站是 h1#logo > a#logo_link > img#hd_logo；
    // 其他站退而找 id／class 為 logo 的區塊裡的圖，或版頭裡 id／class／alt／檔名含 logo 的圖。頁尾的 logo 不算。
    private const string NotInFooter = "not(ancestor::footer or ancestor::*[@id='foorerContent' or @id='foot'])";
    private const string LogoBlock =
        "@id='logo' or @id='logo_link' or contains(concat(' ',normalize-space(@class),' '),' logo ')";
    private const string LogoHint =
        "contains(translate(concat(@id,' ',@class,' ',@alt,' ',@src),'LOGO','logo'),'logo')";
    private static readonly string[] LogoImages =
    [
        "//img[@id='hd_logo']",
        $"//*[{LogoBlock}]//img[{NotInFooter}]",
        $"//header//img[{LogoHint}][{NotInFooter}]"
    ];
    private const int MaxMarqueeTitleLength = 120;
    private const int MaxMarqueeLinkLength = 255;
    private const int MaxMenuTitleLength = 100;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    internal static IReadOnlyList<SiteMarqueeItem> ReadMarquee(HtmlDocument document, Uri pageUrl)
    {
        var items = new List<SiteMarqueeItem>();
        foreach (var node in document.DocumentNode.SelectNodes(MarqueeItems) ?? Enumerable.Empty<HtmlNode>())
        {
            var title = Text(node);
            if (title == null || items.Any(item => item.Title == title)) continue;
            var href = node.SelectSingleNode(".//a[@href]")?.GetAttributeValue("href", "");
            var link = href != null && Uri.TryCreate(pageUrl, HtmlEntity.DeEntitize(href).Trim(), out var target)
                && target.Scheme is "http" or "https" && target.AbsoluteUri.Length <= MaxMarqueeLinkLength
                ? target : null;
            items.Add(new SiteMarqueeItem(Truncate(title, MaxMarqueeTitleLength), link));
        }
        return items;
    }

    internal static IReadOnlyList<SiteMenuLink> ReadSectionMenu(HtmlDocument document, Uri pageUrl, string host)
    {
        var links = new List<SiteMenuLink>();
        foreach (var anchor in document.DocumentNode.SelectNodes(SectionLinks) ?? Enumerable.Empty<HtmlNode>())
        {
            var title = Text(anchor);
            var href = HtmlEntity.DeEntitize(anchor.GetAttributeValue("href", "")).Trim();
            if (title == null || href.StartsWith('#') || !Uri.TryCreate(pageUrl, href, out var target)
                || !SiteCrawlHttp.IsAllowed(target, host))
                continue;
            var key = SiteLayoutScanner.PageKey(target);
            if (links.Any(link => SiteLayoutScanner.PageKey(link.Url!) == key)) continue;
            links.Add(new SiteMenuLink(Truncate(title, MaxMenuTitleLength), target,
                anchor.GetAttributeValue("target", "") == "_blank", []));
        }
        return links;
    }

    internal static Uri? ReadLogo(HtmlDocument document, Uri pageUrl)
    {
        // 舊站沒設 Logo 時 img#hd_logo 的 src 是空的（988job 實測），要往下找，找不到就不匯入。
        foreach (var image in LogoImages.SelectMany(xpath =>
            document.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>()))
        {
            var src = HtmlEntity.DeEntitize(SiteCrawlHtml.ImageSource(image)).Trim();
            if (src.Length > 0 && Uri.TryCreate(pageUrl, src, out var url) && url.Scheme is "http" or "https")
                return new UriBuilder(url) { Fragment = "" }.Uri;
        }
        return null;
    }

    private static string? Text(HtmlNode node)
    {
        var text = Whitespace().Replace(HtmlEntity.DeEntitize(node.InnerText), " ").Trim();
        return text.Length == 0 ? null : text;
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];
}
