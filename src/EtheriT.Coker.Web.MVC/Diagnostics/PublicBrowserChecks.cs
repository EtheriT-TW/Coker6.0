using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed record PublicBrowserImage(string Url, bool Loaded, bool Pending);
public sealed record PublicBrowserLink(string Url, string Raw);
public sealed record PublicBrowserVideo(string Url, string Location);
public sealed record PublicBrowserSnapshot(List<PublicBrowserImage> Images, List<PublicBrowserLink> Links, List<PublicBrowserVideo> Videos, bool Limited);

public static class PublicBrowserChecks
{
    private static readonly SemaphoreSlim BrowserGate = new(1, 1);

    public static PublicFinding ImageFinding(PublicBrowserImage image, string? blocked, int status)
    {
        var source = Uri.TryCreate(image.Url, UriKind.Absolute, out var uri) && PublicProbeClient.IsHttp(uri)
            ? PublicPageChecks.Display(uri) : "頁面內嵌圖片";
        if (string.IsNullOrWhiteSpace(image.Url)) return new("圖片顯示", source, PublicCheckStatus.NotRun, "動態圖片尚未提供來源，尚未完成載入驗證。");
        if (status is 401 or 403 or 429) return new("圖片顯示", source, PublicCheckStatus.NeedsReview, $"圖片伺服器回應 {status}，可能需要登入或封鎖自動存取。");
        if (status is 404 or 410 or >= 500) return new("圖片顯示", source, PublicCheckStatus.Failed, $"圖片來源回應 HTTP {status}，無法正常取得原圖。");
        if (image.Loaded) return new("圖片顯示", source, PublicCheckStatus.Passed, "瀏覽器已成功載入並解碼圖片，圖片尺寸正常。");
        if (blocked != null) return new("圖片顯示", source, PublicCheckStatus.NotRun, "資源受檢測環境限制而未載入：" + blocked);
        if (image.Pending) return new("圖片顯示", source, PublicCheckStatus.NotRun, "等待時間內尚未完成載入，未將此圖片視為通過。");
        return new("圖片顯示", source, PublicCheckStatus.Failed, "瀏覽器無法解碼或載入圖片，已偵測到掉圖。" + (status >= 400 ? $" HTTP {status}。" : ""));
    }

    public static PublicFinding EmbedFinding(string url, string text, bool hasContent, string? blocked, int status)
    {
        var uri = new Uri(url);
        var kind = PublicPageChecks.EmbedKind(uri);
        var source = PublicPageChecks.Display(uri);
        if (status is 404 or 410 or >= 500 || Regex.IsMatch(text,
            @"video (?:is )?unavailable|this video (?:is not|isn't) available|this content (?:is not|isn't) available|媒體解碼失敗|影片格式不支援|影片無法播放|無法播放影片|影片已遭移除|這段影片無法播放|(?:這則|此|這個)內容目前無法顯示|Google Maps Platform rejected|This page can't load Google Maps correctly|gm-err-container|Oops! Something went wrong|這個網頁無法正確載入 Google 地圖|ERR_BLOCKED_BY_RESPONSE|refused to connect|拒絕連線|can't be reached", RegexOptions.IgnoreCase))
        {
            var evidence = Regex.Replace(text.Replace("gm-err-container", ""), @"https?://[^\s]+", "[網址]");
            evidence = Regex.Replace(evidence, @"\s+", " ").Trim();
            if (evidence.Length > 180) evidence = evidence[..180] + "…";
            return new(kind, source, PublicCheckStatus.Failed, "嵌入實際顯示錯誤" + (status >= 400 ? $"（HTTP {status}）" : "") + "：" + evidence);
        }
        if (blocked != null) return new(kind, source, PublicCheckStatus.NotRun, "嵌入受檢測環境限制：" + blocked);
        if (status is 401 or 403 or 429 || (!hasContent && Regex.IsMatch(text, @"log in|sign in|登入|驗證碼|captcha|consent|同意.*cookie", RegexOptions.IgnoreCase)))
            return new(kind, source, PublicCheckStatus.NeedsReview, "第三方要求登入、同意或驗證，或拒絕自動存取，無法替使用者完成此步驟。");
        if (hasContent) return new(kind, source, PublicCheckStatus.Passed, kind == "影片檔案"
            ? "瀏覽器已讀取影片資料與影像尺寸，未偵測媒體錯誤；不代表整段影片皆已驗證。"
            : "瀏覽器已載入嵌入內容，且未偵測到供應商的錯誤畫面；不代表所有互動皆已驗證。");
        return new(kind, source, PublicCheckStatus.Failed, "等待後嵌入仍為空白或未完成載入，未顯示可用內容。");
    }

