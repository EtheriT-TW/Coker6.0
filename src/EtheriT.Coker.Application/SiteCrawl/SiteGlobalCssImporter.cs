using System.Text;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.SiteCrawl;

// 把舊站全站共用的自訂樣式寫進後台「自訂CSS」。Bootstrap、Font Awesome 新站已有新版，依使用者決定不搬。
public sealed class SiteGlobalCssImporter(SiteCrawlHttp http, SiteCrawlHtml html, IHtmlSanitizer sanitizer,
    CokerDbContext db, LoginUserData user, ILogger<SiteGlobalCssImporter> logger)
{
    private const string BeginMarker = "/* ===== 網站匯入：舊站樣式（重新匯入會覆蓋此區塊） ===== */";
    private const string EndMarker = "/* ===== 網站匯入：舊站樣式結束 ===== */";
    private const long MaxStylesheetBytes = 2 * 1024 * 1024;
    private static readonly Regex SkippedLibrary = new(@"bootstrap|font-?awesome", RegexOptions.IgnoreCase);
    private static readonly Regex PageRootRule = new(
        @"(?<=(?:^|\}|\*/)\s*)(?:html|body)(?:[.#\[:][^\s,{]*)?(?:\s*,\s*(?:html|body)(?:[.#\[:][^\s,{]*)?)*\s*\{[^{}]*\}",
        RegexOptions.IgnoreCase);
    private static readonly Regex CssUrl = new(@"url\(\s*(['""]?)[^'"")]*\1\s*\)", RegexOptions.IgnoreCase);
    private const string GlobalStyles =
        "//link[@href and contains(translate(@rel,'STYLEHS','stylehs'),'stylesheet')]" +
        " | //style[not(ancestor::*[@id='cont'] or ancestor::main or ancestor::article)]";

    internal async Task ImportAsync(Uri entry, SiteLayoutSelection layout, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        try
        {
            var css = await CollectAsync(entry, layout, result, checkpoint, token);
            if (string.IsNullOrWhiteSpace(css)) return;
            await SaveAsync(css, token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
        {
            logger.LogError(ex, "Site crawl global CSS import failed for {Entry}", entry);
            result.AddError("全站樣式匯入失敗，請查看 MVC 日誌。");
        }
    }

    private async Task<string> CollectAsync(Uri entry, SiteLayoutSelection layout, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        await checkpoint();
        var reply = await http.GetAsync(entry, entry.IdnHost, false, MaxStylesheetBytes, token);
        HtmlDocument document;
        await using (var content = reply.Content)
            document = SiteLayoutScanner.ReadDocument(content, reply.Charset);

        var footerStyles = (layout.FooterPage?.Footer?.SelectNodes(".//style") ?? Enumerable.Empty<HtmlNode>())
            .Select(node => node.InnerHtml.Trim()).ToHashSet(StringComparer.Ordinal);
        var css = new StringBuilder();
        foreach (var node in document.DocumentNode.SelectNodes(GlobalStyles) ?? Enumerable.Empty<HtmlNode>())
        {
            await checkpoint();
            if (node.Name == "style")
            {
                var text = node.InnerHtml.Trim();
                if (text.Length == 0 || footerStyles.Contains(text)) continue;
                css.AppendLine(await html.RewriteCssUrlsAsync(StripBodyBackgrounds(StripHtmlComments(text)), reply.Url, result, checkpoint, token));
                continue;
            }
            var sheet = await ReadStylesheetAsync(node, reply.Url, result, token);
            if (sheet == null) continue;
            css.AppendLine($"/* 來源：{sheet.Value.Url.AbsolutePath} */");
            css.AppendLine(await html.RewriteCssUrlsAsync(StripBodyBackgrounds(sheet.Value.Css), sheet.Value.Url, result, checkpoint, token));
            result.StylesheetsImported++;
        }
        return css.ToString();
    }

    private async Task<(Uri Url, string Css)?> ReadStylesheetAsync(HtmlNode link, Uri pageUrl,
        SiteLayoutResult result, CancellationToken token)
    {
        if (!Uri.TryCreate(pageUrl, HtmlEntity.DeEntitize(link.GetAttributeValue("href", "")).Trim(), out var url)
            || SkippedLibrary.IsMatch(url.AbsolutePath))
            return null;
        if (!SiteCrawlHttp.IsAllowed(url, pageUrl.IdnHost))
        {
            result.AddError($"外部樣式檔 {url.Host}{url.AbsolutePath} 沒有匯入。");
            return null;
        }
        try
        {
            var bytes = await http.GetResourceAsync(url, MaxStylesheetBytes, token);
            return (url, StripHtmlComments(Encoding.UTF8.GetString(bytes).TrimStart('﻿')));
        }
        catch (Exception ex) when (!token.IsCancellationRequested &&
            ex is HttpRequestException or InvalidOperationException or OperationCanceledException)
        {
            result.AddError($"樣式檔 {url.AbsolutePath} 匯入失敗。{ex.Message}");
            return null;
        }
    }

    private async Task SaveAsync(string css, CancellationToken token)
    {
        var websiteId = await user.GetWebsiteId();
        var orgName = await user.GetWebsiteOrgName();
        var website = await db.Websites.FirstOrDefaultAsync(x => x.Id == websiteId, token)
            ?? throw new InvalidOperationException("找不到目前網站。");
        // 前台 /upload 已指向該站目錄，全站 CSS 不會再被改寫路徑，所以存前台用的網址。
        var imported = sanitizer.SanitizePublicCss(css).Replace($"/upload/{orgName}/", "/upload/");
        website.Css = MergeBlock(website.Css ?? "", $"{BeginMarker}\n{imported.Trim()}\n{EndMarker}");
        await user.SaveChanges(website);
    }

    private static string MergeBlock(string existing, string block)
    {
        var start = existing.IndexOf(BeginMarker, StringComparison.Ordinal);
        var end = start < 0 ? -1 : existing.IndexOf(EndMarker, start, StringComparison.Ordinal);
        if (start >= 0 && end >= 0)
            return existing[..start] + block + existing[(end + EndMarker.Length)..];
        return string.IsNullOrWhiteSpace(existing) ? block : existing.TrimEnd() + "\n\n" + block;
    }

    private static string StripHtmlComments(string css) => css.Replace("<!--", "").Replace("-->", "");

    // 使用者決定（2026-11-13）：html／body 的背景圖不搬，新站版型自有背景；同一條規則的其他屬性照搬。
    // 例 body{background:url(/upload/head/x.jpg) no-repeat top;} → body{background: no-repeat top;}
    private static string StripBodyBackgrounds(string css) =>
        PageRootRule.Replace(css, rule => CssUrl.Replace(rule.Value, ""));
}
