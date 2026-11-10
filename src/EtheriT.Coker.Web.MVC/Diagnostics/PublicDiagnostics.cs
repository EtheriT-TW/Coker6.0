using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public enum PublicCheckStatus { NotRun, Passed, Failed, NeedsReview }
public sealed record PublicTarget(string Title, string Url);
public sealed record PublicFinding(string Kind, string Resource, PublicCheckStatus Status, string Message)
{
    public string Id { get; init; } = "";
    public string? Location { get; init; }
    public string? RelatedUrl { get; init; }
    public string Label => Status switch { PublicCheckStatus.Passed => "通過", PublicCheckStatus.Failed => "異常",
        PublicCheckStatus.NeedsReview => "無法確認", _ => "未檢測" };
}
public sealed record PublicPageResult(string Title, string Url, DateTime AtUtc, List<PublicFinding> Findings)
{
    public bool StopRun { get; init; }
}
public sealed record PublicPlan(long WebsiteId, string Stamp, List<PublicTarget> Targets, bool Limited, string? Error)
{
    public HashSet<string> AllowedLinks { get; init; } = new(StringComparer.Ordinal);
}
public sealed record PublicReport(string Stamp, DateTime AtUtc, Dictionary<string, PublicPageResult> Pages);
public sealed record PublicDiagnosticsModel(PublicPlan Plan, PublicReport? Report, bool ShowAll, string? Warning);
public sealed class PublicProbePolicyException(string message) : HttpRequestException(message);
public sealed record PublicWireResponse(int Code, Dictionary<string, string> Headers, byte[] Body);

public sealed class PublicResultStore(string contentRoot)
{
    private string FileName(long site, long user)
    {
        if (site <= 0 || user <= 0) throw new InvalidOperationException("Invalid diagnostics identity.");
        return Path.Combine(contentRoot, "App_Data", "PublicDiagnostics", $"{site}-{user}.json");
    }
    public async Task<PublicReport?> LoadAsync(long site, long user, string stamp)
    {
        var path = FileName(site, user);
        if (!File.Exists(path)) return null;
        var report = JsonSerializer.Deserialize<PublicReport>(await File.ReadAllTextAsync(path));
        return report?.Stamp == stamp && report.Pages != null ? report : null;
    }
    public async Task SaveAsync(long site, long user, string stamp, PublicPageResult page)
    {
        PublicReport? previous;
        try { previous = await LoadAsync(site, user, stamp); }
        catch (JsonException) { previous = null; }
        var pages = previous == null ? new Dictionary<string, PublicPageResult>() : new Dictionary<string, PublicPageResult>(previous.Pages);
        pages[page.Url] = page;
        var path = FileName(site, user);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(new PublicReport(stamp, DateTime.UtcNow, pages)));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}