    public static async Task<PublicPageResult> RunAsync(PublicTarget target, IReadOnlySet<string> allowedLinks,
        IConfiguration configuration, ILogger logger, CancellationToken token, bool functionScan = false, PublicBrowserSession? session = null,
        long websiteId = 0, IReadOnlySet<string>? enabledFeatures = null,
        Func<string, CancellationToken, Task<PublicFinding>>? isolated = null,
        Func<PublicPageResult, Task>? publish = null, Func<CancellationToken, Task<string>>? quickCartFixture = null)
    {
        var findings = new List<PublicFinding>();
        if (!await BrowserGate.WaitAsync(0, token))
            return PublicPageChecks.Identify(target, new[] { new PublicFinding("瀏覽器檢測", target.Url, PublicCheckStatus.NotRun, "瀏覽器正在檢測其他頁面，請稍後複測。") }) with { StopRun = true };
        try
        {
            await using var ownedSession = session == null ? new PublicBrowserSession() : null;
            var browser = await (session ?? ownedSession!).GetAsync(configuration);
            await using var context = await browser.NewContextAsync(new()
            { ServiceWorkers = ServiceWorkerPolicy.Block, AcceptDownloads = false, ViewportSize = new() { Width = 1365, Height = 900 } });
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
            lifetime.CancelAfter(TimeSpan.FromSeconds(functionScan ? 180 : 45));
            var cancellation = lifetime.Token;
            using var abort = cancellation.Register(() => { _ = context.CloseAsync(); });
            var readPolicy = websiteId > 0 && enabledFeatures != null
                ? new PublicReadOnlyPolicy(new Uri(target.Url), websiteId, enabledFeatures) : null;
            using var client = new PublicProbeClient(readPolicy);
            var statuses = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
            var blocked = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            var errors = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            var restrictedFrames = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            var networkSlots = new SemaphoreSlim(6); // Route callbacks may finish while the context is closing.
            var requests = 0;
            Task<string>? productFixture = null;
            long received = 0;
            bool ReserveFixture(int bytes) => Interlocked.Increment(ref requests) <= 300
                && Interlocked.Add(ref received, bytes) <= 64 * 1024 * 1024;
            await context.AddInitScriptAsync("Object.defineProperty(window,'WebSocket',{value:undefined});Object.defineProperty(window,'EventSource',{value:undefined});Object.defineProperty(window,'RTCPeerConnection',{value:undefined});Object.defineProperty(window,'WebTransport',{value:undefined});");
            if (functionScan) await context.AddInitScriptAsync(PublicFeatureChecks.ObservationScript);
            await context.RouteAsync("**/*", async route =>
            {
                var request = route.Request;
                if (functionScan && quickCartFixture != null && enabledFeatures?.Contains("ProductList") == true
                    && request.Method == "GET" && Uri.TryCreate(request.Url, UriKind.Absolute, out var productUri)
                    && PublicPageChecks.SameOrigin(new Uri(target.Url), productUri)
                    && productUri.AbsolutePath.TrimEnd('/').Equals("/api/Product/GetMainDisplayOne", StringComparison.OrdinalIgnoreCase))
                {
                    // This public display getter can sanitize and persist HTML.
                    // Exercise the UI with the real service's isolated DTO instead.
                    if (!ReserveFixture(0)) { await route.AbortAsync(); return; }
                    try
                    {
                        productFixture ??= quickCartFixture(cancellation);
                        var json = await productFixture;
                        if (Interlocked.Add(ref received, System.Text.Encoding.UTF8.GetByteCount(json)) > 64 * 1024 * 1024)
                        { await route.AbortAsync(); return; }
                        await route.FulfillAsync(new() { Status = 200, ContentType = "application/json", Body = json });
                    }
                    catch (Exception error)
                    {
                        logger.LogWarning(error, "Could not prepare isolated product data");
                        await route.FulfillAsync(new() { Status = 503, ContentType = "application/json", Body = "{\"message\":\"隔離資料未就緒\"}" });
                    }
                    return;
                }
                // Captcha endpoints record account events. Supply only an isolated
                // image fixture for client-side refresh/validation, never call them.
                if (functionScan && Uri.TryCreate(request.Url, UriKind.Absolute, out var captchaUri)
                    && PublicPageChecks.SameOrigin(new Uri(target.Url), captchaUri)
                    && captchaUri.AbsolutePath.Equals("/api/Captcha/index", StringComparison.OrdinalIgnoreCase)
                    && request.Method == "GET")
                {
                    if (!ReserveFixture(100)) { await route.AbortAsync(); return; }
                    await route.FulfillAsync(new() { Status = 200, ContentType = "image/png", BodyBytes = Convert.FromBase64String(
                        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aYZ0AAAAASUVORK5CYII=") });
                    return;
                }
                if (!functionScan && (request.ResourceType is "media" || request.IsNavigationRequest && request.Frame.ParentFrame != null))
                { await route.AbortAsync(); return; }
                if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || !PublicProbeClient.IsHttp(uri)
                    || readPolicy?.BlocksUnreviewedGet(uri, request.Method) == true
                    || request.Method is not ("GET" or "HEAD") && !(functionScan && PublicProbeClient.IsReadOnlyVideoRequest(uri, request.Method))
                        && readPolicy?.Allows(uri, request.Method, request.PostDataBuffer) != true
                    || Interlocked.Increment(ref requests) > 300
                    || Interlocked.Read(ref received) > 64 * 1024 * 1024
                    || request.IsNavigationRequest && PublicPageChecks.IsOperationLink(uri)
                    || request.IsNavigationRequest && request.Frame.ParentFrame == null && !PublicPageChecks.SameOrigin(new Uri(target.Url), uri))
                {
                    blocked[request.Url] = "非唯讀操作、非公開網址或超過本頁資源上限";
                    if (request.Method is not ("GET" or "HEAD"))
                    {
                        try { restrictedFrames[request.Frame.Url] = "此嵌入另需本次未開放的請求，尚未完整驗證"; }
                        catch (PlaywrightException) { }
                    }
                    await route.AbortAsync(); return;
                }
                var entered = false;
                try
                {
                    await networkSlots.WaitAsync(cancellation); entered = true;
                    var response = await client.FetchForBrowserAsync(uri, request.Method, request.Headers, cancellation, request.PostDataBuffer);
                    statuses[request.Url] = response.Code;
                    Interlocked.Add(ref received, response.Body.Length);
                    await route.FulfillAsync(new() { Status = response.Code, Headers = response.Headers, BodyBytes = response.Body });
                }
                catch (Exception exception)
                {
                    if (PublicProbeClient.IsPolicyFailure(exception)) blocked[request.Url] = "非公開目的地或資源超過大小限制";
                    else errors[request.Url] = "連線失敗、TLS 錯誤或逾時";
                    try { await route.AbortAsync(); } catch (PlaywrightException) { }
                }
                finally { if (entered) networkSlots.Release(); }
            });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(15000);
            IResponse? navigation;
            try { navigation = await page.GotoAsync(target.Url, new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 35000 }); }
            catch (PlaywrightException)
            {
                token.ThrowIfCancellationRequested();
                if (cancellation.IsCancellationRequested)
                    return PublicPageChecks.Identify(target, new[] { new PublicFinding("頁面載入", target.Url, PublicCheckStatus.NotRun, "已達本頁檢測時間上限，未完成的項目未列通過。") });
                findings.Add(new("頁面載入", target.Url, blocked.ContainsKey(target.Url) ? PublicCheckStatus.NotRun : PublicCheckStatus.Failed,
                    blocked.ContainsKey(target.Url) ? "此目的地不符合公開網路檢測限制。" : "實際瀏覽器無法完成頁面載入。"));
                return PublicPageChecks.Identify(target, findings);
            }
            if (navigation == null || navigation.Status is < 200 or >= 400)
            {
                var code = navigation?.Status ?? 0;
                findings.Add(new("頁面載入", target.Url, code is 401 or 403 or 429 ? PublicCheckStatus.NeedsReview : PublicCheckStatus.Failed,
                    $"實際頁面回應 {code}。"));
                return PublicPageChecks.Identify(target, findings);
            }
            findings.Add(new("頁面載入", target.Url, PublicCheckStatus.Passed, "實際瀏覽器已完成頁面文件載入。"));
            if (functionScan)
                findings.AddRange(await PublicFeatureChecks.RunAsync(page, target, cancellation, enabledFeatures, isolated,
                    publish == null ? null : async progress =>
                        await publish(PublicPageChecks.Identify(target, findings.Concat(progress)) with { Completed = false }), logger));
            var element = await page.EvaluateAsync<JsonElement>(SnapshotScript, !functionScan);
            var snapshot = JsonSerializer.Deserialize<PublicBrowserSnapshot>(element.GetRawText())!;
            foreach (var image in snapshot.Images.DistinctBy(image => image.Url))
                findings.Add(ImageFinding(image, blocked.GetValueOrDefault(image.Url), statuses.GetValueOrDefault(image.Url)));
            if (snapshot.Limited) findings.Add(new("檢測範圍", target.Url, PublicCheckStatus.NotRun, "本頁超過 100 張可見圖片或 100 個不重複連結，其餘未檢測。"));
            if (!functionScan)
            {
                // Links are checked separately with HEAD and a GET fallback; normal public redirects pass.
                using var linkSlots = new SemaphoreSlim(4);
                var linkResults = await Task.WhenAll(snapshot.Links.Select(async link =>
                {
                    await linkSlots.WaitAsync(cancellation);
                    try { return await CheckLinkAsync(link, new Uri(target.Url), allowedLinks, client, cancellation); }
                    finally { linkSlots.Release(); }
                }));
                findings.AddRange(linkResults.OfType<PublicFinding>());
                if (requests > 300 || received > 64 * 1024 * 1024)
                    findings.Add(new("檢測範圍", target.Url, PublicCheckStatus.NotRun, "已達本頁網路上限，受限資源未列通過。"));
                return PublicPageChecks.Identify(target, findings);
            }
            foreach (var video in snapshot.Videos.DistinctBy(item => item.Url).Take(3))
            {
                if (!Uri.TryCreate(new Uri(page.Url), video.Url, out var videoUri) || !PublicProbeClient.IsHttp(videoUri)) continue;
                var kind = PublicPageChecks.EmbedKind(videoUri);
                var embedUrl = videoUri.AbsoluteUri;
                if (kind == "YouTube 嵌入")
                {
                    var videoId = PublicPageChecks.YouTubeId(videoUri);
                    if (videoId == null)
                    { findings.Add(new(kind, PublicPageChecks.Display(videoUri), PublicCheckStatus.Failed, "YouTube 影片來源未包含有效影片識別碼。") { Location = video.Location }); continue; }
                    embedUrl = "https://www.youtube-nocookie.com/embed/" + videoId;
                }
                await page.EvaluateAsync("source => { let frame=[...document.querySelectorAll('iframe')].find(frame => frame.src === source.url); if (!frame) {frame=document.createElement('iframe');frame.src=source.url;frame.width='480';frame.height='270';frame.loading='eager';document.body.appendChild(frame);} frame.dataset.cokerDiagnosticLocation=source.location; }", new { url = embedUrl, location = video.Location });
            }
            // Inspect real child-frame DOM, including cross-origin provider frames.
            var frames = await page.Locator("iframe").ElementHandlesAsync();
            var frameNumber = 0;
            foreach (var frameElement in frames.Take(200))
            {
                try
                {
                if (!await frameElement.IsVisibleAsync()) continue;
                if (frameNumber >= 10)
                { findings.Add(new("檢測範圍", target.Url, PublicCheckStatus.NotRun, "本頁超過十個可見嵌入框，其餘未檢測。")); break; }
                frameNumber++;
                await frameElement.ScrollIntoViewIfNeededAsync();
                var source = await frameElement.GetAttributeAsync("src");
                if (string.IsNullOrWhiteSpace(source)) continue;
                if (!Uri.TryCreate(new Uri(page.Url), source, out var frameUri) || !PublicProbeClient.IsHttp(frameUri)) continue;
                var location = await frameElement.GetAttributeAsync("data-coker-diagnostic-location");
                if (string.IsNullOrWhiteSpace(location)) location = "頁面中的第 " + frameNumber + " 個可見嵌入框";
                PublicFinding Locate(PublicFinding finding) => finding with { Location = location, RelatedUrl = PublicPageChecks.FacebookVideoSource(frameUri) };
                var frame = await frameElement.ContentFrameAsync();
                string text = "";
                var content = false;
                var mediaPending = false;
                if (frame != null)
                {
                    try
                    {
                        var state = await frame.EvaluateAsync<JsonElement>(FrameScript);
                        text = state.GetProperty("Text").GetString() ?? "";
                        content = state.GetProperty("HasContent").GetBoolean();
                        if (PublicPageChecks.EmbedKind(frameUri) == "影片檔案") content = state.GetProperty("MediaLoaded").GetBoolean();
                        mediaPending = state.GetProperty("MediaPending").GetBoolean();
                    }
                    catch (PlaywrightException) { }
                }
                var frameUrl = frame?.Url ?? frameUri.AbsoluteUri;
                if (!content && restrictedFrames.TryGetValue(frameUrl, out var restriction))
                { findings.Add(Locate(new(PublicPageChecks.EmbedKind(frameUri), PublicPageChecks.Display(frameUri), PublicCheckStatus.NotRun, restriction))); continue; }
                if (errors.ContainsKey(frameUrl) || errors.ContainsKey(frameUri.AbsoluteUri))
                { findings.Add(Locate(new(PublicPageChecks.EmbedKind(frameUri), PublicPageChecks.Display(frameUri), PublicCheckStatus.Failed, "嵌入來源實際連線失敗，無法載入內容。"))); continue; }
                var httpStatus = statuses.GetValueOrDefault(frameUrl, statuses.GetValueOrDefault(frameUri.AbsoluteUri));
                var outcome = mediaPending && !content && httpStatus < 400 && !blocked.ContainsKey(frameUri.AbsoluteUri)
                    ? new PublicFinding(PublicPageChecks.EmbedKind(frameUri), PublicPageChecks.Display(frameUri), PublicCheckStatus.NotRun, "影片仍在讀取中，尚未完成媒體載入驗證。")
                    : EmbedFinding(frameUri.AbsoluteUri, text, content,
                    blocked.GetValueOrDefault(frameUrl) ?? blocked.GetValueOrDefault(frameUri.AbsoluteUri),
                    httpStatus);
                findings.Add(Locate(outcome));
                }
                catch (PlaywrightException)
                { findings.Add(new("嵌入載入", target.Url, PublicCheckStatus.NotRun, "檢測途中嵌入框被頁面更新或移除，這個框的結果尚未確認。")); }
            }
            if (frames.Count > 200) findings.Add(new("檢測範圍", target.Url, PublicCheckStatus.NotRun, "本頁超過二百個候選嵌入框，其餘未盤點。"));
            if (requests > 300 || received > 64 * 1024 * 1024)
                findings.Add(new("檢測範圍", target.Url, PublicCheckStatus.NotRun, "已達本頁三百個請求或 64 MB 上限，受限資源未列通過。"));
            return PublicPageChecks.Identify(target, findings);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception) when (token.IsCancellationRequested) { throw new OperationCanceledException(token); }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Public browser diagnostics failed for {Target}", target.Title);
            findings.Add(new("瀏覽器檢測", target.Url, PublicCheckStatus.NotRun, "瀏覽器無法啟動或檢測逾時中止。請確認檢測主機已具備瀏覽器與執行權限；這不是網站錯誤。"));
            return PublicPageChecks.Identify(target, findings) with { StopRun = true };
        }
        finally { BrowserGate.Release(); }
    }

