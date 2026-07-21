using System.Security.Cryptography;
using System.Text;
using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ImageMagick;

namespace EtheriT.Coker.Application.FileManagement
{
    public interface IThumbnailGeneratorService
    {
        void AssignThumbnailUrl(
            FileSystemInfo fileSystemInfo,
            FileSystemItem clientItem,
            string relativePath);

        Task<FileInfo?> GetOrCreateThumbnailAsync(
            string physicalFilePath,
            CancellationToken cancellationToken = default);
    }

    public class ThumbnailGeneratorService : IThumbnailGeneratorService, IDisposable
    {
        private const int ThumbnailWidth = 100;
        private const int ThumbnailHeight = 100;
        private const string ThumbnailsDirectoryPath = "thumb";
        private readonly SemaphoreSlim thumbnailGenerationGate = new(2, 2);
        private IHttpContextAccessor HttpContextAccessor { get; }
        private DirectoryInfo ThumbnailsDirectory { get; }

        private static readonly IReadOnlyCollection<string> AllowedFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ".png", ".gif", ".jpg", ".jpeg", ".ico", ".bmp", ".avif", ".webp", ".svg"
        };

        public ThumbnailGeneratorService(
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor
        )
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            var fullThumbnailsDirectoryPath = Path.Combine(environment.WebRootPath, ThumbnailsDirectoryPath);
            ThumbnailsDirectory = new DirectoryInfo(fullThumbnailsDirectoryPath);
        }

        public void AssignThumbnailUrl(
            FileSystemInfo fileSystemInfo,
            FileSystemItem clientItem,
            string relativePath)
        {
            if (clientItem.IsDirectory || !CanGenerateThumbnail(fileSystemInfo))
                return;

            if (!(fileSystemInfo is FileInfo fileInfo))
                return;

            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            // 目錄 API 只回傳縮圖 URL，不在列舉目錄時同步解碼所有圖片。
            // 縮圖由瀏覽器另行請求，避免 GetDirContents 因大量圖片逾時。
            var pathBase = httpContext.Request.PathBase.Value?.TrimEnd('/') ?? "";
            var encodedPath = Uri.EscapeDataString(relativePath.Replace('\\', '/'));
            clientItem.CustomFields["thumbnailUrl"] =
                $"{pathBase}/api/FileManagement/Thumbnail?path={encodedPath}&v={fileInfo.LastWriteTimeUtc.Ticks}";
        }

        public async Task<FileInfo?> GetOrCreateThumbnailAsync(
            string physicalFilePath,
            CancellationToken cancellationToken = default)
        {
            var sourceFile = new FileInfo(physicalFilePath);
            if (!sourceFile.Exists || !CanGenerateThumbnail(sourceFile))
                return null;

            var thumbnailFile = new FileInfo(GetThumbnailFilePath(sourceFile));
            if (HasFreshThumbnail(sourceFile, thumbnailFile))
                return thumbnailFile;

            await thumbnailGenerationGate.WaitAsync(cancellationToken);
            try
            {
                // 等待期間可能已由另一個請求完成。
                sourceFile.Refresh();
                thumbnailFile.Refresh();
                return GetThumbnail(sourceFile);
            }
            finally
            {
                thumbnailGenerationGate.Release();
            }
        }

        private FileInfo? GetThumbnail(FileInfo file)
        {
            var thumbnailFile = new FileInfo(GetThumbnailFilePath(file));

            if (!HasFreshThumbnail(file, thumbnailFile))
            {
                using (var thumbnailStream = file.OpenRead())
                {
                    if (!GenerateThumbnail(thumbnailStream, thumbnailFile))
                        return null;
                }
            }

            return thumbnailFile;
        }
        private static bool GenerateThumbnail(Stream file, FileInfo thumbnailFile)
        {
            try
            {
                if (thumbnailFile.Exists)
                    thumbnailFile.Delete();

                if (thumbnailFile.DirectoryName != null && !System.IO.Directory.Exists(thumbnailFile.DirectoryName))
                    System.IO.Directory.CreateDirectory(thumbnailFile.DirectoryName);

                GenerateThumbnailCore(file, thumbnailFile, ThumbnailWidth, ThumbnailHeight);
                // FileInfo 會快取產生前的 Exists 狀態；若不 Refresh，第一次請求會被誤判為 404。
                thumbnailFile.Refresh();
                return thumbnailFile.Exists && thumbnailFile.Length > 0;
            }
            catch
            {
                return false;
            }
        }
        private static void GenerateThumbnailCore(Stream file, FileInfo thumbnailFile, int width, int height)
        {
            // 使用 ImageMagick 處理圖片。可以處理多種格式，包括 AVIF 和 SVG。
            using (var originalImage = new MagickImage(file))
            using (var thumbnail = ChangeImageSize(originalImage, width, height))
            {
                // 縮圖一律輸出 PNG，避免 ICO、SVG 等來源格式在瀏覽器縮圖中相容性不一。
                thumbnail.Format = MagickFormat.Png;
                thumbnail.Write(thumbnailFile.FullName);
            }
        }

        private static MagickImage ChangeImageSize(MagickImage original, int width, int height)
        {
            // 計算新的尺寸
            uint newHeight = (uint)original.Height;
            uint newWidth = (uint)original.Width;
            if (original.Height > height || original.Width > width)
            {
                newHeight = (original.Height > original.Width) ? (uint)height : (uint)(height * original.Height / original.Width);
                newWidth = (original.Width > original.Height) ? (uint)width : (uint)(width * original.Width / original.Height);
            }

            // 製作一個調整大小後的原始圖像副本
            using var resizedOriginal = original.Clone();
            resizedOriginal.FilterType = FilterType.Lanczos;
            resizedOriginal.Resize(newWidth, newHeight);

            // 創建一個新的 MagickImage 作為縮略圖背景（白色背景）
            var thumbnail = new MagickImage(
                MagickColor.FromRgb(255, 255, 255),
                (uint)width,
                (uint)height);

            // 計算居中位置
            int top = (height - (int)newHeight) / 2;
            int left = (width - (int)newWidth) / 2;

            try
            {
                // 將縮放後的圖像合成到中心位置
                thumbnail.Composite(resizedOriginal, left, top);
                return thumbnail;
            }
            catch
            {
                // 回傳給呼叫端前發生錯誤時，也必須釋放 ImageMagick 原生記憶體。
                thumbnail.Dispose();
                throw;
            }
        }

        private static bool HasFreshThumbnail(FileSystemInfo file, FileSystemInfo thumbnail)
        {
            return thumbnail.Exists && file.LastWriteTime <= thumbnail.LastWriteTime;
        }

        private static bool CanGenerateThumbnail(FileSystemInfo fileSystemInfo)
        {
            return AllowedFileExtensions.Contains(fileSystemInfo.Extension);
        }

        private string GetThumbnailFilePath(FileSystemInfo file)
        {
            var thumbnailName = GetThumbnailFileName(file);
            return Path.Combine(ThumbnailsDirectory.FullName, thumbnailName.Substring(0, 3), thumbnailName);
        }

        private string GetThumbnailFileName(FileSystemInfo file)
        {
            return GetSHA1Hash(Encoding.UTF8.GetBytes(file.FullName)) + ".png";
        }

        private static string GetSHA1Hash(byte[] data)
        {
            var hashBytes = SHA1.HashData(data);
            return string.Concat(
                Array.ConvertAll(hashBytes, b => b.ToString("x2"))
            );
        }

        void IDisposable.Dispose()
        {
            thumbnailGenerationGate.Dispose();
        }
    }
}
