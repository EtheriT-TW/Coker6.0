using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteCrawlHttp(IHttpClientFactory factory)
{
    public const string ClientName = "SiteCrawl";

    public static SocketsHttpHandler CreateHandler() => new()
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        UseProxy = false,
        AutomaticDecompression = DecompressionMethods.All,
        ConnectCallback = ConnectPublicAsync
    };

    public static bool IsAllowed(Uri uri, string host) =>
        IsWebUrl(uri) && SiteCrawlStateStore.NormalizeHost(uri.IdnHost) == SiteCrawlStateStore.NormalizeHost(host);

    private static bool IsWebUrl(Uri uri) =>
        uri.IsAbsoluteUri && uri.Scheme is "https" or "http" &&
        uri.Port is 80 or 443 && uri.UserInfo.Length == 0;

    public static async Task<IPAddress[]> ResolvePublicAsync(string host, CancellationToken token)
    {
        var addresses = await Dns.GetHostAddressesAsync(host, token);
        if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
            throw new InvalidOperationException("來源網域解析至非公網位址。");
        return addresses;
    }

    // 腳本、樣式檔等附屬資源；頁面常從外部 CDN 載入 jQuery，由這裡代抓才能套用公網位址檢查。
    public async Task<byte[]> GetResourceAsync(Uri uri, long maxBytes, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        using var client = factory.CreateClient(ClientName);
        for (var redirects = 0; redirects <= 5; redirects++)
        {
            if (!IsWebUrl(uri)) throw new InvalidOperationException("資源網址不允許。");
            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if ((int)response.StatusCode is 301 or 302 or 303 or 307 or 308 && response.Headers.Location is { } location)
            {
                uri = new Uri(uri, location);
                continue;
            }

            response.EnsureSuccessStatusCode();
            await using var body = await ReadBoundedAsync(response.Content, maxBytes, timeout.Token);
            return body.ToArray();
        }
        throw new InvalidOperationException("轉址次數超過上限。");
    }

    // 舊站連續抓取時偶爾回 503（2026-11-16 ayawan 實測一次匯入 4 頁），等一下重抓通常就好。
    // 逾時也一樣（2026-11-17 superiorlink 商品 id=84 匯入時 15 秒沒回應，事後單獨抓不到 1 秒）。
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10)];
    private const int TimeoutSeconds = 15;

    internal async Task<HttpStatusCode?> ProbeStatusAsync(Uri uri, CancellationToken token)
    {
        if (!IsAllowed(uri, uri.IdnHost)) return null;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var client = factory.CreateClient(ClientName);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            return response.StatusCode;
        }
        catch (Exception ex) when (!token.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            return null;
        }
    }

    internal static bool IsTimeout(Exception exception) =>
        exception is OperationCanceledException or TimeoutException
            or HttpRequestException { StatusCode: HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout }
        || exception.InnerException is OperationCanceledException or TimeoutException;

    public Task<(Uri Url, MemoryStream Content, string? Charset)> GetAsync(
        Uri uri, string host, bool image, long maxBytes, CancellationToken token) =>
        GetAsync(uri, host, image ? SiteCrawlContent.Image : SiteCrawlContent.Html, maxBytes, token);

    internal async Task<(Uri Url, MemoryStream Content, string? Charset)> GetAsync(
        Uri uri, string host, SiteCrawlContent kind, long maxBytes, CancellationToken token)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await GetOnceAsync(uri, host, kind, maxBytes, token);
            }
            catch (HttpRequestException ex) when (attempt < RetryDelays.Length && IsTransient(ex.StatusCode))
            {
                await Task.Delay(RetryDelays[attempt], token);
            }
            // 只處理單次抓取的逾時；整個匯入被取消（token）時照常往外丟。
            catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
            {
                if (attempt >= RetryDelays.Length)
                    throw new HttpRequestException($"舊站 {TimeoutSeconds} 秒內沒有回應（已重試 {RetryDelays.Length} 次）。", ex);
                await Task.Delay(RetryDelays[attempt], token);
            }
        }
    }

    private static bool IsTransient(HttpStatusCode? status) => status is HttpStatusCode.ServiceUnavailable
        or HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout;

    private async Task<(Uri Url, MemoryStream Content, string? Charset)> GetOnceAsync(
        Uri uri, string host, SiteCrawlContent kind, long maxBytes, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));
        using var client = factory.CreateClient(ClientName);
        for (var redirects = 0; redirects <= 5; redirects++)
        {
            if (!IsAllowed(uri, host)) throw new InvalidOperationException("網址或轉址超出來源網域範圍。");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(kind switch
            {
                SiteCrawlContent.Image => "image/*",
                SiteCrawlContent.File => "*/*",
                _ => "text/html"
            }));
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if ((int)response.StatusCode is 301 or 302 or 303 or 307 or 308 && response.Headers.Location is { } location)
            {
                uri = new Uri(uri, location);
                continue;
            }

            response.EnsureSuccessStatusCode();
            var type = response.Content.Headers.ContentType;
            var unexpected = kind switch
            {
                SiteCrawlContent.Image => type?.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true,
                // 附件的類型由副檔名決定（舊站常回 application/octet-stream）；回 HTML 代表是錯誤頁或登入頁。
                SiteCrawlContent.File => type?.MediaType is "text/html" or "application/xhtml+xml",
                _ => type?.MediaType is not ("text/html" or "application/xhtml+xml")
            };
            if (unexpected) throw new InvalidOperationException("回應類型不符合預期。");

            return (uri, await ReadBoundedAsync(response.Content, maxBytes, timeout.Token), type?.CharSet);
        }
        throw new InvalidOperationException("轉址次數超過上限。");
    }

    private static async Task<MemoryStream> ReadBoundedAsync(HttpContent content, long limit, CancellationToken token)
    {
        if (content.Headers.ContentLength > limit) throw new InvalidOperationException("回應超過大小上限。");
        await using var input = await content.ReadAsStreamAsync(token);
        var buffer = new byte[16384];
        var output = new MemoryStream();
        try
        {
            int read;
            while ((read = await input.ReadAsync(buffer, token)) != 0)
            {
                if (output.Length + read > limit) throw new InvalidOperationException("回應超過大小上限。");
                await output.WriteAsync(buffer.AsMemory(0, read), token);
            }
            output.Position = 0;
            return output;
        }
        catch
        {
            output.Dispose();
            throw;
        }
    }

    private static async ValueTask<Stream> ConnectPublicAsync(SocketsHttpConnectionContext context, CancellationToken token)
    {
        var endpoint = context.DnsEndPoint;
        foreach (var address in await ResolvePublicAsync(endpoint.Host, token))
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, endpoint.Port), token);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (SocketException)
            {
                socket.Dispose();
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
        throw new HttpRequestException("無法連線至來源網站。");
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (FileUploadAppService.IsPrivateAddress(address)) return false;
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            return (bytes[0] & 0xe0) == 0x20
                && !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] < 2)
                && !(bytes[0] == 0x20 && bytes[1] == 0x02);
        return !(bytes[0] == 100 && bytes[1] is >= 64 and <= 127)
            && !(bytes[0] == 198 && bytes[1] is 18 or 19);
    }
}

// 抓取的內容種類：決定 Accept 標頭與回應類型檢查。
internal enum SiteCrawlContent
{
    Html,
    Image,
    File
}
