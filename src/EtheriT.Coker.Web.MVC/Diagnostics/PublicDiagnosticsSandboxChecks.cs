using System.Net;
using System.Text;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task PublicDiagnosticsRegression(BackofficeSandbox box)
    {
        foreach (var address in new[] { "127.0.0.1", "10.0.0.1", "172.16.0.1", "192.168.1.1", "169.254.169.254", "100.64.0.1", "::1", "::ffff:127.0.0.1", "fc00::1", "2002:7f00:1::" })
            Require(!PublicProbeClient.IsPublicAddress(IPAddress.Parse(address)), "Private destination was accepted.");
        Require(PublicPageChecks.EmbedKind(new Uri("https://youtube.com.attacker.invalid/embed/aaaaaaaaaaa")) == "外部嵌入", "Lookalike provider was trusted.");
        var mp4 = "https://example.invalid/upload/test.MP4?autoplay=1";
        Require(PublicPageChecks.EmbedKind(new Uri(mp4)) == "影片檔案"
            && PublicBrowserChecks.EmbedFinding(mp4, "", true, null, 200).Status == PublicCheckStatus.Passed,
            "Valid MP4 was treated as an invalid YouTube URL.");
        Require(PublicBrowserChecks.EmbedFinding(mp4, "媒體解碼失敗，媒體錯誤代碼 4", false, null, 200).Status == PublicCheckStatus.Failed,
            "Real media decoder error was missed.");
        var facebook = new Uri("https://www.facebook.com/plugins/video.php?href=https%3A%2F%2Fwww.facebook.com%2Fwatch%2F%3Fv%3D123456%26access_token%3Dsecret");
        Require(PublicPageChecks.EmbedKind(facebook) == "Facebook 嵌入"
            && PublicPageChecks.FacebookVideoSource(facebook) == "https://www.facebook.com/watch/?v=123456",
            "Facebook provenance was lost or query secrets were exposed.");
        Require(PublicPageChecks.FacebookVideoSource(new Uri("https://www.facebook.com/plugins/video.php?href=https%3A%2F%2Ffacebook.com.attacker.invalid%2Fwatch")) == null,
            "Untrusted original source was accepted.");
        Require(!PublicPageChecks.Display(new Uri("https://example.invalid/maps?key=fixture-secret")).Contains("fixture-secret"), "Query secret leaked.");
        Require(PublicProbeClient.IsReadOnlyVideoRequest(new Uri("https://www.youtube.com/youtubei/v1/player"), "POST")
            && !PublicProbeClient.IsReadOnlyVideoRequest(new Uri("https://www.youtube.com/youtubei/v1/delete"), "POST")
            && !PublicProbeClient.IsReadOnlyVideoRequest(new Uri("https://www.youtube.com.attacker.invalid/youtubei/v1/player"), "POST"), "Video metadata allowlist expanded to unsafe endpoints.");
        var valid = PublicBrowserChecks.ImageFinding(new("https://example.invalid/photo.avif", true, false), null, 200);
        Require(valid.Status == PublicCheckStatus.Passed, "Decoded AVIF was left for manual confirmation.");
        Require(PublicBrowserChecks.ImageFinding(new("data:image/png;base64,fixture", true, false), null, 0).Status == PublicCheckStatus.Passed, "Decoded inline image was rejected.");
        Require(PublicBrowserChecks.ImageFinding(new("https://example.invalid/broken", false, false), null, 200).Status == PublicCheckStatus.Failed, "Broken image with HTTP 200 passed.");
        Require(PublicBrowserChecks.ImageFinding(new("https://example.invalid/missing", false, false), null, 404).Status == PublicCheckStatus.Failed, "Missing image passed.");
        Require(PublicBrowserChecks.ImageFinding(new("https://example.invalid/missing-placeholder", true, false), null, 404).Status == PublicCheckStatus.Failed, "A decoded 404 placeholder passed.");
        Require(PublicBrowserChecks.ImageFinding(new("https://example.invalid/blocked", false, false), null, 403).Status == PublicCheckStatus.NeedsReview, "Authentication block was misreported.");
        Require(PublicBrowserChecks.ImageFinding(new("https://example.invalid/large", false, false), "limit", 0).Status == PublicCheckStatus.NotRun, "Internal limit was reported as a website failure.");
        Require(PublicBrowserChecks.EmbedFinding("https://www.google.com/maps/embed", "map", true, null, 200).Status == PublicCheckStatus.Passed, "Loaded map was left for manual confirmation.");
        Require(PublicBrowserChecks.EmbedFinding("https://www.google.com/maps/embed", "gm-err-container invalid API key", false, null, 200).Status == PublicCheckStatus.Failed, "Map error was missed.");
        Require(PublicBrowserChecks.EmbedFinding("https://www.facebook.com/plugins/page.php", "post", true, null, 200).Status == PublicCheckStatus.Passed, "Loaded Facebook content was left for manual confirmation.");
        Require(PublicBrowserChecks.EmbedFinding("https://www.facebook.com/plugins/page.php", "Log in to continue", false, null, 200).Status == PublicCheckStatus.NeedsReview, "Login gate passed.");
        Require(PublicBrowserChecks.EmbedFinding("https://www.youtube.com/embed/aaaaaaaaaaa", "Video unavailable", false, null, 200).Status == PublicCheckStatus.Failed, "Video error was missed.");
        var handler = new PublicFixtureHandler();
        using var client = new PublicProbeClient(new HttpClient(handler));
        var origin = new Uri("https://example.invalid/");
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "https://example.invalid/Search", "https://example.invalid/redirect", "https://example.invalid/missing" };
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "site-prefix:https://example.invalid/site/", "detail-prefix:https://example.invalid/site/search/product/" };
        Require(PublicBrowserChecks.AllowedPage(new Uri("https://example.invalid/site/Brandstory"), routes)
            && PublicBrowserChecks.AllowedPage(new Uri("https://example.invalid/site/search/product/999"), routes)
            && !PublicBrowserChecks.AllowedPage(new Uri("https://example.invalid/site/Member"), routes), "Link scope confused sampling with enabled routes.");
        var redirect = await PublicBrowserChecks.CheckLinkAsync(new("https://example.invalid/redirect", "/redirect"), origin, allowed, client, CancellationToken.None);
        Require(redirect?.Status == PublicCheckStatus.Passed, "Normal cross-origin redirect was left for manual confirmation.");
        var search = await PublicBrowserChecks.CheckLinkAsync(new("https://example.invalid/Search?q=fixture", "/Search?q=fixture"), origin, allowed, client, CancellationToken.None);
        Require(search?.Status == PublicCheckStatus.Passed, "Read-only query was not tested.");
        var missing = await PublicBrowserChecks.CheckLinkAsync(new("https://example.invalid/missing", "/missing"), origin, allowed, client, CancellationToken.None);
        Require(missing?.Status == PublicCheckStatus.Failed, "Missing link was not detected.");
        var phone = await PublicBrowserChecks.CheckLinkAsync(new("https://example.invalid/site/tel:0980-123", "site/tel:0980-123"), origin, allowed, client, CancellationToken.None);
        Require(phone?.Status == PublicCheckStatus.Failed, "Malformed telephone link was missed.");
        await PublicBrowserChecks.CheckLinkAsync(new("https://example.invalid/unsubscribe?token=fixture", "/unsubscribe?token=fixture"), origin, allowed, client, CancellationToken.None);
        Require(handler.Methods.All(method => method is "GET" or "HEAD") && !handler.Paths.Contains("/unsubscribe"), "Unsafe operation was requested.");
        var target = new PublicTarget("隔離頁面", origin.AbsoluteUri);
        var result = PublicPageChecks.Identify(target, new[] { valid, redirect!, missing!, phone! });
        Require(result.Findings.Select(item => item.Id).Distinct().Count() == result.Findings.Count, "Finding IDs were duplicated.");
        var root = box.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
        var store = new PublicResultStore(root);
        await store.SaveAsync(1, 1, "fixture", result);
        var restored = await new PublicResultStore(root).LoadAsync(1, 1, "fixture");
        Require(restored?.Pages[target.Url].Findings.Count == result.Findings.Count, "Public history was not restored.");
        Require(await store.LoadAsync(2, 1, "fixture") == null && await store.LoadAsync(1, 2, "fixture") == null
            && await store.LoadAsync(1, 1, "changed") == null, "History leaked across scope.");
        Require(await new BackofficeResultStore(root).LoadAsync(1, 1, null) == null, "Public history contaminated backoffice history.");
    }
    private sealed class PublicFixtureHandler : HttpMessageHandler
    {
        public List<string> Methods { get; } = new();
        public List<string> Paths { get; } = new();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Methods.Add(request.Method.Method);
            var path = request.RequestUri!.AbsolutePath;
            Paths.Add(path);
            var response = new HttpResponseMessage(path == "/missing" ? HttpStatusCode.NotFound : path == "/redirect" ? HttpStatusCode.Found : HttpStatusCode.OK)
            { Content = new StringContent("fixture", Encoding.UTF8, "text/html") };
            if (path == "/redirect") response.Headers.Location = new Uri("https://maps.google.com/destination");
            return Task.FromResult(response);
        }
    }
}
