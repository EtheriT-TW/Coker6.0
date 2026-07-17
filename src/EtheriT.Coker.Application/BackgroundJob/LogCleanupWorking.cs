using EtheriT.Coker.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IODirectory = System.IO.Directory;

namespace EtheriT.Coker.Application.BackgroundJob
{
    /// <summary>
    /// 清除所有 UploadRoot 網站 logs 目錄中超過保留期限的檔案。
    /// </summary>
    public sealed class LogCleanupWorking
    {
        private const int RetentionDays = 30;

        private readonly VirtualDirectory virtualDirectory;
        private readonly ILogger<LogCleanupWorking> logger;

        public LogCleanupWorking(
            IOptions<VirtualDirectory> virtualDirectory,
            ILogger<LogCleanupWorking> logger)
        {
            this.virtualDirectory = virtualDirectory.Value;
            this.logger = logger;
        }

        public void CleanupExpiredLogs()
        {
            var thresholdUtc = DateTime.UtcNow.AddDays(-RetentionDays);
            var scannedFiles = 0;
            var deletedFiles = 0;

            foreach (var uploadRoot in GetUploadRoots())
            {
                if (!IODirectory.Exists(uploadRoot))
                {
                    logger.LogWarning("Log 清除略過不存在的 UploadRoot：{UploadRoot}", uploadRoot);
                    continue;
                }

                IEnumerable<string> siteDirectories;
                try
                {
                    siteDirectories = IODirectory
                        .EnumerateDirectories(uploadRoot, "*", SearchOption.TopDirectoryOnly)
                        .ToList();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Log 清除無法掃描 UploadRoot：{UploadRoot}", uploadRoot);
                    continue;
                }

                foreach (var siteDirectory in siteDirectories)
                {
                    var logsDirectory = Path.Combine(siteDirectory, "logs");
                    if (!IODirectory.Exists(logsDirectory))
                        continue;

                    CleanupLogsDirectory(logsDirectory, thresholdUtc, ref scannedFiles, ref deletedFiles);
                }
            }

            logger.LogInformation(
                "Log 清除完成：保留 {RetentionDays} 天，掃描 {ScannedFiles} 個檔案，刪除 {DeletedFiles} 個檔案",
                RetentionDays,
                scannedFiles,
                deletedFiles);
        }

        private IEnumerable<string> GetUploadRoots()
        {
            var roots = virtualDirectory.UploadRoots?.Values ?? Enumerable.Empty<string>();
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var root in roots)
            {
                if (string.IsNullOrWhiteSpace(root))
                    continue;

                try
                {
                    result.Add(Path.GetFullPath(root));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Log 清除略過無效的 UploadRoot：{UploadRoot}", root);
                }
            }

            return result;
        }

        private void CleanupLogsDirectory(
            string logsDirectory,
            DateTime thresholdUtc,
            ref int scannedFiles,
            ref int deletedFiles)
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint
            };

            IEnumerable<string> files;
            try
            {
                files = IODirectory.EnumerateFiles(logsDirectory, "*", options).ToList();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Log 清除無法掃描目錄：{LogsDirectory}", logsDirectory);
                return;
            }

            foreach (var file in files)
            {
                scannedFiles++;

                try
                {
                    if (File.GetLastWriteTimeUtc(file) >= thresholdUtc)
                        continue;

                    File.Delete(file);
                    deletedFiles++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Log 清除無法刪除檔案：{LogFile}", file);
                }
            }
        }
    }
}
