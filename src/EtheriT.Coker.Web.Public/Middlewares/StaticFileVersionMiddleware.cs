using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class StaticFileVersionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private static readonly ConcurrentDictionary<string, string> _fileHashCache = new();

        public StaticFileVersionMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBody = context.Response.Body;
            using (var newBody = new MemoryStream())
            {
                context.Response.Body = newBody;

                await _next(context);

                if (context.Response.ContentType?.Contains("text/html") == true)
                {
                    newBody.Seek(0, SeekOrigin.Begin);
                    var responseBody = await new StreamReader(newBody).ReadToEndAsync();
                    newBody.Seek(0, SeekOrigin.Begin);

                    // 替換靜態文件 URL
                    var modifiedHtml = await ReplaceStaticFileUrlsAsync(responseBody);

                    await context.Response.WriteAsync(modifiedHtml);
                }

                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);
            }
        }

        private async Task<string> ReplaceStaticFileUrlsAsync(string html)
        {
            // 匹配原生雙引號和被編碼的雙引號
            var regex = new Regex(@"""(/upload/[^""'?#]+\.(?:jpg|jpeg|png|gif|pdf|svg|avif|webp))""|&amp;amp;quot;(/upload/[^""'?#]+\.(?:jpg|jpeg|png|gif|pdf|svg|avif|webp))&amp;amp;quot;", RegexOptions.Compiled);
            var matches = regex.Matches(html);

            var replacements = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                // 判斷是哪一種情況
                var filePath = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
                if (!replacements.ContainsKey(filePath))
                {
                    var fullPath = Path.Combine(_config.GetValue<string>("VirtualDirectory:upload"), filePath.Substring("/upload".Length).TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        var hash = await GetFileHashAsync(fullPath);

                        // 根據匹配的情況生成替換字串
                        if (!string.IsNullOrEmpty(match.Groups[1].Value))
                        {
                            // 原生雙引號
                            replacements[filePath] = $@"""{filePath}?v={hash}""";
                        }
                        else
                        {
                            // 被編碼的雙引號
                            replacements[filePath] = $@"&amp;amp;quot;{filePath}?v={hash}&amp;amp;quot;";
                        }
                    }
                }
            }

            // 替換 URL
            return regex.Replace(html, match =>
            {
                var filePath = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
                return replacements.TryGetValue(filePath, out var replacement) ? replacement : match.Value;
            });
        }

        private async Task<string> GetFileHashAsync(string filePath)
        {
            if (_fileHashCache.TryGetValue(filePath, out var cachedHash))
            {
                return cachedHash;
            }

            var fileInfo = new FileInfo(filePath);

            // 使用檔案的最後修改時間和檔案大小來生成哈希值
            var hashInput = $"{fileInfo.LastWriteTimeUtc.Ticks}_{fileInfo.Length}";

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