// Requests carry no cookies, authentication or customer headers. DNS is checked
// at connection time (including every redirect), preventing private-network probes.
public sealed class PublicProbeClient : IDisposable
{
    private readonly HttpClient client;
    internal PublicProbeClient(HttpClient fixtureClient) { client = fixtureClient; }
    public PublicProbeClient()
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false, UseCookies = false, UseProxy = false,
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(5),
            ConnectCallback = async (context, token) =>
            {
                var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, token);
                if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
                    throw new PublicProbePolicyException("Diagnostics target is not publicly routable.");
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, token);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch { socket.Dispose(); throw; }
            }
        };
        client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Coker-PublicDiagnostics/1.0");
    }
    public static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (IPAddress.IsLoopback(address)) return false;
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            return (bytes[0] & 0xe0) == 0x20 && !address.Equals(IPAddress.IPv6None)
                && !(bytes[0] == 0x20 && bytes[1] == 1 && (bytes[2] < 2 || bytes[2] == 0x0d && bytes[3] == 0xb8))
                && !(bytes[0] == 0x20 && bytes[1] == 2);
        return bytes[0] != 0 && bytes[0] != 10 && bytes[0] != 127 && bytes[0] < 224
            && !(bytes[0] == 169 && bytes[1] == 254) && !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
            && !(bytes[0] == 192 && bytes[1] == 168) && !(bytes[0] == 100 && bytes[1] is >= 64 and <= 127)
            && !(bytes[0] == 192 && bytes[1] == 0) && !(bytes[0] == 198 && bytes[1] is 18 or 19)
            && !(bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100) && !(bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113);
    }
    public static bool IsHttp(Uri uri) => (uri.Scheme == "https" || uri.Scheme == "http")
        && string.IsNullOrEmpty(uri.UserInfo) && (uri.Port == 80 || uri.Port == 443);
    internal static bool IsPolicyFailure(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
            if (current is PublicProbePolicyException) return true;
        return false;
    }
    internal static bool IsReadOnlyVideoRequest(Uri uri, string method) => method == "POST"
        && (uri.IdnHost == "www.youtube.com" || uri.IdnHost == "www.youtube-nocookie.com")
        && uri.AbsolutePath is "/youtubei/v1/player" or "/youtubei/v1/next";

    // Used by browser routing. Redirects are returned to Chromium, so relative
    // resources, origins and CSP keep the semantics of the actual front page.
    internal async Task<PublicWireResponse> FetchForBrowserAsync(Uri uri, string method,
        IReadOnlyDictionary<string, string> browserHeaders, CancellationToken token, byte[]? body = null)
    {
        if (!IsHttp(uri) || method is not ("GET" or "HEAD") && !IsReadOnlyVideoRequest(uri, method)) throw new PublicProbePolicyException("Unsupported browser request.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        using var request = new HttpRequestMessage(new HttpMethod(method), uri);
        if (method == "POST")
        {
            if (body == null || body.Length > 64 * 1024) throw new PublicProbePolicyException("Video metadata request exceeded limit.");
            request.Content = new ByteArrayContent(body);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        }
        foreach (var key in new[] { "accept", "accept-language", "user-agent", "origin", "referer", "range",
            "sec-fetch-dest", "sec-fetch-mode", "sec-fetch-site", "sec-ch-ua", "sec-ch-ua-mobile", "sec-ch-ua-platform",
            "x-youtube-client-name", "x-youtube-client-version", "x-goog-visitor-id", "x-origin" })
            if (browserHeaders.TryGetValue(key, out var value)) request.Headers.TryAddWithoutValidation(key, value);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        var headers = response.Headers.Concat(response.Content.Headers)
            .Where(header => !new[] { "content-encoding", "content-length", "transfer-encoding", "set-cookie", "connection", "alt-svc" }.Contains(header.Key.ToLowerInvariant()))
            .ToDictionary(header => header.Key, header => string.Join(", ", header.Value), StringComparer.OrdinalIgnoreCase);
        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var data = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var length = await stream.ReadAsync(buffer, timeout.Token);
            if (length == 0) break;
            if (data.Length + length > 8 * 1024 * 1024) throw new PublicProbePolicyException("Browser response exceeded resource limit.");
            data.Write(buffer, 0, length);
        }
        return new((int)response.StatusCode, headers, data.ToArray());
    }

    public async Task<(int Code, string Media, byte[] Body, Uri Final)> GetAsync(Uri uri, int limit, CancellationToken token, bool head = false)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(12));
        for (var redirects = 0; redirects <= 3; redirects++)
        {
            if (!IsHttp(uri)) throw new PublicProbePolicyException("Unsupported diagnostics URL.");
            using var request = new HttpRequestMessage(head ? HttpMethod.Head : HttpMethod.Get, uri);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            var code = (int)response.StatusCode;
            if (code is 301 or 302 or 303 or 307 or 308)
            {
                if (response.Headers.Location == null) throw new HttpRequestException("Redirect without destination.");
                uri = new Uri(uri, response.Headers.Location);
                continue;
            }
            var media = response.Content.Headers.ContentType?.MediaType ?? "";
            if (head) return (code, media, Array.Empty<byte>(), uri);
            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var memory = new MemoryStream();
            var buffer = new byte[8192];
            while (memory.Length <= limit)
            {
                var size = await stream.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, limit + 1 - (int)memory.Length)), timeout.Token);
                if (size == 0) break;
                memory.Write(buffer, 0, size);
                if (memory.Length > limit) throw new PublicProbePolicyException("Response exceeded diagnostics limit.");
            }
            return (code, media, memory.ToArray(), uri);
        }
        throw new HttpRequestException("Too many redirects.");
    }
    public void Dispose() => client.Dispose();
}

