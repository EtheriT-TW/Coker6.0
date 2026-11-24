using EtheriT.Coker.Application.SiteCrawl;
using System.Net;
using System.Net.Http.Headers;

namespace EtheriT.Coker.Application.ArticleImports;

public sealed record ArticleImportStoredImage(long FileId, string Path, string FileName, bool Created);

// 匯入用的圖片存取：測試時可換成假的實作，不用真的連 Google Drive。
public interface IArticleImportImageStore
{
    // 下載 Drive 檔案並存進站台上傳目錄；同一個 Drive 檔案已存過就沿用舊檔（Created = false）。
    Task<ArticleImportStoredImage> ImportAsync(string driveFileId, CancellationToken token);

    // 移到資源回收桶（可從檔案管理還原）；檔案已被其他對象綁定就不動。
    Task RecycleAsync(long fileId);
}

public sealed class ArticleImportImageStore(FileUploadAppService files, ArticleImportDriveDownloader downloader)
    : IArticleImportImageStore
{
    public async Task<ArticleImportStoredImage> ImportAsync(string driveFileId, CancellationToken token)
    {
        var source = ArticleImportDriveLinks.DownloadUrl(driveFileId).AbsoluteUri;
        var (file, created) = await files.ImportArticleImageAsync(
            source,
            async t =>
            {
                var image = await downloader.DownloadAsync(driveFileId, t);
                return (image.Content, image.FileName);
            },
            token);
        return new ArticleImportStoredImage(file.Id ?? 0, file.Path ?? "", file.Name ?? "", created);
    }

    public Task RecycleAsync(long fileId) => files.RecycleArticleImportFileAsync(fileId);
}

public sealed record ArticleImportDownloadedImage(byte[] Content, string FileName);

// 從 Google Drive 下載公開檔案：用網站爬蟲的 HttpClient（連線前檢查公網位址），
// 轉址只允許 Google 的檔案網域，重試方式比照 SiteCrawlHttp；只收 JPG、PNG、WebP。
public sealed class ArticleImportDriveDownloader(IHttpClientFactory factory)
{
    public const long MaxBytes = 20L * 1024 * 1024;
    private const int TimeoutSeconds = 30;
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10)];

    public async Task<ArticleImportDownloadedImage> DownloadAsync(string fileId, CancellationToken token)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await DownloadOnceAsync(fileId, token);
            }
            catch (HttpRequestException ex) when (attempt < RetryDelays.Length && IsTransient(ex.StatusCode))
            {
                await Task.Delay(RetryDelays[attempt], token);
            }
            catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
            {
                if (attempt >= RetryDelays.Length)
                    throw new InvalidOperationException($"Google Drive {TimeoutSeconds} 秒內沒有回應（已重試 {RetryDelays.Length} 次）。", ex);
                await Task.Delay(RetryDelays[attempt], token);
            }
        }
    }

    private static bool IsTransient(HttpStatusCode? status) => status is HttpStatusCode.ServiceUnavailable
        or HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout
        or HttpStatusCode.InternalServerError;

    private async Task<ArticleImportDownloadedImage> DownloadOnceAsync(string fileId, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));
        using var client = factory.CreateClient(SiteCrawlHttp.ClientName);
        var uri = ArticleImportDriveLinks.DownloadUrl(fileId);
        for (var redirects = 0; redirects <= 5; redirects++)
        {
            if (!ArticleImportDriveLinks.IsAllowedHost(uri))
                throw new InvalidOperationException("Google Drive 轉址到不允許的網域。");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.5));
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if ((int)response.StatusCode is 301 or 302 or 303 or 307 or 308 && response.Headers.Location is { } location)
            {
                uri = new Uri(uri, location);
                continue;
            }
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                throw new InvalidOperationException("Google Drive 找不到檔案，或檔案沒有開放「知道連結的任何人」檢視。");
            response.EnsureSuccessStatusCode();

            // 沒公開或檔案太大時 Google 會回 HTML 頁面，而不是檔案本身。
            if (response.Content.Headers.ContentType?.MediaType is "text/html" or "application/xhtml+xml")
                throw new InvalidOperationException("Google Drive 沒有回傳檔案（可能沒有公開，或檔案太大）。");

            var content = await ReadBoundedAsync(response.Content, timeout.Token);
            var extension = DetectImageExtension(content)
                ?? throw new InvalidOperationException("檔案不是 JPG、PNG 或 WebP 圖片。");
            return new ArticleImportDownloadedImage(content, FileName(response.Content.Headers.ContentDisposition, fileId, extension));
        }
        throw new InvalidOperationException("Google Drive 轉址次數超過上限。");
    }

    // 只看檔頭：JPG（FF D8 FF）、PNG（89 50 4E 47 0D 0A 1A 0A）、WebP（RIFF....WEBP）。
    public static string? DetectImageExtension(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return ".jpg";
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }

    // 檔名用 Drive 回傳的原始檔名（AI 靠檔名判斷是 Logo 還是橫幅），副檔名以檔頭為準。
    internal static string FileName(ContentDispositionHeaderValue? disposition, string fileId, string extension)
    {
        var raw = disposition?.FileNameStar ?? FixMojibake(disposition?.FileName?.Trim('"') ?? "");
        var name = Path.GetFileNameWithoutExtension(Path.GetFileName(raw.Replace('\\', '/')));
        var invalid = Path.GetInvalidFileNameChars();
        name = new string(name.Where(c => !invalid.Contains(c) && !char.IsControl(c)).ToArray()).Trim();
        if (name.Length > 80) name = name[..80];
        return (name.Length == 0 ? $"drive-{fileId}" : name) + extension;
    }

    // 標頭 filename="..." 若直接放 UTF-8 位元組，HttpClient 會當成 Latin-1 解碼而變亂碼；
    // 這裡還原回位元組再用 UTF-8 解碼，解不了（本來就不是亂碼）就維持原樣。
    internal static string FixMojibake(string raw)
    {
        if (raw.Length == 0 || raw.Any(c => c > 0xFF)) return raw;
        try
        {
            return new System.Text.UTF8Encoding(false, true).GetString(System.Text.Encoding.Latin1.GetBytes(raw));
        }
        catch (System.Text.DecoderFallbackException)
        {
            return raw;
        }
    }

    private static async Task<byte[]> ReadBoundedAsync(HttpContent content, CancellationToken token)
    {
        if (content.Headers.ContentLength > MaxBytes)
            throw new InvalidOperationException($"圖片超過 {MaxBytes / 1024 / 1024} MB。");
        await using var input = await content.ReadAsStreamAsync(token);
        using var output = new MemoryStream();
        var buffer = new byte[16384];
        int read;
        while ((read = await input.ReadAsync(buffer, token)) != 0)
        {
            if (output.Length + read > MaxBytes)
                throw new InvalidOperationException($"圖片超過 {MaxBytes / 1024 / 1024} MB。");
            await output.WriteAsync(buffer.AsMemory(0, read), token);
        }
        return output.ToArray();
    }
}