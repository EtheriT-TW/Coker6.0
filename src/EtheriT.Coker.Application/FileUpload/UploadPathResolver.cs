using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared;
using Microsoft.Extensions.Options;
using IODirectory = System.IO.Directory;

namespace EtheriT.Coker.Application
{
    public class UploadPathResolver : IUploadPathResolver
    {
        private readonly VirtualDirectory virtualDirectory;

        public UploadPathResolver(IOptions<VirtualDirectory> virtualDirectory)
        {
            this.virtualDirectory = virtualDirectory.Value;
        }

        public string GetRootPath(string orgName)
        {
            if (string.IsNullOrWhiteSpace(orgName))
                throw new Exception("orgName 不可為空");

            // 前台的 VirtualDirectory:upload 已直接指向單一網站目錄，
            // 例如 ...\upload\research-tju，不應再次附加 OrgName。
            if (!string.IsNullOrWhiteSpace(virtualDirectory.Upload))
            {
                var siteUploadPath = Path.GetFullPath(virtualDirectory.Upload);

                if (IODirectory.Exists(siteUploadPath))
                    return siteUploadPath;
            }

            var uploadRoots = virtualDirectory.UploadRoots ?? new Dictionary<string, string>();

            foreach (var root in uploadRoots.Values)
            {
                if (string.IsNullOrWhiteSpace(root))
                    continue;

                var siteRoot = Path.Combine(root, orgName);

                if (IODirectory.Exists(siteRoot))
                    return siteRoot;
            }

            throw new Exception($"找不到網站 Upload 目錄：{orgName}");
        }

        public string GetDirectoryPath(string orgName, string directory)
        {
            return Path.Combine(GetRootPath(orgName), directory);
        }

        public string GetPhysicalPath(string orgName, string relativePath)
        {
            relativePath = relativePath
                .TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            var rootPath = GetRootPath(orgName);
            var physicalPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
            var safeRoot = Path.GetFullPath(rootPath);

            if (!physicalPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
                throw new Exception("檔案路徑不合法");

            return physicalPath;
        }

        public string GetPhysicalPathFromDownloadFileName(string orgName, string downloadFileName)
        {
            if (string.IsNullOrWhiteSpace(downloadFileName))
                throw new Exception("DownloadFileName 不可為空");

            var p = downloadFileName.Trim().Replace("\\", "/");

            if (p.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                throw new Exception("外部 URL 無法轉換為實體檔案路徑");

            const string uploadRoot = "/upload/";
            string relativePath;

            if (p.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = p.Substring(uploadRoot.Length);

                if (relativePath.StartsWith(orgName + "/", StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(orgName.Length + 1);
            }
            else
            {
                relativePath = p.TrimStart('/');
            }

            return GetPhysicalPath(orgName, relativePath);
        }
    }
}
