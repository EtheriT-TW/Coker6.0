using System.Net.Sockets;
using EtheriT.Coker.Application.SiteCrawl;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace EtheriT.Coker.Web.MVC.Services.SiteCrawl;

public sealed class PlaywrightSiteCrawlBrowserFactory(SiteCrawlHttp http, IOptions<SiteCrawlOptions> options)
    : ISiteCrawlBrowserFactory
{
    public async Task<ISiteCrawlBrowser> OpenAsync(string host, CancellationToken token)
    {
        var addresses = await SiteCrawlHttp.ResolvePublicAsync(host, token);
        var address = addresses.OrderBy(item => item.AddressFamily == AddressFamily.InterNetworkV6).First();
        var pinned = address.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{address}]" : address.ToString();

        var playwright = await Playwright.CreateAsync().WaitAsync(token);
        try
        {
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Channel = options.Value.BrowserChannel,
                Headless = true,
                ChromiumSandbox = true,
                // 瀏覽器只能連到已檢查過的來源 IP；轉址或內嵌資源想連別的主機都解析失敗。
                Args = [$"--host-resolver-rules=MAP {host} {pinned}, MAP * ~NOTFOUND"]
            }).WaitAsync(token);
            return new PlaywrightSiteCrawlBrowser(playwright, browser, host, http);
        }
        catch
        {
            playwright.Dispose();
            throw;
        }
    }
}

internal sealed class PlaywrightSiteCrawlBrowser(IPlaywright playwright, IBrowser browser, string host, SiteCrawlHttp http)
    : ISiteCrawlBrowser
{
    private const int NavigationTimeoutMs = 30_000;
    private const int NetworkIdleTimeoutMs = 10_000;
    private const long MaxScriptBytes = 5 * 1024 * 1024;
    private const int ScrollWaitMs = 1_200;
    private const int ScrollStableRounds = 2;
    private const int MaxScrollRounds = 1_000;

    // 圖片不能直接擋：舊站 img 的 onerror 會把 src 換成 notFound.png，內文網址被改掉且不斷重試。
    private static readonly byte[] BlankImage =
        Convert.FromBase64String("R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

    public async Task<string> RenderAsync(Uri uri, bool scrollToEnd, CancellationToken token)
    {
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = false,
            ServiceWorkers = ServiceWorkerPolicy.Block
        }).WaitAsync(token);
        await context.RouteAsync("**/*", route => HandleRequestAsync(route, token)).WaitAsync(token);
        var page = await context.NewPageAsync().WaitAsync(token);
        return await LoadAsync(page, uri, scrollToEnd).WaitAsync(token);
    }

    private static async Task<string> LoadAsync(IPage page, Uri uri, bool scrollToEnd)
    {
        var response = await page.GotoAsync(uri.AbsoluteUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = NavigationTimeoutMs
        });
        if (response is not { Ok: true })
            throw new InvalidOperationException($"來源回應 HTTP {response?.Status}。");

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
            {
                Timeout = NetworkIdleTimeoutMs
            });
        }
        catch (TimeoutException)
        {
            // 有輪詢或長連線的頁面永遠等不到閒置，用當下已載入的內容。
        }
        if (scrollToEnd) await ScrollToEndAsync(page);
        return await page.ContentAsync();
    }

    private static async Task ScrollToEndAsync(IPage page)
    {
        var last = -1;
        var stable = 0;
        for (var round = 0; round < MaxScrollRounds && stable < ScrollStableRounds; round++)
        {
            await page.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");
            await page.Mouse.WheelAsync(0, 3_000);
            await page.WaitForTimeoutAsync(ScrollWaitMs);
            var links = await page.EvaluateAsync<int>("() => document.querySelectorAll('a[href]').length");
            stable = links == last ? stable + 1 : 0;
            last = links;
        }
    }

    private async Task HandleRequestAsync(IRoute route, CancellationToken token)
    {
        var request = route.Request;
        try
        {
            if (request.ResourceType == "image")
                await route.FulfillAsync(new RouteFulfillOptions { Status = 200, BodyBytes = BlankImage, ContentType = "image/gif" });
            else if (request.ResourceType is "media" or "font"
                || !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                await route.AbortAsync();
            else if (SiteCrawlHttp.IsAllowed(uri, host))
                await route.ContinueAsync();
            else if (request.ResourceType == "script" && request.Method == "GET")
                await FulfillExternalScriptAsync(route, uri, token);
            else
                await route.AbortAsync();
        }
        catch (PlaywrightException)
        {
            // 頁面已讀取完畢並關閉時，尚未完成的請求會丟這個例外，不影響結果。
        }
    }

    private async Task FulfillExternalScriptAsync(IRoute route, Uri uri, CancellationToken token)
    {
        byte[] body;
        try
        {
            body = await http.GetResourceAsync(uri, MaxScriptBytes, token);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or OperationCanceledException)
        {
            await route.AbortAsync();
            return;
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Status = 200,
            BodyBytes = body,
            ContentType = "text/javascript",
            Headers = new Dictionary<string, string> { ["Access-Control-Allow-Origin"] = "*" }
        });
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await browser.DisposeAsync();
        }
        finally
        {
            playwright.Dispose();
        }
    }
}