    internal static async Task<PublicFinding?> CheckLinkAsync(PublicBrowserLink link, Uri root, IReadOnlySet<string> allowed,
        PublicProbeClient client, CancellationToken token)
    {
        var raw = link.Raw.Trim();
        if (raw.Length == 0 || raw.StartsWith('#') || Regex.IsMatch(raw, @"^(?:mailto:|tel:|javascript:)", RegexOptions.IgnoreCase)) return null;
        if (!Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) || !PublicProbeClient.IsHttp(uri)) return null;
        var resource = PublicPageChecks.Display(uri);
        if (Regex.IsMatch(uri.AbsolutePath, @"/(?:tel|mailto):", RegexOptions.IgnoreCase))
            return new("連結可用性", resource, PublicCheckStatus.Failed, "電話或郵件連結誤寫成站內路徑，使用者無法正常開啟。請修正連結格式。");
        if (PublicPageChecks.IsOperationLink(uri)) return null; // Do not execute account mutations.
        if (PublicPageChecks.SameOrigin(root, uri) && !AllowedPage(uri, allowed))
            return new("連結可用性", resource, PublicCheckStatus.NotRun, "此目的地不屬於目前啟用的公開頁面，未執行操作。");
        try
        {
            var response = await client.GetAsync(uri, 8 * 1024 * 1024, token, head: true);
            if (response.Code is 405 or 501)
                response = await client.GetAsync(uri, 8 * 1024 * 1024, token, metadataOnly: true);
            var status = response.Code is >= 200 and < 300 ? PublicCheckStatus.Passed
                : response.Code is 401 or 403 or 429 ? PublicCheckStatus.NeedsReview : PublicCheckStatus.Failed;
            return new("連結可用性", resource, status, status == PublicCheckStatus.Passed
                ? "已追蹤公開轉址，目的地 HTTP 回應正常；未驗證內容或互動功能。" : $"目的地回應 HTTP {response.Code}。");
        }
        catch (Exception exception) when (PublicProbeClient.IsPolicyFailure(exception))
        { return new("連結可用性", resource, PublicCheckStatus.NotRun, "目的地不符合公開網路檢測限制。"); }
        catch (Exception exception) when (exception is HttpRequestException or IOException || exception is OperationCanceledException && !token.IsCancellationRequested)
        { return new("連結可用性", resource, PublicCheckStatus.Failed, "實際請求目的地失敗，無法連線或等待逾時。"); }
    }

    internal static bool AllowedPage(Uri uri, IReadOnlySet<string> allowed)
    {
        var path = uri.GetLeftPart(UriPartial.Path);
        if (allowed.Contains(path.TrimEnd('/')) || uri.AbsolutePath.StartsWith("/upload/", StringComparison.OrdinalIgnoreCase)) return true;
        foreach (var rule in allowed)
        {
            var detail = rule.StartsWith("detail-prefix:", StringComparison.Ordinal);
            var page = rule.StartsWith("site-prefix:", StringComparison.Ordinal);
            if (!detail && !page) continue;
            var prefix = rule[(detail ? 14 : 12)..];
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var suffix = Uri.UnescapeDataString(path[prefix.Length..]).TrimEnd('/');
            if (detail && Regex.IsMatch(suffix, @"^[0-9]+$")) return true;
            if (page && Regex.IsMatch(suffix, @"^[\p{L}\p{N}_-]+$")
                && !new[] { "Member", "ShoppingCar", "Search", "Logout", "Checkout" }.Contains(suffix, StringComparer.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private const string SnapshotScript = """
        async inspectImages => {
            const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
            await pause(600);
            const visible = element => element.getClientRects().length > 0 && getComputedStyle(element).visibility !== 'hidden';
            const allImages = [...document.images].filter(visible);
            const images = inspectImages ? allImages.slice(0, 100) : [];
            for (const image of images) {
                image.loading = 'eager';
                image.scrollIntoView({block:'center'});
                await pause(10);
            }
            await Promise.all(images.map(image => Promise.race([image.decode().catch(() => {}), pause(3000)])));

            const links = [...new Map([...document.querySelectorAll('a[href]')].filter(visible).map(a => [a.href, a])).values()];
            const videoElements = (inspectImages ? [] : [...document.querySelectorAll('[data-model-target],.YTmodal_frame,video')])
                .filter(el => visible(el) && !el.closest('template,[hidden],[aria-hidden="true"]'));
            const videos = videoElements.map((el, index) => {
                const url = el.getAttribute('data-model-target') || el.getAttribute('data-link') || el.getAttribute('link')
                    || (el.tagName === 'VIDEO' ? el.currentSrc || el.src || el.querySelector('source')?.src : '') || '';
                let title = el.getAttribute('aria-label') || el.getAttribute('title') || '';
                if (!title || /^YouTube video player$/i.test(title)) title = el.querySelector('img')?.getAttribute('alt') || el.innerText || '';
                title = title.replace(/\s+/g,' ').trim().slice(0,80);
                const region = el.closest('header') ? '頁首' : el.closest('footer') ? '頁尾' : '主要內容';
                return {Url:url, Location:region + '・第 ' + (index + 1) + ' 個影片來源' + (title ? '「' + title + '」' : '')};
            }).filter(video => video.Url);
            return {
                Images: images.map(image => ({Url:image.currentSrc || image.src || '', Loaded:image.complete && image.naturalWidth > 0 && image.naturalHeight > 0, Pending:!image.complete})),
                Links: links.slice(0, 100).map(a => ({Url:a.href, Raw:a.getAttribute('href') || ''})),
                Videos:videos,
                Limited:inspectImages && (allImages.length > 100 || links.length > 100)
            };
        }
        """;

    private const string FrameScript = """
        async () => {
            const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
            for (let attempt = 0; attempt < 8; attempt++) {
                const media = document.querySelector('video');
                if (media ? media.readyState >= 1 || media.error : document.querySelector('.gm-err-container,.gm-err-message,.ytp-error,.ytp-error-content-wrap,canvas,.gm-style') || (document.body?.innerText || '').trim().length > 100) break;
                await pause(1000);
            }
            const body = document.body;
            const text = (body?.innerText || '').slice(0, 16000);
            const error = [...document.querySelectorAll('.gm-err-container,.gm-err-message,.ytp-error,.ytp-error-content-wrap')]
                .find(el => el.getClientRects().length > 0 && getComputedStyle(el).visibility !== 'hidden' && el.textContent.trim());
            const map = document.querySelector('canvas,.gm-style,[role="application"]');
            const image = [...document.images].some(image => image.complete && image.naturalWidth > 32 && image.naturalHeight > 32);
            const video = document.querySelector('video');
            const authWall = /log in to continue|sign in to continue|you must (?:be logged|log)|登入以繼續|before you continue|verify you are human/i.test(text);
            return {Text:video?.error ? '媒體解碼失敗，媒體錯誤代碼 ' + video.error.code : error ? 'gm-err-container ' + error.textContent : text,
                MediaPending:!!video && video.readyState === 0 && !video.error && !error,
                MediaLoaded:!!video && video.readyState >= 1 && video.videoWidth > 0 && !video.error,
                HasContent:!error && !video?.error && !authWall && !!(map || image || (video?.readyState >= 1 && video.videoWidth > 0) || text.trim().length > 100)};
        }
        """;
}
