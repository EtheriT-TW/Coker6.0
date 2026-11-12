using Microsoft.Playwright;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

// One browser per background run; every page still receives a fresh isolated context.
public sealed class PublicBrowserSession : IAsyncDisposable
{
    private IPlaywright? playwright;
    private IBrowser? browser;
    public async Task<IBrowser> GetAsync(IConfiguration configuration)
    {
        if (browser != null) return browser;
        playwright = await Playwright.CreateAsync();
        var channel = configuration["Diagnostics:BrowserChannel"] ?? "msedge";
        if (channel is not ("msedge" or "chrome" or "chromium")) throw new InvalidOperationException("Unsupported diagnostics browser channel.");
        browser = await playwright.Chromium.LaunchAsync(new()
        {
            Channel = channel == "chromium" ? null : channel, Headless = true, ChromiumSandbox = true,
            // Every browser request is fulfilled by the DNS-pinned HTTP client.
            // Any un-intercepted browser traffic has no functioning proxy.
            Proxy = new Proxy { Server = "http://127.0.0.1:9" },
            Args = new[] { "--disable-quic", "--disable-background-networking" }, Timeout = 20000
        });
        return browser;
    }
    public async ValueTask DisposeAsync()
    {
        try { if (browser != null) await browser.DisposeAsync(); }
        finally { playwright?.Dispose(); }
    }
}
