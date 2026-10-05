using EtheriT.Coker.Application.Shared.Dto.Files;
using Microsoft.Extensions.FileProviders;

namespace EtheriT.Coker.Web.Public.Services
{
    /// <summary>
    /// 缺圖回報的後端檢查：來源頁必須是送出請求的那一頁、圖片必須與來源頁同站，
    /// 且圖片在磁碟上確實不存在，才需要記錄。
    /// </summary>
    public class NotFoundImageVerifier
    {
        private const int MaxColumnLength = 255;
        private readonly List<(string Prefix, IFileProvider Provider)> providers = new();

        public NotFoundImageVerifier(IConfiguration configuration, IWebHostEnvironment env)
        {
            // 與 Program.cs 的 UseStaticFiles 對應：子站 upload、主站 upload、wwwroot
            var childOrgNames = configuration.GetSection("WebConfig:childSiteOrgName").Get<List<string>>() ?? new();
            var childPaths = configuration.GetSection("WebConfig:childPath").Get<List<string>>() ?? new();
            for (int i = 0; i < childOrgNames.Count && i < childPaths.Count; i++)
                providers.Add(($"/upload/{childOrgNames[i]}/", new PhysicalFileProvider(childPaths[i])));

            var uploadRoot = configuration.GetValue<string>("VirtualDirectory:upload");
            if (!string.IsNullOrWhiteSpace(uploadRoot))
                providers.Add(("/upload/", new PhysicalFileProvider(uploadRoot)));

            providers.Add(("/", env.WebRootFileProvider));
        }

        public NotFoundImageCheckResult Verify(InsertNotFoundFileDto dto, string? referer)
        {
            if (string.IsNullOrWhiteSpace(dto?.Url) || string.IsNullOrWhiteSpace(dto.From))
                return NotFoundImageCheckResult.Skip("缺少圖片或來源頁");

            if (!TryGetHttpUri(dto.From, out var pageUri) || !TryGetHttpUri(referer, out var refererUri))
                return NotFoundImageCheckResult.Skip("來源頁或 Referer 不是有效網址");

            // 來源頁必須就是瀏覽器送出這個請求時所在的那一頁
            if (!IsSameHost(pageUri, refererUri) || pageUri.PathAndQuery != refererUri.PathAndQuery)
                return NotFoundImageCheckResult.Skip("來源頁與 Referer 不是同一頁");

            // 圖片必須與來源頁同站；相對路徑以來源頁為基準解析
            if (!Uri.TryCreate(pageUri, dto.Url.Trim(), out var imageUri) || !IsHttp(imageUri) || !IsSameHost(pageUri, imageUri))
                return NotFoundImageCheckResult.Skip("圖片不屬於來源頁的網站");

            var imagePath = Uri.UnescapeDataString(imageUri.AbsolutePath);
            var pagePath = pageUri.PathAndQuery;
            if (imagePath.Contains("..") || imagePath.Length > MaxColumnLength || pagePath.Length > MaxColumnLength)
                return NotFoundImageCheckResult.Skip("圖片或來源頁網址不合法");

            if (FileExists(imagePath))
                return NotFoundImageCheckResult.Skip("圖片實際存在");

            return NotFoundImageCheckResult.Record(imagePath, pagePath);
        }

        private bool FileExists(string path)
        {
            // 任一個對應的目錄有這個檔案，瀏覽器就拿得到，視為存在
            foreach (var (prefix, provider) in providers)
            {
                if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var file = provider.GetFileInfo(path.Substring(prefix.Length - 1));
                if (file.Exists && !file.IsDirectory)
                    return true;
            }
            return false;
        }

        private static bool TryGetHttpUri(string? value, out Uri uri)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out uri!) && IsHttp(uri);
        }

        private static bool IsHttp(Uri uri)
        {
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        private static bool IsSameHost(Uri a, Uri b)
        {
            return string.Equals(a.Host, b.Host, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed record NotFoundImageCheckResult(bool ShouldRecord, string ImagePath, string PagePath, string Reason)
    {
        public static NotFoundImageCheckResult Skip(string reason) => new(false, "", "", reason);
        public static NotFoundImageCheckResult Record(string imagePath, string pagePath) => new(true, imagePath, pagePath, "");
    }
}