using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteLayoutScanner(SiteCrawlHttp http, IOptions<SiteCrawlOptions> options,
    ISiteCrawlBrowserFactory browsers, ILogger<SiteLayoutScanner> logger)
{
    internal sealed record LayoutPage(Uri Url, HtmlNode? Banner, HtmlNode? Footer, HtmlNode? Content = null,
        SiteArticleInfo? Article = null, SiteArticleList? List = null, string? Title = null,
        IReadOnlySet<string>? ContentLinks = null, IReadOnlyList<string>? Breadcrumb = null,
        IReadOnlyList<SiteMarqueeItem>? Marquee = null, IReadOnlyList<SiteMenuLink>? SectionLinks = null,
        string? SubTitle = null, IReadOnlyList<Uri>? SubCategories = null, Uri? Logo = null);
    private const string Banner = "contains(@class,'banner') or contains(@class,'swiper') or contains(@class,'carousel') or contains(@class,'slider') or contains(@class,'hero') or contains(@id,'banner')";
    private const string Footer = "//*[@role='contentinfo' or @id='footer' or contains(concat(' ',normalize-space(@class),' '),' footer ')]";
    private const string LegacyFooter = "//*[ @id='foorerContent' or contains(concat(' ',normalize-space(@class),' '),' foorerContent ') or @id='footerContent' or contains(concat(' ',normalize-space(@class),' '),' footerContent ')]";
    private const int MaxBrokenLinksShown = 3;
    private static readonly Regex BannerImages = new("""var\s+bannerAra\s*=\s*(?<list>\[[^\]]*\])""");
    private static readonly Regex BannerLinks = new("""var\s+bannerLinkAra\s*=\s*(?<list>\[[^\]]*\])""");
    // JS 字串陣列的每一項：舊站內頁用雙引號，首頁 default1.asp 用單引號（superiorlink 2026-11-17 實測），兩種都要讀。
    private static readonly Regex JsString = new(@"'(?<value>(?:\\.|[^'\\])*)'|""(?<value>(?:\\.|[^""\\])*)""");
    private static readonly Regex JsEscape = new("""\\(?<char>.)""");
    private static readonly Regex MetaRefresh = new(
        """^\s*\d+(?:\.\d+)?\s*;\s*url\s*=\s*(?<url>.+?)\s*$""", RegexOptions.IgnoreCase);
    private static readonly Regex LocationRedirect = new(
        // 舊站入口也有 location.href = ("tw/index.asp?…") 這種多一層括號的寫法（2026-11-17 sf2.com.tw）。
        """^\s*(?:window\.|document\.|self\.|top\.)?location(?:(?:\.href)?\s*=\s*\(?\s*|\.(?:replace|assign)\s*\(\s*)(?<quote>['"])(?<url>[^'"\r\n]+)\k<quote>\s*\)?\s*;?\s*$""",
        RegexOptions.IgnoreCase);

    internal async Task<List<LayoutPage>> ScanAsync(Uri root, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var pending = new Queue<Uri>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var fetched = new HashSet<string>(StringComparer.Ordinal);
        var variants = new Dictionary<string, int>(StringComparer.Ordinal);
        var pages = new List<LayoutPage>();
        var listedArticles = new HashSet<string>(StringComparer.Ordinal);
        var referrers = new Dictionary<string, string>(StringComparer.Ordinal);
        var brokenLinks = new List<string>();
        var entryRedirects = 0;
        await using var renderer = new SiteCrawlRenderSession(browsers, root.IdnHost, result, logger);
        Enqueue(root, root, pending, seen, variants, result);
        while (pending.TryDequeue(out var uri))
        {
            await checkpoint();
            token.ThrowIfCancellationRequested();
            if (fetched.Contains(uri.AbsoluteUri)) continue;
            try
            {
                var reply = await http.GetAsync(uri, root.IdnHost, false, 2 * 1024 * 1024, token);
                await using var content = reply.Content;
                var canonical = Normalize(reply.Url);
                if (!fetched.Add(canonical.AbsoluteUri)) continue;
                var bytes = content.Length;
                var page = ReadDocument(content, reply.Charset);
                var entryTarget = FindEntryTarget(page, reply.Url);
                if (entryTarget != null)
                {
                    if (!SiteCrawlHttp.IsAllowed(entryTarget, root.IdnHost))
                        result.AddError($"入口 {uri.AbsolutePath} 的跳轉超出來源網域。");
                    else if (pages.Count == 0 && ++entryRedirects > 5)
                        result.AddError("入口頁跳轉超過上限。");
                    else if (fetched.Contains(Normalize(entryTarget).AbsoluteUri))
                        result.AddError($"入口頁跳轉重複到訪：{entryTarget.AbsolutePath}。");
                    else
                        Enqueue(entryTarget, root, pending, seen, variants, result, isEntryRedirect: pages.Count == 0);
                    if (pending.Count > 0) await Task.Delay(500, token);
                    continue;
                }
                var layout = ReadLayout(canonical, page);
                var rendered = SiteCrawlRenderSession.NeedsRender(page)
                    ? await renderer.TryRenderAsync(reply.Url, SiteCrawlRenderSession.NeedsScroll(page), token)
                    : null;
                if (rendered != null) layout = FillMissingLayout(layout, ReadLayout(canonical, rendered));
                var article = SiteArticleDetector.ReadArticle(rendered ?? page);
                var list = SiteArticleDetector.ReadList(rendered ?? page, reply.Url, root.IdnHost);
                if (article != null || list != null)
                    layout = layout with { Article = article, List = list };
                // 內文全部留著：選單、文章，以及事後才知道要建隱藏選單的獨立頁面都會用到。
                var contentNode = ReadContent(rendered ?? page);
                layout = layout with
                {
                    Content = contentNode,
                    Title = SiteArticleDetector.PageName(rendered ?? page),
                    SubTitle = SiteArticleDetector.ReadSubTitle(rendered ?? page),
                    Breadcrumb = SiteArticleDetector.ReadBreadcrumb(rendered ?? page, reply.Url, root.IdnHost),
                    ContentLinks = ContentLinks(contentNode, reply.Url, root.IdnHost),
                    Marquee = SitePageParts.ReadMarquee(rendered ?? page, reply.Url),
                    SectionLinks = SitePageParts.ReadSectionMenu(rendered ?? page, reply.Url, root.IdnHost)
                };
                pages.Add(layout);
                result.PagesScanned = pages.Count;
                if (layout.Banner != null) result.PagesWithBanner++;
                if (layout.Footer != null) result.PagesWithFooter++;
                var links = page.DocumentNode.SelectNodes("//a[@href]");
                if (pages.Count == 1)
                    result.FirstPageDiagnostic = DescribePage(canonical, page, bytes, links?.Count ?? 0);
                var renderedLinks = rendered?.DocumentNode.SelectNodes("//a[@href]");
                // 列表項目的詳細頁不佔掃描頁數，文章匯入時會依列表逐篇補抓，避免頁數上限讓目錄缺文章。
                // 只有 sub_id 的項目照常掃描：要掃到才知道它是子分類列表還是單一內容頁（ResolveSubPages）。
                foreach (var item in list?.Items ?? [])
                    if (!SiteArticleDetector.IsSubPageKey(item.Key)) listedArticles.Add(PageKey(item.Url));
                foreach (var link in (links ?? Enumerable.Empty<HtmlNode>()).Concat(renderedLinks ?? Enumerable.Empty<HtmlNode>()))
                    if (Uri.TryCreate(reply.Url, HtmlEntity.DeEntitize(link.GetAttributeValue("href", "")), out var target)
                        && SiteCrawlHttp.IsAllowed(target, root.IdnHost)
                        && !listedArticles.Contains(PageKey(target)))
                    {
                        referrers.TryAdd(PageKey(target), canonical.PathAndQuery);
                        Enqueue(target, root, pending, seen, variants, result);
                    }
            }
            // 舊站本身的失效連結：記下是哪一頁連過去的，最後合併成一則，不逐筆報錯。
            catch (HttpRequestException ex) when (!token.IsCancellationRequested && ex.StatusCode == HttpStatusCode.NotFound)
            {
                var from = referrers.GetValueOrDefault(PageKey(uri));
                brokenLinks.Add(from == null ? uri.PathAndQuery : $"{uri.PathAndQuery}（來自 {from}）");
            }
            catch (Exception ex) when (!token.IsCancellationRequested &&
                ex is HttpRequestException or InvalidOperationException or OperationCanceledException)
            {
                result.AddError($"無法抓取 {uri.PathAndQuery}。{ex.Message}");
            }
            if (pending.Count > 0) await Task.Delay(500, token);
        }
        if (brokenLinks.Count > 0)
            result.AddError($"舊站有 {brokenLinks.Count} 個連結找不到頁面（404），已略過：" +
                string.Join("、", brokenLinks.Take(MaxBrokenLinksShown)) + (brokenLinks.Count > MaxBrokenLinksShown ? " 等" : "") + "。");
        ResolveSubPages(pages);
        return pages;
    }

    // 列表裡只有 sub_id 的項目：掃到的目標頁是列表 → 子分類，從列表拿掉；不是列表 → 標成文章，
    // 之後不會被建成隱藏選單。沒掃到的（頁數上限）留在列表，文章匯入補抓時再用 ReadSubPageArticle 判斷。
    // 拿掉的子分類依列表順序記在 SubCategories：隱藏選單靠它排出分類層級，分類頁靠它改用選單目錄（SiteCategoryLists）。
    private static void ResolveSubPages(List<LayoutPage> pages)
    {
        var lists = pages.Where(page => page.List != null).Select(page => PageKey(page.Url)).ToHashSet(StringComparer.Ordinal);
        var articles = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < pages.Count; i++)
        {
            if (pages[i].List is not { } list) continue;
            var subCategories = list.Items
                .Where(item => SiteArticleDetector.IsSubPageKey(item.Key) && lists.Contains(PageKey(item.Url)))
                .ToList();
            var items = list.Items.Except(subCategories).ToList();
            foreach (var item in items.Where(item => SiteArticleDetector.IsSubPageKey(item.Key)))
                articles.TryAdd(PageKey(item.Url), item.Key);
            if (subCategories.Count > 0)
                pages[i] = pages[i] with
                {
                    List = items.Count == 0 ? null : list with { Items = items },
                    SubCategories = subCategories.Select(item => item.Url).ToList()
                };
        }
        for (var i = 0; i < pages.Count; i++)
        {
            var page = pages[i];
            if (page.Article == null && page.List == null && page.Content != null
                && articles.TryGetValue(PageKey(page.Url), out var key))
                pages[i] = page with { Article = new SiteArticleInfo(key, page.Title ?? key) };
        }
    }

    internal SiteCrawlRenderSession OpenRenderer(string host, SiteLayoutResult result) => new(browsers, host, result, logger);

    // 文章詳細頁先用一般 HTTP 抓；頁面有舊站 JS 才填入的區塊（.contItem，例 房型的產品說明／房間說明，2026-11-16 實測）
    // 再用瀏覽器重抓，判斷方式和掃描一致。瀏覽器失敗時用 HTTP 版本，少的只是 JS 載入的內容。
    // key 是列表給的文章識別碼；sub: 開頭（只有 sub_id 的頁面）要渲染後確認目標頁不是列表才算文章。
    internal async Task<LayoutPage?> FetchArticleAsync(Uri url, string key, string host, SiteCrawlRenderSession renderer,
        CancellationToken token)
    {
        var reply = await http.GetAsync(url, host, false, 2 * 1024 * 1024, token);
        await using var content = reply.Content;
        var page = ReadDocument(content, reply.Charset);
        var article = SiteArticleDetector.ReadArticle(page);
        if (article == null && !SiteArticleDetector.IsSubPageKey(key)) return null;
        if (SiteCrawlRenderSession.NeedsRender(page)
            && await renderer.TryRenderAsync(reply.Url, false, token) is { } rendered
            && (article == null || SiteArticleDetector.ReadArticle(rendered) != null))
        {
            page = rendered;
            article = SiteArticleDetector.ReadArticle(rendered) ?? article;
        }
        article ??= SiteArticleDetector.ReadSubPageArticle(page, reply.Url, host, key);
        return article == null ? null : new LayoutPage(Normalize(reply.Url), null, null, ReadContent(page), article,
            Title: SiteArticleDetector.PageName(page), SubTitle: SiteArticleDetector.ReadSubTitle(page));
    }

    private void Enqueue(Uri uri, Uri root, Queue<Uri> pending, HashSet<string> seen,
        Dictionary<string, int> variants, SiteLayoutResult result, bool isEntryRedirect = false)
    {
        if (!SiteCrawlHttp.IsAllowed(uri, root.IdnHost) || SiteListDetector.IsFileLink(uri)) return;
        // 舊站常混用 http／https 連結，不統一的話同一頁會被抓兩次。
        if (!isEntryRedirect && uri.Scheme != root.Scheme)
            uri = new UriBuilder(uri) { Scheme = root.Scheme, Port = root.Port }.Uri;
        uri = Normalize(uri);
        if (seen.Contains(uri.AbsoluteUri)) return;
        if (!isEntryRedirect && options.Value.MaxPages > 0 && seen.Count >= options.Value.MaxPages)
        {
            result.PageLimitReached = true;
            return;
        }
        var path = uri.AbsolutePath;
        var variantLimit = options.Value.MaxQueryVariantsPerPath;
        if (variantLimit > 0 && uri.Query.Length > 0 && variants.GetValueOrDefault(path) >= variantLimit)
        {
            result.QueryLimitPaths.Add(path);
            return;
        }
        if (uri.Query.Length > 0) variants[path] = variants.GetValueOrDefault(path) + 1;
        seen.Add(uri.AbsoluteUri);
        pending.Enqueue(uri);
    }

    internal static Uri Normalize(Uri uri)
    {
        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(item =>
            {
                var key = Uri.UnescapeDataString(item.Split('=', 2)[0]).ToLowerInvariant();
                return !key.StartsWith("utm_", StringComparison.Ordinal) &&
                    key is not ("fbclid" or "gclid" or "msclkid" or "sort" or "sortby" or "sortorder" or "orderby" or "order" or "direction");
            }).OrderBy(item => item.Split('=', 2)[0], StringComparer.Ordinal);
        return new UriBuilder(uri) { Fragment = "", Query = string.Join("&", query) }.Uri;
    }

    private static HashSet<string> ContentLinks(HtmlNode? content, Uri pageUrl, string host)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var link in content?.SelectNodes(".//a[@href]") ?? Enumerable.Empty<HtmlNode>())
            if (Uri.TryCreate(pageUrl, HtmlEntity.DeEntitize(link.GetAttributeValue("href", "")), out var target)
                && SiteCrawlHttp.IsAllowed(target, host))
                keys.Add(PageKey(target));
        return keys;
    }

    internal static string PageKey(Uri uri)
    {
        var normalized = Normalize(uri);
        return normalized.IdnHost.ToLowerInvariant() + normalized.PathAndQuery;
    }

    internal static bool CanHoldContent(ImportedSiteMenu menu) =>
        !menu.IsAlias && menu.SourceUrl != null && menu.PageType is PageTypeEnum.一般頁面 or PageTypeEnum.首頁;

    internal static bool IsSameSection(HtmlNode? node, Uri url, HtmlNode? other, Uri otherUrl) =>
        node != null && other != null && Signature(node, url) == Signature(other, otherUrl);

    internal static HtmlDocument ReadDocument(Stream content, string? charset)
    {
        var document = new HtmlDocument();
        Encoding? encoding = null;
        if (!string.IsNullOrWhiteSpace(charset))
        {
            try { encoding = Encoding.GetEncoding(charset.Trim('"')); }
            catch (ArgumentException) { }
        }
        if (encoding == null)
            document.Load(content, true);
        else
            document.Load(content, encoding, true);
        return document;
    }

    internal static string DescribePage(Uri url, HtmlDocument document, long bytes, int links)
    {
        var title = HtmlEntity.DeEntitize(document.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "").Trim();
        if (title.Length > 80) title = title[..80];
        return $"第一份 HTML：{url.GetLeftPart(UriPartial.Path)}，標題「{title}」，{bytes} 位元組，{links} 個連結。";
    }

    internal static Uri? FindEntryTarget(HtmlDocument document, Uri url)
    {
        var root = document.DocumentNode;
        var refresh = root.SelectSingleNode("//meta[translate(@http-equiv,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='refresh']");
        var match = MetaRefresh.Match(refresh?.GetAttributeValue("content", "") ?? "");
        if (match.Success && TryResolve(match, url) is { } refreshTarget) return refreshTarget;

        if (root.SelectSingleNode("//a[@href] | //main | //article | //footer") != null) return null;
        var frames = root.SelectNodes("//frameset//frame[@src]");
        if (frames?.Count == 1 && Uri.TryCreate(url, frames[0].GetAttributeValue("src", ""), out var frame))
            return frame;
        foreach (var script in root.SelectNodes("//script[not(@src)]") ?? Enumerable.Empty<HtmlNode>())
        {
            match = LocationRedirect.Match(script.InnerHtml);
            if (match.Success && TryResolve(match, url) is { } scriptTarget) return scriptTarget;
        }
        return null;
    }

    private static Uri? TryResolve(Match match, Uri url)
        => Uri.TryCreate(url, HtmlEntity.DeEntitize(match.Groups["url"].Value).Trim().Trim('\'', '"'), out var target)
            && target.Scheme is "http" or "https" ? target : null;

    private static LayoutPage ReadLayout(Uri url, HtmlDocument document)
    {
        var root = document.DocumentNode;
        var header = root.SelectSingleNode("//header[not(ancestor::article or ancestor::main)]");
        var banner = root.SelectSingleNode("//*[@id='bannerCont']") ??
            header?.SelectSingleNode($".//*[{Banner}]") ??
            root.SelectSingleNode($"//*[{Banner}][not(ancestor::footer or ancestor::article)]");
        var footer = root.SelectSingleNode("//*[@id='foorerContent']")
            ?? root.SelectSingleNode("//*[@id='foot']")
            ?? root.SelectSingleNode("//footer[not(ancestor::article or ancestor::main)]")
            ?? root.SelectSingleNode(Footer) ?? root.SelectSingleNode(LegacyFooter);
        return new LayoutPage(url, ReadScriptedBanner(root) ?? Detach(banner), DetachWithSiblingStyles(footer),
            Logo: SitePageParts.ReadLogo(document, url));
    }

    private static HtmlNode? ReadContent(HtmlDocument document)
    {
        var root = document.DocumentNode;
        return Detach(root.SelectSingleNode("//*[@id='cont']")
            ?? root.SelectSingleNode("//main")
            ?? root.SelectSingleNode("//article")
            ?? root.SelectSingleNode("//body"));
    }

    // 版頭頁尾優先用靜態 HTML：渲染後 JS 會加上不固定的 class，跨頁比對會被誤判成不一致。
    private static LayoutPage FillMissingLayout(LayoutPage layout, LayoutPage rendered)
        => layout with
        {
            Banner = layout.Banner ?? rendered.Banner, Footer = layout.Footer ?? rendered.Footer, Logo = layout.Logo ?? rendered.Logo
        };

    // 舊站的完整輪播只寫在內嵌腳本，靜態 HTML 僅有第一張圖且連結為 javascript:void(0)。
    // 輸出成 Bootstrap 輪播結構：版頭匯入只取圖片與連結；頁面自己的版頭放進內文時由 SiteSwiperMarkup 轉成新站輪播。
    // 沒有連結的圖片不包 <a href="">：空連結比對版頭時會被解析成各頁自己的網址，每頁版頭都被誤判成不同。
    private static HtmlNode? ReadScriptedBanner(HtmlNode root)
    {
        var script = root.SelectNodes("//script[not(@src)]")?
            .Select(node => node.InnerHtml)
            .FirstOrDefault(text => BannerImages.IsMatch(text));
        if (script == null) return null;

        var links = ReadStrings(BannerLinks, script);
        var slides = ReadStrings(BannerImages, script)
            .Select((image, index) => (image, link: index < links.Length ? links[index] : ""))
            .Where(slide => !string.IsNullOrWhiteSpace(slide.image))
            .Select(slide =>
            {
                var image = $"<img src=\"{WebUtility.HtmlEncode(slide.image)}\" />";
                return string.IsNullOrWhiteSpace(slide.link) ? $"<div class=\"item\">{image}</div>"
                    : $"<div class=\"item\"><a href=\"{WebUtility.HtmlEncode(slide.link)}\">{image}</a></div>";
            })
            .ToArray();
        return slides.Length == 0 ? null
            : HtmlNode.CreateNode($"<div id=\"bannerCont\" class=\"carousel\"><div class=\"carousel-inner\">{string.Concat(slides)}</div></div>");
    }

    private static string[] ReadStrings(Regex pattern, string script)
    {
        var match = pattern.Match(script);
        if (!match.Success) return [];
        return JsString.Matches(match.Groups["list"].Value)
            .Select(item => JsEscape.Replace(item.Groups["value"].Value, escape => escape.Groups["char"].Value))
            .ToArray();
    }

    private static HtmlNode? Detach(HtmlNode? node) => node == null ? null : HtmlNode.CreateNode(node.OuterHtml);

    // 舊站頁尾的 CSS 寫在 #foorerContent 旁邊的 <style>，要一起帶走；body 層的 <style> 屬於全站，不算頁尾。
    private static HtmlNode? DetachWithSiblingStyles(HtmlNode? node)
    {
        var copy = Detach(node);
        if (copy == null || node!.ParentNode is not { Name: not ("body" or "html" or "#document") } parent) return copy;
        foreach (var style in parent.ChildNodes.Where(x => x.Name == "style"))
            copy.AppendChild(HtmlNode.CreateNode(style.OuterHtml));
        return copy;
    }

    internal static (LayoutPage? Page, int Votes) SelectLayout(
        IEnumerable<LayoutPage> pages, Func<LayoutPage, HtmlNode?> select)
    {
        var winner = pages.Select((page, index) => (page, index, node: select(page)))
            .Where(x => x.node != null)
            .GroupBy(x => Signature(x.node!, x.page.Url))
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.First().index == 0 ? "" : x.Min(item => item.page.Url.AbsoluteUri), StringComparer.Ordinal)
            .FirstOrDefault();
        return (winner?.First().page, winner?.Count() ?? 0);
    }

    private static string Signature(HtmlNode source, Uri url)
    {
        var node = source.CloneNode(true);
        foreach (var remove in node.SelectNodes(".//script|.//input[@type='hidden']|.//comment()") ?? Enumerable.Empty<HtmlNode>())
            remove.Remove();
        foreach (var item in node.DescendantsAndSelf().Where(x => x.NodeType == HtmlNodeType.Element))
        {
            var attrs = item.Attributes.OrderBy(x => x.Name, StringComparer.Ordinal).ToArray();
            item.Attributes.RemoveAll();
            foreach (var attr in attrs)
            {
                if (attr.Name is "id" or "nonce" or "data-timestamp" or "aria-current" || attr.Name.StartsWith("on")) continue;
                var value = attr.Value;
                if (attr.Name == "class")
                    value = string.Join(" ", value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Where(x => x is not ("active" or "current" or "swiper-slide-active")));
                if (attr.Name is "src" or "href" or "data-src" && value.Length > 0 && !value.StartsWith('#')
                    && Uri.TryCreate(url, HtmlEntity.DeEntitize(value), out var absolute))
                    value = absolute.AbsoluteUri;
                if (value.Length > 0) item.SetAttributeValue(attr.Name, value);
            }
        }
        var normalized = Regex.Replace(node.OuterHtml, @"\s+", " ").Trim();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }
}