public static class PublicPageChecks
{
    public static string Stamp(IEnumerable<PublicTarget> targets, IEnumerable<string> features) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("public-browser-v3|" + JsonSerializer.Serialize(targets)
            + "|" + string.Join("|", features.OrderBy(value => value, StringComparer.Ordinal)))));
    public static string Display(Uri uri) => uri.GetLeftPart(UriPartial.Path); // Never show query tokens / map API keys.
    public static bool SameOrigin(Uri left, Uri right) => left.Scheme == right.Scheme && left.IdnHost == right.IdnHost && left.Port == right.Port;
    public static bool IsOperationLink(Uri uri) => Regex.IsMatch(uri.AbsolutePath,
        @"(?:^|/)(?:api|front|logout|unsubscribe|delete|remove|checkout|reset|confirm|callback|signout)(?:/|$)", RegexOptions.IgnoreCase)
        || Regex.IsMatch(Uri.UnescapeDataString(uri.Query), @"(?:\?|&)(?:token|action|command|delete|unsubscribe|confirm|addToCart|access_token)=", RegexOptions.IgnoreCase);
    internal static PublicPageResult Identify(PublicTarget target, IEnumerable<PublicFinding> findings)
    {
        var occurrences = new Dictionary<string, int>();
        return new(target.Title, target.Url, DateTime.UtcNow, findings.Select(finding =>
        {
            var key = target.Url + "|" + finding.Kind + "|" + finding.Resource;
            var ordinal = occurrences.GetValueOrDefault(key);
            occurrences[key] = ordinal + 1;
            return finding with { Id = "public-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key + "|" + ordinal))) };
        }).ToList());
    }
    public static bool IsEmbedHost(string host, string domain) => host.Equals(domain, StringComparison.OrdinalIgnoreCase)
        || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);
    public static string EmbedKind(Uri uri) => IsEmbedHost(uri.IdnHost, "youtube.com") || IsEmbedHost(uri.IdnHost, "youtube-nocookie.com") || uri.IdnHost == "youtu.be"
        ? "YouTube 嵌入" : IsEmbedHost(uri.IdnHost, "facebook.com") ? "Facebook 嵌入"
        : Regex.IsMatch(uri.AbsolutePath, @"\.(mp4|webm|ogv|ogg|m4v)$", RegexOptions.IgnoreCase) ? "影片檔案"
        : IsEmbedHost(uri.IdnHost, "google.com") && uri.AbsolutePath.Contains("/maps", StringComparison.OrdinalIgnoreCase) ? "Google Maps 嵌入" : "外部嵌入";

    public static string? FacebookVideoSource(Uri uri)
    {
        if (EmbedKind(uri) != "Facebook 嵌入") return null;
        var href = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).GetValueOrDefault("href").ToString();
        if (!Uri.TryCreate(href, UriKind.Absolute, out var original) || !PublicProbeClient.IsHttp(original)
            || !IsEmbedHost(original.IdnHost, "facebook.com")) return null;
        var result = Display(original);
        var videoId = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(original.Query).GetValueOrDefault("v").ToString();
        return Regex.IsMatch(videoId, @"^[0-9]+$") ? result + "?v=" + videoId : result;
    }

    public static string? YouTubeId(Uri uri)
    {
        if (EmbedKind(uri) != "YouTube 嵌入") return null;
        var value = uri.IdnHost == "youtu.be" ? uri.AbsolutePath.Trim('/')
            : Regex.Match(uri.AbsolutePath, @"^/(?:embed|shorts|v)/([^/]+)").Groups[1].Value;
        if (value.Length == 0) value = Regex.Match(uri.Query, @"(?:\?|&)v=([^&]+)").Groups[1].Value;
        return Regex.IsMatch(value, @"^[A-Za-z0-9_-]{11}$") ? value : null;
    }

}
