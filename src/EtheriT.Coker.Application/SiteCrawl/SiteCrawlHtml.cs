using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Dto;
using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteCrawlHtml(SiteCrawlHttp http, FileUploadAppService files)
{
    private const string UnsafeElements = ".//script|.//form|.//link|.//style|.//source|.//video|.//audio|.//embed|.//object|.//svg|.//base";
    private const string InlineClassPrefix = "crawl-s-";
    private const string PlaceholderPath = "/holder.js/";
    private static readonly Regex CssUrl = new("""url\(\s*(?<q>['"]?)(?<url>[^'")]*)\k<q>\s*\)""", RegexOptions.IgnoreCase);
    private static readonly Regex ImagePath = new(@"\.(png|jpe?g|gif|webp|svg|bmp|ico)$", RegexOptions.IgnoreCase);
    private static readonly Regex DefaultDocument = new(@"/(index|default)\.(asp|aspx|php|html?|jsp)$", RegexOptions.IgnoreCase);
    private const string PageParameter = "page";

    // 回傳該段 HTML 用到的 CSS：<style> 原樣保留；style 屬性會被淨化器擋掉，改成 class＋規則。
    internal async Task<string> CleanAsync(HtmlNode node, Uri pageUrl, SiteLayoutResult result, bool allowIframes,
        Func<Task> checkpoint, CancellationToken token)
    {
        var css = CollectCss(node);
        RemoveElements(node);
        await CleanElementsAsync(node, pageUrl, result, allowIframes, checkpoint, token);
        return await RewriteCssUrlsAsync(css, pageUrl, result, checkpoint, token);
    }

    private static string CollectCss(HtmlNode node)
    {
        var css = new StringBuilder();
        foreach (var style in node.SelectNodes(".//style") ?? Enumerable.Empty<HtmlNode>())
            css.AppendLine(style.InnerHtml.Replace("<!--", "").Replace("-->", "").Trim());

        var inlineRules = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var element in node.DescendantsAndSelf().Where(x => x.NodeType == HtmlNodeType.Element))
        {
            var declarations = Regex.Replace(HtmlEntity.DeEntitize(element.GetAttributeValue("style", "")), "[{}<>]", "").Trim();
            if (declarations.Length == 0) continue;
            var name = InlineClassPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(declarations)))[..10].ToLowerInvariant();
            element.AddClass(name);
            inlineRules.TryAdd(name, declarations);
        }
        foreach (var (name, declarations) in inlineRules)
            css.AppendLine($".{name}{{{declarations}}}");
        return css.ToString();
    }

    internal async Task<string> RewriteCssUrlsAsync(string css, Uri baseUrl, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var output = new StringBuilder();
        var last = 0;
        foreach (Match match in CssUrl.Matches(css))
        {
            output.Append(css, last, match.Index - last);
            last = match.Index + match.Length;
            var value = match.Groups["url"].Value.Trim();
            var resolved = await ResolveCssUrlAsync(value, baseUrl, result, checkpoint, token);
            output.Append(resolved == null ? match.Value : $"url('{resolved}')");
        }
        output.Append(css, last, css.Length - last);
        return output.ToString().Trim();
    }

    // 同網域的圖片搬到新站；字型等其他檔案改成舊站完整網址，至少在舊站下線前還讀得到。
    private async Task<string?> ResolveCssUrlAsync(string value, Uri baseUrl, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        if (value.Length == 0 || value.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith('#') || !Uri.TryCreate(baseUrl, value, out var target)
            || target.Scheme is not ("http" or "https"))
            return null;
        if (SiteCrawlHttp.IsAllowed(target, baseUrl.IdnHost) && ImagePath.IsMatch(target.AbsolutePath))
            return await ImportImageAsync(target.AbsoluteUri, baseUrl, result, checkpoint, token) ?? target.AbsoluteUri;
        return target.AbsoluteUri;
    }

    private static void RemoveElements(HtmlNode node)
    {
        foreach (var child in node.SelectNodes(UnsafeElements) ?? Enumerable.Empty<HtmlNode>())
            child.Remove();
    }

    private async Task CleanElementsAsync(HtmlNode node, Uri pageUrl, SiteLayoutResult result, bool allowIframes,
        Func<Task> checkpoint, CancellationToken token)
    {
        foreach (var child in node.DescendantsAndSelf().Where(x => x.NodeType == HtmlNodeType.Element).ToArray())
        {
            if (child.Name == "iframe" && !(allowIframes && IsHttps(child, pageUrl)))
            {
                child.Remove();
                continue;
            }
            if (child.Name == "img")
            {
                var path = await ImportImageAsync(ImageSource(child), pageUrl, result, checkpoint, token);
                if (path == null) { child.Remove(); continue; }
                child.SetAttributeValue("src", path);
            }
            foreach (var attr in child.Attributes.ToArray())
                if (attr.Name.StartsWith("on") || attr.Name.StartsWith("data-") ||
                    attr.Name is "style" or "srcset" or "background" or "poster" or "ping" ||
                    (attr.Name == "src" && child.Name is not ("img" or "iframe")))
                    child.Attributes.Remove(attr);
            await RewriteLinkAsync(child, pageUrl, result, checkpoint, token);
        }
    }

    private static bool IsHttps(HtmlNode iframe, Uri pageUrl) =>
        Uri.TryCreate(pageUrl, HtmlEntity.DeEntitize(iframe.GetAttributeValue("src", "")), out var src)
        && src.Scheme == "https";

    private async Task RewriteLinkAsync(HtmlNode node, Uri pageUrl, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        if (node.Attributes["href"] is not { } link) return;
        var value = HtmlEntity.DeEntitize(link.Value).Trim();
        if (node.Name == "a" && value.StartsWith('#')) return;
        if (node.Name == "a" && Uri.TryCreate(pageUrl, value, out var href) &&
            href.Scheme is "http" or "https" or "mailto" or "tel")
            link.Value = FindRoute(result, href)
                ?? await ImportLinkedFileAsync(href, pageUrl, result, checkpoint, token)
                ?? href.AbsoluteUri;
        else
            node.Attributes.Remove(link);
    }

    // 內文連到舊站的檔案：圖片（例 sf2 證書縮圖點開的 /upload/iso01.jpg）、PDF、Word 等附件都搬到新站，連結改指新站，
    // 舊站下線後才不會失效（使用者 2026-11-17 要求）。搬不動時保留舊站網址並記錯誤。
    private async Task<string?> ImportLinkedFileAsync(Uri href, Uri pageUrl, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        if (href.Scheme is not ("http" or "https") || !SiteCrawlHttp.IsAllowed(href, pageUrl.IdnHost)) return null;
        if (ImagePath.IsMatch(href.AbsolutePath))
            return await ImportImageAsync(href.AbsoluteUri, pageUrl, result, checkpoint, token);
        if (!SiteListDetector.IsFileLink(href)) return null;
        await checkpoint();
        var target = new UriBuilder(href) { Fragment = "" }.Uri;
        try
        {
            return await files.ImportCrawlFileAsync(target, pageUrl.IdnHost, http, token);
        }
        catch (Exception ex) when (!token.IsCancellationRequested &&
            ex is HttpRequestException or InvalidOperationException or OperationCanceledException or ArgumentException)
        {
            // 只寫是哪一頁的哪個檔案，不寫舊站路徑（使用者 2026-11-18 要求）；
            // 404 是舊站本身的失效連結（例 superiorlink 型錄 PDF 已被刪除），講清楚不是匯入出錯。
            var source = result.ContentSource ?? SiteLayoutResult.SourceName(null, pageUrl, "頁面");
            var name = Uri.UnescapeDataString(target.Segments.LastOrDefault() ?? target.AbsolutePath);
            result.AddError(ex is HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound }
                ? $"{source}中的「{name}」在舊站中已無資料。"
                : $"{source}中的「{name}」無法從舊站下載，連結暫時指向舊站。{ex.Message}");
            return null;
        }
    }

    // 下載同網域圖片並綁到文章等對象（例如列表上的文章圖片），失敗只記錯誤，不中斷匯入。
    internal async Task<bool> BindImageAsync(string url, Uri pageUrl, long sid, FileBindTypeEnum type,
        SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        await checkpoint();
        var uri = new Uri(url);
        try
        {
            var file = await files.ImportCrawlImageFileAsync(uri, pageUrl.IdnHost, http, token);
            await files.BindCrawlImageAsync(file, sid, type);
            return true;
        }
        catch (Exception ex) when (!token.IsCancellationRequested &&
            ex is HttpRequestException or InvalidOperationException or OperationCanceledException or ArgumentException)
        {
            result.AddError($"圖片 {uri.AbsolutePath} 匯入失敗。{ex.Message}");
            return false;
        }
    }

    // 舊站 Logo 下載後設成網站資料的網站Logo；失敗只記錯誤，不中斷匯入。回傳 SiteLayoutResult.LogoStatus 的值。
    internal async Task<string?> ImportWebsiteLogoAsync(Uri url, Uri pageUrl, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        await checkpoint();
        try
        {
            var file = await files.ImportCrawlImageFileAsync(url, pageUrl.IdnHost, http, token);
            return await files.SetCrawlWebsiteLogoAsync(file);
        }
        catch (Exception ex) when (!token.IsCancellationRequested &&
            ex is HttpRequestException or InvalidOperationException or OperationCanceledException or ArgumentException)
        {
            result.AddError($"網站 Logo {Uri.UnescapeDataString(url.AbsolutePath)} 匯入失敗。{ex.Message}");
            return null;
        }
    }

    // 舊站網址 → 新站網址：先找選單，再找已匯入的文章／商品詳細頁。
    internal static string? FindRoute(SiteLayoutResult result, Uri target) =>
        FindMenuRoute(result, target) ?? FindDetailRoute(result, target);

    // 舊站詳細頁網址只靠 id（或只有 sub_id 的頁面）識別，au_id／sub_id 不同仍是同一篇（SiteArticleDetector.ArticleKey）。
    internal static string? FindDetailRoute(SiteLayoutResult result, Uri target) =>
        target.Scheme is "http" or "https" && DetailKey(target) is { } key ? result.DetailRoutes.GetValueOrDefault(key) : null;

    // 記下匯入後的文章／商品網址：/{OrgName}/{列表選單路徑}/article|product/{id}，和前台目錄元件產生的連結相同；
    // 找不到列表選單時文章用 /{OrgName}/article/{id}、商品用 /{OrgName}/search/product/{id}（前台搜尋結果用的寫法）。
    internal static void RegisterDetail(SiteLayoutResult result, Uri articleUrl, Uri? listUrl, bool isProduct, long id)
    {
        result.ClaimedRemovalTargets.Add((isProduct ? "Product" : "Article", id));
        if (DetailKey(articleUrl) is not { } key) return;
        var option = isProduct ? "product" : "article";
        var listRoute = listUrl == null ? null : FindMenuRoute(result, listUrl);
        var orgName = result.OrgName?.Trim('/');
        var root = string.IsNullOrEmpty(orgName) ? "" : "/" + orgName;
        result.DetailRoutes[key] = listRoute != null && !listRoute.EndsWith('/')
            ? $"{listRoute}/{option}/{id}"
            : isProduct ? $"{root}/search/product/{id}" : $"{root}/article/{id}";
    }

    internal static string? DetailKey(Uri url)
    {
        if (SiteArticleDetector.ArticleKey(url) is not { } key) return null;
        return SiteCrawlStateStore.NormalizeHost(url.IdnHost) + "|" + key;
    }

    // 先完全比對；對不到時再去掉分頁參數比對（列表第 2 頁也是同一個選單）。
    internal static string? FindMenuRoute(SiteLayoutResult result, Uri target)
    {
        if (target.Scheme is not ("http" or "https")) return null;
        var menu = FindMenu(result, LinkKey(target, dropPage: false), dropPage: false)
            ?? FindMenu(result, LinkKey(target, dropPage: true), dropPage: true);
        if (menu == null) return null;
        // 前台路由是 /{OrgName}/{RouterName}，少了 OrgName 會被當成站台名稱而 404（系統內既有頁面連結也都帶 OrgName）。
        var orgName = result.OrgName?.Trim('/');
        return string.IsNullOrEmpty(orgName) ? "/" + menu.RouterName : $"/{orgName}/{menu.RouterName}";
    }

    private static ImportedSiteMenu? FindMenu(SiteLayoutResult result, string key, bool dropPage) =>
        result.ImportedMenus.FirstOrDefault(menu => menu.SourceUrl != null
            && LinkKey(new Uri(menu.SourceUrl), dropPage) == key);

    // 連結比對用的網址識別：同一頁在舊站常有不同寫法，都要對到同一個選單（使用者 2026-11-18 要求連到已匯入頁面的連結一律改成新站）。
    // 不分 http／https、有沒有 www、路徑大小寫（舊站是 IIS）、參數名稱大小寫與順序；結尾的 index／default 預設檔名視同目錄
    // （/tw/ 與 /tw/index.asp 是同一頁）；追蹤參數由 SiteLayoutScanner.Normalize 拿掉。
    private static string LinkKey(Uri uri, bool dropPage)
    {
        var normalized = SiteLayoutScanner.Normalize(uri);
        var host = normalized.IdnHost.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal)) host = host[4..];
        var path = DefaultDocument.Replace(Uri.UnescapeDataString(normalized.AbsolutePath).ToLowerInvariant(), "/");
        var query = normalized.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(item =>
            {
                var parts = item.Split('=', 2);
                var name = Uri.UnescapeDataString(parts[0]).ToLowerInvariant();
                return (Name: name, Item: parts.Length == 2 ? name + "=" + parts[1] : name);
            })
            .Where(item => !dropPage || item.Name != PageParameter)
            .OrderBy(item => item.Item, StringComparer.Ordinal)
            .Select(item => item.Item);
        return host + path + "?" + string.Join("&", query);
    }

    internal static string ImageSource(HtmlNode image)
    {
        // 舊站縮圖常是 src＝真圖、data-src＝holder.js 佔位，這時要用 src。
        var src = image.GetAttributeValue("data-src", "");
        if (src.Length > 0 && !src.Contains(PlaceholderPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase)) return src;
        var candidates = image.GetAttributeValue("srcset", "");
        if (candidates.Length > 0 && !candidates.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var candidate = candidates.Split(',').Last().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (candidate.Length > 0) return candidate[0];
        }
        src = image.GetAttributeValue("src", "");
        if (src.Length > 0) return src;
        var style = HtmlEntity.DeEntitize(image.GetAttributeValue("style", ""));
        return Regex.Match(style, """background-image\s*:\s*url\(\s*['"]?(?<url>[^'")]+)""",
            RegexOptions.IgnoreCase).Groups["url"].Value.Trim();
    }

    internal async Task<string?> ImportImageAsync(string src, Uri baseUrl, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        await checkpoint();
        src = HtmlEntity.DeEntitize(src).Trim();
        if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return await files.ImportCrawlDataImageAsync(src, token);
            }
            catch (InvalidOperationException ex)
            {
                result.AddError($"{baseUrl.PathAndQuery} 的內嵌圖片匯入失敗。{ex.Message}");
                return null;
            }
        }
        if (!Uri.TryCreate(baseUrl, src, out var uri) || src.Length == 0)
            return null;
        // holder.js 佔位圖（例 data-src="holder.js/160x120"）不是真的圖片，舊站沒放圖時才會出現，直接略過。
        if (uri.AbsolutePath.Contains(PlaceholderPath, StringComparison.OrdinalIgnoreCase))
            return null;
        try
        {
            return await files.ImportCrawlImageAsync(new UriBuilder(uri) { Fragment = "" }.Uri,
                baseUrl.IdnHost, http, token);
        }
        catch (Exception ex) when (!token.IsCancellationRequested &&
            ex is HttpRequestException or InvalidOperationException or OperationCanceledException or ArgumentException)
        {
            result.AddError($"圖片 {uri.AbsolutePath} 匯入失敗。{ex.Message}");
            return null;
        }
    }
}
