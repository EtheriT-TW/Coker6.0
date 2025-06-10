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

            var helper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
            var thumbnail = GetThumbnail(fileInfo);
            var relativeThumbnailPath = Path.Combine(ThumbnailsDirectory.Name, thumbnail.Directory?.Name, thumbnail.Name);
            clientItem.CustomFields["thumbnailUrl"] = helper.Content(relativeThumbnailPath);
        }

        private FileInfo GetThumbnail(FileInfo file)
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

                if (!System.IO.Directory.Exists(thumbnailFile.DirectoryName))
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
            using (var originalImage = Image.FromStream(file))
            using (var thumbnail = ChangeImageSize(originalImage, width, height))
            {
                try
                {
                    thumbnail.Save(thumbnailFile.FullName);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static Bitmap ChangeImageSize(Image original, int width, int height)
        {
            var thumbnail = new Bitmap(width, height);

            var newHeight = original.Height;
            var newWidth = original.Width;
            if (original.Height > height || original.Width > width)
            {
                newHeight = (original.Height > original.Width) ? height : (height * original.Height / original.Width);
                newWidth = (original.Width > original.Height) ? width : (width * original.Width / original.Height);
            }

            var g = Graphics.FromImage(thumbnail);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            var top = (height - newHeight) / 2;
            var left = (width - newWidth) / 2;
            g.DrawImage(original, left, top, newWidth, newHeight);

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