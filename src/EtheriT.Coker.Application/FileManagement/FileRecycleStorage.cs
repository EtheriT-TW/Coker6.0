using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.Application.FileManagement
{
    public sealed record FileRecycleMove(string OriginalPath, string RecyclePath);

    public static class FileRecycleStorage
    {
        public const string DirectoryName = "_Remove";

        public static IReadOnlyList<FileRecycleMove> MoveToRecycleBin(
            IUploadPathResolver uploadPathResolver,
            string orgName,
            IEnumerable<FileUpload> files)
        {
            var moved = new List<FileRecycleMove>();
            try
            {
                foreach (var file in files)
                {
                    if (string.IsNullOrWhiteSpace(file.DownloadFileName))
                        continue;

                    var originalPath = uploadPathResolver
                        .GetPhysicalPathFromDownloadFileName(orgName, file.DownloadFileName);
                    var recyclePath = GetRecyclePath(
                        uploadPathResolver,
                        orgName,
                        file.DownloadFileName);

                    if (!File.Exists(originalPath))
                    {
                        // 已搬入隔離區時視為完成，讓先前中斷的操作可以安全重試。
                        if (File.Exists(recyclePath))
                            continue;
                        throw new FileNotFoundException("找不到要移入資源回收桶的實體檔案。", originalPath);
                    }
                    if (File.Exists(recyclePath))
                        throw new IOException($"資源回收桶已存在同路徑檔案：{recyclePath}");

                    Directory.CreateDirectory(Path.GetDirectoryName(recyclePath)!);
                    File.Move(originalPath, recyclePath);
                    moved.Add(new FileRecycleMove(originalPath, recyclePath));
                }
                return moved;
            }
            catch
            {
                RestoreMovedFiles(moved);
                throw;
            }
        }

        public static IReadOnlyList<FileRecycleMove> RestoreFromRecycleBin(
            IUploadPathResolver uploadPathResolver,
            string orgName,
            IEnumerable<FileUpload> files)
        {
            var restored = new List<FileRecycleMove>();
            try
            {
                foreach (var file in files)
                {
                    if (string.IsNullOrWhiteSpace(file.DownloadFileName))
                        continue;

                    var originalPath = uploadPathResolver
                        .GetPhysicalPathFromDownloadFileName(orgName, file.DownloadFileName);
                    var recyclePath = GetRecyclePath(
                        uploadPathResolver,
                        orgName,
                        file.DownloadFileName);

                    // 相容功能上線前只做軟刪除、檔案仍在原路徑的回收桶資料。
                    if (!File.Exists(recyclePath))
                    {
                        if (File.Exists(originalPath))
                            continue;
                        throw new FileNotFoundException("找不到可還原的實體檔案。", recyclePath);
                    }
                    if (File.Exists(originalPath))
                        throw new IOException($"原路徑已有檔案，無法還原：{originalPath}");

                    Directory.CreateDirectory(Path.GetDirectoryName(originalPath)!);
                    File.Move(recyclePath, originalPath);
                    restored.Add(new FileRecycleMove(originalPath, recyclePath));
                    DeleteEmptyRecycleDirectories(uploadPathResolver, orgName, recyclePath);
                }
                return restored;
            }
            catch
            {
                MoveRestoredFilesBack(restored);
                throw;
            }
        }

        public static string GetStoredRecyclePath(
            IUploadPathResolver uploadPathResolver,
            string orgName,
            string downloadFileName)
        {
            var recyclePath = GetRecyclePath(uploadPathResolver, orgName, downloadFileName);
            if (File.Exists(recyclePath))
                return recyclePath;

            // 相容功能上線前的回收桶資料。
            return uploadPathResolver.GetPhysicalPathFromDownloadFileName(orgName, downloadFileName);
        }

        public static void PermanentlyDelete(
            IUploadPathResolver uploadPathResolver,
            string orgName,
            string downloadFileName)
        {
            var recyclePath = GetRecyclePath(uploadPathResolver, orgName, downloadFileName);
            if (File.Exists(recyclePath))
            {
                File.Delete(recyclePath);
                DeleteEmptyRecycleDirectories(uploadPathResolver, orgName, recyclePath);
                return;
            }

            // 相容功能上線前尚未搬入隔離區的回收桶資料。
            var originalPath = uploadPathResolver
                .GetPhysicalPathFromDownloadFileName(orgName, downloadFileName);
            if (File.Exists(originalPath))
                File.Delete(originalPath);
        }

        public static void RestoreMovedFiles(IEnumerable<FileRecycleMove> movedFiles)
        {
            foreach (var moved in movedFiles.Reverse())
            {
                if (!File.Exists(moved.RecyclePath) || File.Exists(moved.OriginalPath))
                    continue;
                Directory.CreateDirectory(Path.GetDirectoryName(moved.OriginalPath)!);
                File.Move(moved.RecyclePath, moved.OriginalPath);
            }
        }

        public static void MoveRestoredFilesBack(IEnumerable<FileRecycleMove> restoredFiles)
        {
            foreach (var restored in restoredFiles.Reverse())
            {
                if (!File.Exists(restored.OriginalPath) || File.Exists(restored.RecyclePath))
                    continue;
                Directory.CreateDirectory(Path.GetDirectoryName(restored.RecyclePath)!);
                File.Move(restored.OriginalPath, restored.RecyclePath);
            }
        }

        private static string GetRecyclePath(
            IUploadPathResolver uploadPathResolver,
            string orgName,
            string downloadFileName)
        {
            var rootPath = Path.GetFullPath(uploadPathResolver.GetRootPath(orgName));
            var originalPath = Path.GetFullPath(uploadPathResolver
                .GetPhysicalPathFromDownloadFileName(orgName, downloadFileName));
            var relativePath = Path.GetRelativePath(rootPath, originalPath);
            if (relativePath.StartsWith("..", StringComparison.Ordinal)
                || Path.IsPathRooted(relativePath))
            {
                throw new IOException("檔案不在網站 Upload 目錄內。");
            }

            var recycleRoot = Path.GetFullPath(Path.Combine(rootPath, DirectoryName));
            var recyclePath = Path.GetFullPath(Path.Combine(recycleRoot, relativePath));
            if (!recyclePath.StartsWith(
                recycleRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("資源回收桶路徑不合法。");
            }
            return recyclePath;
        }

        private static void DeleteEmptyRecycleDirectories(
            IUploadPathResolver uploadPathResolver,
            string orgName,
            string filePath)
        {
            var recycleRoot = Path.GetFullPath(Path.Combine(
                uploadPathResolver.GetRootPath(orgName),
                DirectoryName));
            var directory = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrWhiteSpace(directory)
                && directory.StartsWith(recycleRoot, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(directory, recycleRoot, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.EnumerateFileSystemEntries(directory).Any())
                    break;
                Directory.Delete(directory);
                directory = Path.GetDirectoryName(directory);
            }
        }
    }
}
