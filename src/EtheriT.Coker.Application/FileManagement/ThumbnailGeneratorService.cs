using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Cryptography;
using System.Text;
using DevExtreme.AspNet.Mvc.FileManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ImageMagick;

namespace EtheriT.Coker.Application.FileManagement
{
    public interface IThumbnailGeneratorService
    {
        void AssignThumbnailUrl(FileSystemInfo fileSystemInfo, FileSystemItem clientItem);
    }

    public class ThumbnailGeneratorService : IThumbnailGeneratorService, IDisposable
    {
        private const int ThumbnailWidth = 100;
        private const int ThumbnailHeight = 100;
        private const string ThumbnailsDirectoryPath = "thumb";

        private IUrlHelperFactory UrlHelperFactory { get; }
        private IActionContextAccessor ActionContextAccessor { get; }
        private DirectoryInfo ThumbnailsDirectory { get; }

        private SHA1 CryptoProvider { get; }


        private static readonly IReadOnlyCollection<string> AllowedFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
            ".png", ".gif", ".jpg", ".jpeg", ".ico", ".bmp", ".avif", ".webp", ".svg"
        };

        public ThumbnailGeneratorService(IWebHostEnvironment environment, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            UrlHelperFactory = urlHelperFactory ?? throw new ArgumentNullException(nameof(urlHelperFactory));
            ActionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));

            var fullThumbnailsDirectoryPath = Path.Combine(environment.WebRootPath, ThumbnailsDirectoryPath);
            ThumbnailsDirectory = new DirectoryInfo(fullThumbnailsDirectoryPath);

            CryptoProvider = SHA1.Create();
        }

        public void AssignThumbnailUrl(FileSystemInfo fileSystemInfo, FileSystemItem clientItem)
        {
            if (clientItem.IsDirectory || !CanGenerateThumbnail(fileSystemInfo))
                return;

            if (!(fileSystemInfo is FileInfo fileInfo))
                return;

            if (ActionContextAccessor?.ActionContext != null)
            {
                var helper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
                var thumbnail = GetThumbnail(fileInfo);
                if (thumbnail != null && thumbnail.Directory != null)
                {
                    var relativeThumbnailPath = Path.Combine(ThumbnailsDirectory.Name, thumbnail.Directory.Name, thumbnail.Name);
                    clientItem.CustomFields["thumbnailUrl"] = helper.Content(relativeThumbnailPath);
                }
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
                return true;
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
                try
                {
                    // 將 MagickImage 寫入檔案
                    thumbnail.Write(thumbnailFile.FullName);
                }
                catch
                {
                    // ignored
                }
            }
        }        private static MagickImage ChangeImageSize(MagickImage original, int width, int height)
        {
            // 創建一個新的 MagickImage 作為縮略圖背景（白色背景）
            var thumbnail = new MagickImage(MagickColor.FromRgb(255, 255, 255), (uint)width, (uint)height);

            // 計算新的尺寸
            uint newHeight = (uint)original.Height;
            uint newWidth = (uint)original.Width;
            if (original.Height > height || original.Width > width)
            {
                newHeight = (original.Height > original.Width) ? (uint)height : (uint)(height * original.Height / original.Width);
                newWidth = (original.Width > original.Height) ? (uint)width : (uint)(width * original.Width / original.Height);
            }

            // 製作一個調整大小後的原始圖像副本
            var resizedOriginal = original.Clone();
            resizedOriginal.Resize(newWidth, newHeight);
            resizedOriginal.FilterType = FilterType.Lanczos; // 高品質縮放濾鏡，類似於 HighQualityBicubic

            // 計算居中位置
            int top = (height - (int)newHeight) / 2;
            int left = (width - (int)newWidth) / 2;

            // 將縮放後的圖像合成到中心位置
            thumbnail.Composite(resizedOriginal, left, top);

            return thumbnail;
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
            return GetSHA1Hash(Encoding.UTF8.GetBytes(file.FullName)) + file.Extension;
        }

        private string GetSHA1Hash(byte[] data)
        {
            var hashBytes = CryptoProvider.ComputeHash(data);
            return string.Concat(
                Array.ConvertAll(hashBytes, b => b.ToString("x2"))
            );
        }

        void IDisposable.Dispose()
        {
            CryptoProvider.Dispose();
        }
    }
}