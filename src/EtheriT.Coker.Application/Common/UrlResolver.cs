using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace EtheriT.Coker.Application.Common
{
    public static class UrlResolver
    {
        public static string ResolvePublicBaseUrl(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            string? fallbackUrl = null)
        {
            var request = httpContextAccessor.HttpContext?.Request;
            var host = request?.Host.Value;

            // 正式環境：優先使用網站設定的主網址
            if (env.IsProduction())
            {
                if (!string.IsNullOrWhiteSpace(fallbackUrl))
                    return fallbackUrl.TrimEnd('/');

                if (!string.IsNullOrWhiteSpace(host))
                    return $"{request!.Scheme}://{host}".TrimEnd('/');

                throw new Exception("正式環境無法解析公開網址。");
            }

            // 非正式環境：優先使用目前 Request Host，包含 localhost
            if (!string.IsNullOrWhiteSpace(host))
                return $"{request!.Scheme}://{host}".TrimEnd('/');

            if (!string.IsNullOrWhiteSpace(fallbackUrl))
                return fallbackUrl.TrimEnd('/');

            throw new Exception("無法解析公開網址。");
        }

        public static string ResolveUrl(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env,
            string path,
            string? fallbackUrl = null)
        {
            var baseUrl = ResolvePublicBaseUrl(
                httpContextAccessor,
                env,
                fallbackUrl);

            path = path.StartsWith("/") ? path : "/" + path;

            return $"{baseUrl}{path}";
        }
    }
}
