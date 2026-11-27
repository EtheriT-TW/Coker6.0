using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.SiteCrawl;

// 只有真的需要時才開瀏覽器；瀏覽器開不起來時退回純 HTTP，不讓整個匯入失敗。
internal sealed class SiteCrawlRenderSession(ISiteCrawlBrowserFactory factory, string host,
    SiteLayoutResult result, ILogger logger) : IAsyncDisposable
{
    private const int ShellTextLimit = 200;
    private const string EmptyLegacyContent =
        "//*[@id='cont']//*[contains(concat(' ',normalize-space(@class),' '),' contItem ') or @id='result_box'][not(*)][not(normalize-space())]";
    private const string EmptyScrollList = "//*[@id='cont']//*[@id='result_box'][not(*)]";
    private const string VisibleText = ".//text()[not(ancestor::script or ancestor::style or ancestor::noscript)]";

    private ISiteCrawlBrowser? browser;
    private string? browserHost;
    private bool unavailable;

    internal static bool NeedsRender(HtmlDocument document)
    {
        var root = document.DocumentNode;
        if (root.SelectSingleNode(EmptyLegacyContent) != null) return true;
        var body = root.SelectSingleNode("//body");
        if (body == null || body.SelectSingleNode(".//img[not(ancestor::noscript)]") != null) return false;
        var text = body.SelectNodes(VisibleText)?.Sum(node => node.InnerText.Trim().Length) ?? 0;
        return text < ShellTextLimit;
    }

    internal static bool NeedsScroll(HtmlDocument document) =>
        document.DocumentNode.SelectSingleNode(EmptyScrollList) != null;

    public async Task<HtmlDocument?> TryRenderAsync(Uri uri, bool scrollToEnd, CancellationToken token)
    {
        if (unavailable || !SiteCrawlHttp.IsAllowed(uri, host)) return null;
        try
        {
            // HTTP 跟隨 www 轉址後，瀏覽器要固定在最終主機經檢查的 IP，才能讀到 JS 載入的內容。
            if (browser != null && browserHost != uri.IdnHost)
            {
                var previous = browser;
                browser = null;
                await previous.DisposeAsync();
            }
            browser ??= await factory.OpenAsync(uri.IdnHost, token);
            browserHost = uri.IdnHost;
        }
        catch (Exception ex) when (!token.IsCancellationRequested)
        {
            unavailable = true;
            logger.LogError(ex, "Site crawl browser failed to start for {Host}", uri.IdnHost);
            result.AddError("無法啟動瀏覽器，由 JS 載入的內容不會被讀取，請查看 MVC 日誌。");
            return null;
        }

        try
        {
            var document = new HtmlDocument();
            document.LoadHtml(await browser.RenderAsync(uri, scrollToEnd, token));
            result.PagesRendered++;
            return document;
        }
        catch (Exception ex) when (!token.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Site crawl failed to render {Url}", uri);
            result.AddError($"無法以瀏覽器讀取 {uri.PathAndQuery}。");
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (browser != null) await browser.DisposeAsync();
    }
}
