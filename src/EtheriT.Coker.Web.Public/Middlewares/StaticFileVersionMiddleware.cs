using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class StaticFileVersionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly (string Prefix, string Root)[] _uploadRoots;
        private readonly ILogger<StaticFileVersionMiddleware> _logger;
        private static readonly Regex UploadFileRegex = new(
            @"""(/upload/[^""'?#]+\.(?:jpg|jpeg|png|gif|pdf|svg|avif|webp))""|&amp;amp;quot;(/upload/[^""'?#]+\.(?:jpg|jpeg|png|gif|pdf|svg|avif|webp))&amp;amp;quot;",
            RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private static readonly Regex InlineScriptRegex = new(
            @"<script(?![^>]*\bnonce=)(?![^>]*\bsrc=)([^>]*)>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public StaticFileVersionMiddleware(RequestDelegate next, IConfiguration config,
            ILogger<StaticFileVersionMiddleware> logger)
        {
            _next = next;
            var roots = new List<(string Prefix, string Root)>();
            var names = config.GetSection("WebConfig:childSiteOrgName").Get<string[]>() ?? [];
            var paths = config.GetSection("WebConfig:childPath").Get<string[]>() ?? [];
            for (var i = 0; i < Math.Min(names.Length, paths.Length); i++)
            {
                if (!string.IsNullOrWhiteSpace(names[i]) && !string.IsNullOrWhiteSpace(paths[i]))
                    roots.Add(($"/upload/{names[i]}/", paths[i]));
            }
            var mainRoot = config.GetValue<string>("VirtualDirectory:upload");
            if (!string.IsNullOrWhiteSpace(mainRoot)) roots.Add(("/upload/", mainRoot));
            _uploadRoots = roots.OrderByDescending(root => root.Prefix.Length).ToArray();
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/File/DecryptFile"))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            try
            {
                context.Response.Body = newBody;

                await _next(context);

                if (context.Response.ContentType?.Contains("text/html") == true)
                {
                    newBody.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(newBody, Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
                    var responseBody = await reader.ReadToEndAsync();

                    if (context.Items.TryGetValue(ContentSecurityPolicyMiddleware.RewriteNonceKey, out var value)
                        && value is string nonce)
                    {
                        responseBody = InlineScriptRegex.Replace(responseBody,
                            $"<script nonce=\"{nonce}\">");
                    }

                    // 替換靜態文件 URL
                    var modifiedHtml = ReplaceStaticFileUrls(responseBody);

                    context.Response.Body = originalBody;
                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(modifiedHtml);
                    await context.Response.WriteAsync(modifiedHtml, context.RequestAborted);
                    return;
                }

                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody, context.RequestAborted);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private string ReplaceStaticFileUrls(string html)
        {
            // 已帶 ?v= 的產物不會命中，只補上舊／動態內容尚無版本的網址。
            // 僅在本次回應內去重，方法結束後即可回收；不跨頁／跨請求累積。
            var versions = new Dictionary<string, string?>(StringComparer.Ordinal);
            try
            {
                return UploadFileRegex.Replace(html, match =>
                {
                    var filePath = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    if (!versions.TryGetValue(filePath, out var hash))
                    {
                        hash = TryGetFileHash(filePath);
                        versions[filePath] = hash;
                    }
                    if (hash == null) return match.Value;
                    return match.Groups[1].Success
                        ? $"\"{filePath}?v={hash}\""
                        : $"&amp;amp;quot;{filePath}?v={hash}&amp;amp;quot;";
                });
            }
            catch (RegexMatchTimeoutException ex)
            {
                _logger.LogWarning(ex, "HTML 資源版本替換逾時，保留原回應內容");
                return html;
            }
        }

        private string? TryGetFileHash(string url)
        {
            try
            {
                var decoded = Uri.UnescapeDataString(url).Replace('\\', '/');
                var binding = _uploadRoots.FirstOrDefault(item => decoded.StartsWith(item.Prefix, StringComparison.OrdinalIgnoreCase));
                var root = binding.Root;
                if (string.IsNullOrWhiteSpace(root)) return null;
                var relative = decoded[binding.Prefix.Length..];
                if (relative.Split('/').Any(part => part.StartsWith('_') || part is "." or "..")) return null;
                var safeRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var filePath = Path.GetFullPath(Path.Combine(safeRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
                if (!filePath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase)) return null;

                var info = new FileInfo(filePath);
                if (!info.Exists) return null;
                var length = info.Length;
                var ticks = info.LastWriteTimeUtc.Ticks;
                // 相容舊內容的版本語意，已發布片段則使用內容 SHA-256。
                // 只計算時間與大小的短字串，不讀取圖片內容，也不保存常駐版本快取。
                return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{ticks}_{length}")))
                    .ToLowerInvariant();
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                return null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
                or NotSupportedException or System.Security.SecurityException)
            {
                _logger.LogWarning(ex, "無法取得前台資源版本，保留原網址：{Url}", url);
                return null;
            }
        }
    }
}
