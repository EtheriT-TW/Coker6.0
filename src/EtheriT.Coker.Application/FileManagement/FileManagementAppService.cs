using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.FileManagement;
using Microsoft.Extensions.Options;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.FileManagement
{
    public class FileManagementAppService : IFileManagementAppService
    {
        private readonly IConfiguration _configuration;
        private readonly LoginUserData _loginUserData;
        private readonly CokerDbContext _dbContext;
        private readonly IUploadPathResolver _uploadPathResolver;
        private readonly IFileReferenceScanner _fileReferenceScanner;
        private readonly FileAllow _fileAllow;
        private IThumbnailGeneratorService _thumbnailGenerator { get; }

        public FileManagementAppService(
            IConfiguration configuration,
            LoginUserData loginUserData,
            CokerDbContext dbContext,
            IThumbnailGeneratorService thumbnailGenerator,
            IUploadPathResolver uploadPathResolver,
            IFileReferenceScanner fileReferenceScanner,
            IOptions<VirtualDirectory> virtualDirectory)
        {
            _configuration = configuration;
            _loginUserData = loginUserData;
            _dbContext = dbContext;
            _thumbnailGenerator = thumbnailGenerator;
            _uploadPathResolver = uploadPathResolver;
            _fileReferenceScanner = fileReferenceScanner;
            _fileAllow = virtualDirectory.Value.FileAllow;
        }

        public object FileSystem(FileSystemCommand command, string arguments, HttpRequest request)
        {
            string orgName = _loginUserData.GetWebsiteOrgName().Result;
            var filePath = _uploadPathResolver.GetRootPath(orgName);

            // 取得目前登入使用者的 ID
            long userId = _loginUserData.GetUserId().Result;
            var allowFileAllowMIME = _fileAllow.Ext ?? new List<string>();

            // 使用 FileExtensionContentTypeProvider 從 MIME 值反推取得副檔名
            var provider = new FileExtensionContentTypeProvider();

            // 從 MIME 反向查找副檔名並只保留允許的副檔名
            var allowFileExtension = provider.Mappings.Where(x => allowFileAllowMIME.Contains(x.Value))
                                                      .Select(x => x.Key)
                                                      .OrderBy(x => x)
                                                      .Distinct()
                                                      .ToList();

            allowFileExtension.Add(".avif");

            if (!System.IO.Directory.Exists(filePath))
            {
                System.IO.Directory.CreateDirectory(filePath);
            }

            var customFileSystemProvider = new CustomFileSystemProvider(
                                                                        (fileInfo, item) =>
                                                                        {
                                                                            var relativePath = Path.GetRelativePath(filePath, fileInfo.FullName);
                                                                            _thumbnailGenerator.AssignThumbnailUrl(fileInfo, item, relativePath);
                                                                        },
                                                                        filePath,
                                                                        _dbContext,
                                                                        orgName,
                                                                        userId,
                                                                        _configuration,
                                                                        request,
                                                                        _fileReferenceScanner,
                                                                        _uploadPathResolver);

            var config = new FileSystemConfiguration
            {
                Request = request,
                FileSystemProvider = customFileSystemProvider,

                //uncomment the code below to enable file/folder management
                //AllowCopy = true,
                AllowCreate = true,
                AllowMove = true,
                AllowDelete = true,
                AllowRename = true,
                AllowUpload = true,
                AllowDownload = true,
                AllowedFileExtensions = allowFileExtension.ToArray()
            };

            var processor = new FileSystemCommandProcessor(config);
            var result = processor.Execute(command, arguments);
            return result.GetClientCommandResult();
        }

        public async Task<bool> CheckFileHasBindingsAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                string orgName = await _loginUserData.GetWebsiteOrgName();
                long websiteId = await _loginUserData.GetWebsiteId();

                // 正規化檔案路徑，處理不同格式的路徑
                string normalizedPath = filePath.Replace("\\", "/").TrimStart('/');

                // 可能的路徑格式
                var possiblePaths = new List<string>
                {
                    $"/upload/{orgName}/{normalizedPath}",
                    $"/upload/{normalizedPath}",
                    $"\\upload\\{orgName}\\{normalizedPath.Replace("/", "\\")}",
                    $"\\upload\\{normalizedPath.Replace("/", "\\")}"
                };

                // 檢查是否存在於 FileUploads.DownloadFileName
                var fileUpload = await _dbContext.FileUploads
                    .Where(f => f.FK_WebsiteId == websiteId &&
                               !f.IsDeleted &&
                               f.DownloadFileName != null &&
                               possiblePaths.Contains(f.DownloadFileName))
                    .FirstOrDefaultAsync();

                if (fileUpload == null)
                {
                    return false;
                }

                var referencedIds = await _fileReferenceScanner
                    .GetReferencedFileIdsAsync(websiteId);
                return referencedIds.Contains(fileUpload.Id);
            }
            catch (Exception)
            {
                // 無法完成引用檢查時採取保守策略，避免誤刪仍在使用的檔案。
                return true;
            }
        }

        public async Task<bool> CheckFileExistsAsync(string directoryPath, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                string orgName = await _loginUserData.GetWebsiteOrgName();

                // 組合完整的檔案路徑
                var relativePath = System.IO.Path.Combine(
                    directoryPath.TrimStart('/').TrimStart('\\'),
                    fileName);

                string fullPath = _uploadPathResolver.GetPhysicalPath(orgName, relativePath);

                // 檢查檔案是否存在
                return System.IO.File.Exists(fullPath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<FileCleanupItemDto>> GetUnreferencedFilesAsync()
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var orgName = await _loginUserData.GetWebsiteOrgName();
            var rows = await (
                from candidate in _dbContext.FileCleanupCandidates.AsNoTracking()
                join file in _dbContext.FileUploads.AsNoTracking()
                    on candidate.FK_FileUploadId equals file.Id
                where candidate.FK_WebsiteId == websiteId
                    && file.FK_WebsiteId == websiteId
                    && !candidate.IsDeleted
                    && !file.IsDeleted
                orderby candidate.FirstDetectedTime descending
                select new { candidate, file }
            ).ToListAsync();

            return rows.Select(row => CreateCleanupItem(
                row.file,
                orgName,
                row.candidate.FirstDetectedTime,
                row.candidate.Reason)).ToList();
        }

        public async Task<IReadOnlyList<FileCleanupItemDto>> GetRecycleBinFilesAsync()
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var orgName = await _loginUserData.GetWebsiteOrgName();
            var files = await (
                from recycle in _dbContext.FileRecycleBinItems.AsNoTracking()
                join file in _dbContext.FileUploads.IgnoreQueryFilters().AsNoTracking()
                    on recycle.FK_FileUploadId equals file.Id
                where recycle.FK_WebsiteId == websiteId
                    && !recycle.IsDeleted
                    && file.FK_WebsiteId == websiteId
                    && file.IsDeleted
                orderby recycle.RecycledTime descending
                select file
            )
                .Take(2_000)
                .ToListAsync();

            return files.Select(file => CreateCleanupItem(file, orgName, isRecycle: true))
                .Where(item => item.PhysicalFileExists)
                .ToList();
        }

        public async Task MoveToRecycleBinAsync(long fileUploadId)
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var orgName = await _loginUserData.GetWebsiteOrgName();
            var userId = await _loginUserData.GetUserId();
            var candidateExists = await _dbContext.FileCleanupCandidates
                .AnyAsync(item => item.FK_WebsiteId == websiteId
                    && item.FK_FileUploadId == fileUploadId
                    && !item.IsDeleted);
            if (!candidateExists)
                throw new InvalidOperationException("此檔案不在查無引用清單中，請先重新掃描。");

            var referencedIds = await _fileReferenceScanner
                .GetReferencedFileIdsAsync(websiteId);
            var familyIds = await GetFileFamilyIdsAsync(websiteId, fileUploadId);
            if (familyIds.Any(referencedIds.Contains))
                throw new InvalidOperationException("檔案目前已有引用，已取消移至資源回收桶。");

            var now = DateTime.Now;
            var files = await _dbContext.FileUploads
                .Where(item => familyIds.Contains(item.Id))
                .ToListAsync();
            var movedFiles = FileRecycleStorage.MoveToRecycleBin(
                _uploadPathResolver,
                orgName,
                files);
            foreach (var file in files)
            {
                file.IsDeleted = true;
                file.DeletionTime = now;
                file.DeleterUserId = userId;
            }
            await UpsertRecycleBinEntriesAsync(
                files,
                websiteId,
                now,
                userId,
                "由查無引用清單移入");
            await HideCleanupCandidatesAsync(familyIds, now, userId);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                FileRecycleStorage.RestoreMovedFiles(movedFiles);
                throw;
            }
        }

        public async Task<FileCleanupBatchResultDto> MoveAllUnreferencedToRecycleBinAsync()
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var orgName = await _loginUserData.GetWebsiteOrgName();
            var userId = await _loginUserData.GetUserId();
            var candidateIds = await (
                from candidate in _dbContext.FileCleanupCandidates.AsNoTracking()
                join file in _dbContext.FileUploads.AsNoTracking()
                    on candidate.FK_FileUploadId equals file.Id
                where candidate.FK_WebsiteId == websiteId
                    && file.FK_WebsiteId == websiteId
                    && !candidate.IsDeleted
                    && !file.IsDeleted
                select file.Id
            ).Distinct().ToListAsync();

            if (candidateIds.Count == 0)
                return new FileCleanupBatchResultDto();

            // 掃描完成後內容可能又被編輯，因此批次移動前仍須重新檢查引用。
            var referencedIds = await _fileReferenceScanner
                .GetReferencedFileIdsAsync(websiteId);
            var familyGraph = await GetFileFamilyGraphAsync(websiteId);
            var eligibleRoots = new List<long>();
            var eligibleFileIds = new HashSet<long>();

            foreach (var candidateId in candidateIds)
            {
                var familyIds = ExpandFileFamily(candidateId, familyGraph);
                if (familyIds.Any(referencedIds.Contains))
                    continue;

                eligibleRoots.Add(candidateId);
                eligibleFileIds.UnionWith(familyIds);
            }

            if (eligibleFileIds.Count > 0)
            {
                var now = DateTime.Now;
                var files = await _dbContext.FileUploads
                    .Where(item => item.FK_WebsiteId == websiteId
                        && eligibleFileIds.Contains(item.Id))
                    .ToListAsync();
                var movedFiles = FileRecycleStorage.MoveToRecycleBin(
                    _uploadPathResolver,
                    orgName,
                    files);
                foreach (var file in files)
                {
                    file.IsDeleted = true;
                    file.DeletionTime = now;
                    file.DeleterUserId = userId;
                }

                await UpsertRecycleBinEntriesAsync(
                    files,
                    websiteId,
                    now,
                    userId,
                    "由查無引用清單批次移入");
                await HideCleanupCandidatesAsync(eligibleFileIds, now, userId);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch
                {
                    FileRecycleStorage.RestoreMovedFiles(movedFiles);
                    throw;
                }
            }

            return new FileCleanupBatchResultDto
            {
                MovedCount = eligibleRoots.Count,
                SkippedCount = candidateIds.Count - eligibleRoots.Count
            };
        }

        public async Task RestoreFromRecycleBinAsync(long fileUploadId)
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var orgName = await _loginUserData.GetWebsiteOrgName();
            var userId = await _loginUserData.GetUserId();
            await EnsureRecycleBinEntryAsync(websiteId, fileUploadId);
            var familyIds = await GetFileFamilyIdsAsync(websiteId, fileUploadId);
            var files = await _dbContext.FileUploads.IgnoreQueryFilters()
                .Where(item => item.FK_WebsiteId == websiteId
                    && familyIds.Contains(item.Id))
                .ToListAsync();
            if (files.Count == 0)
                throw new InvalidOperationException("找不到可還原的檔案。");

            var restorePaths = files
                .Where(item => !string.IsNullOrWhiteSpace(item.DownloadFileName))
                .Select(item => item.DownloadFileName!)
                .ToList();
            var pathConflict = await _dbContext.FileUploads
                .AnyAsync(item => item.FK_WebsiteId == websiteId
                    && !familyIds.Contains(item.Id)
                    && item.DownloadFileName != null
                    && restorePaths.Contains(item.DownloadFileName));
            if (pathConflict)
                throw new InvalidOperationException("原路徑已有其他檔案，無法直接還原。");

            var restoredFiles = FileRecycleStorage.RestoreFromRecycleBin(
                _uploadPathResolver,
                orgName,
                files);
            foreach (var file in files)
            {
                file.IsDeleted = false;
                file.DeletionTime = null;
                file.DeleterUserId = null;
                file.LastModificationTime = DateTime.Now;
                file.LastModifierUserId = userId;
            }
            await CloseRecycleBinEntriesAsync(familyIds, DateTime.Now, userId);
            await HideCleanupCandidatesAsync(familyIds, DateTime.Now, userId);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                FileRecycleStorage.MoveRestoredFilesBack(restoredFiles);
                throw;
            }
        }

        public async Task PermanentlyDeleteAsync(long fileUploadId)
        {
            var websiteId = await _loginUserData.GetWebsiteId();
            var orgName = await _loginUserData.GetWebsiteOrgName();
            var userId = await _loginUserData.GetUserId();
            await EnsureRecycleBinEntryAsync(websiteId, fileUploadId);
            var familyIds = await GetFileFamilyIdsAsync(websiteId, fileUploadId);
            var referencedIds = await _fileReferenceScanner
                .GetReferencedFileIdsAsync(websiteId);
            if (familyIds.Any(referencedIds.Contains))
                throw new InvalidOperationException("檔案目前已有引用，無法永久刪除。");

            var files = await _dbContext.FileUploads.IgnoreQueryFilters()
                .Where(item => item.FK_WebsiteId == websiteId
                    && item.IsDeleted
                    && familyIds.Contains(item.Id))
                .ToListAsync();
            if (files.Count == 0)
                throw new InvalidOperationException("找不到資源回收桶中的檔案。");

            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file.DownloadFileName))
                    continue;
                FileRecycleStorage.PermanentlyDelete(
                    _uploadPathResolver,
                    orgName,
                    file.DownloadFileName);
            }

            var relations = await _dbContext.FileBindMores.IgnoreQueryFilters()
                .Where(item => !item.IsDeleted
                    && item.FK_FileUploadId.HasValue
                    && familyIds.Contains(item.FK_FileUploadId.Value))
                .ToListAsync();
            foreach (var relation in relations)
            {
                relation.IsDeleted = true;
                relation.DeletionTime = DateTime.Now;
                relation.DeleterUserId = userId;
            }
            await CloseRecycleBinEntriesAsync(familyIds, DateTime.Now, userId);
            await HideCleanupCandidatesAsync(familyIds, DateTime.Now, userId);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<HashSet<long>> GetFileFamilyIdsAsync(long websiteId, long fileUploadId)
        {
            var fileExists = await _dbContext.FileUploads.IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(item => item.FK_WebsiteId == websiteId
                    && item.Id == fileUploadId);
            if (!fileExists)
                throw new InvalidOperationException("找不到指定檔案。");

            var familyGraph = await GetFileFamilyGraphAsync(websiteId);
            return ExpandFileFamily(fileUploadId, familyGraph);
        }

        private async Task<Dictionary<long, HashSet<long>>> GetFileFamilyGraphAsync(
            long websiteId)
        {
            var uploads = await _dbContext.FileUploads.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId)
                .Select(item => new { item.Id, item.GuidKey })
                .ToListAsync();
            var uploadByGuid = uploads.ToDictionary(item => item.GuidKey, item => item.Id);
            var uploadIds = uploads.Select(item => item.Id).ToHashSet();
            var relations = await (
                from relation in _dbContext.FileBindMores.IgnoreQueryFilters().AsNoTracking()
                join upload in _dbContext.FileUploads.IgnoreQueryFilters().AsNoTracking()
                    on relation.FK_FileUploadId equals (long?)upload.Id
                where !relation.IsDeleted && upload.FK_WebsiteId == websiteId
                select new { relation.FK_FileBindGuid, FileId = upload.Id }
            ).ToListAsync();

            var graph = new Dictionary<long, HashSet<long>>();
            foreach (var relation in relations)
            {
                if (!uploadByGuid.TryGetValue(relation.FK_FileBindGuid, out var parentId)
                    || !uploadIds.Contains(relation.FileId))
                    continue;

                AddFileFamilyEdge(graph, parentId, relation.FileId);
                AddFileFamilyEdge(graph, relation.FileId, parentId);
            }

            return graph;
        }

        private static HashSet<long> ExpandFileFamily(
            long fileUploadId,
            Dictionary<long, HashSet<long>> graph)
        {
            var familyIds = new HashSet<long> { fileUploadId };
            var pending = new Queue<long>();
            pending.Enqueue(fileUploadId);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                if (!graph.TryGetValue(current, out var relatedIds))
                    continue;

                foreach (var relatedId in relatedIds)
                {
                    if (familyIds.Add(relatedId))
                        pending.Enqueue(relatedId);
                }
            }
            return familyIds;
        }

        private static void AddFileFamilyEdge(
            Dictionary<long, HashSet<long>> graph,
            long source,
            long target)
        {
            if (!graph.TryGetValue(source, out var relatedIds))
            {
                relatedIds = new HashSet<long>();
                graph[source] = relatedIds;
            }
            relatedIds.Add(target);
        }

        private async Task EnsureRecycleBinEntryAsync(long websiteId, long fileUploadId)
        {
            var exists = await _dbContext.FileRecycleBinItems
                .AnyAsync(item => item.FK_WebsiteId == websiteId
                    && item.FK_FileUploadId == fileUploadId
                    && !item.IsDeleted);
            if (!exists)
                throw new InvalidOperationException("找不到資源回收桶中的檔案。");
        }

        private async Task HideCleanupCandidatesAsync(
            HashSet<long> fileIds,
            DateTime now,
            long userId)
        {
            var candidates = await _dbContext.FileCleanupCandidates
                .Where(item => fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var candidate in candidates)
            {
                candidate.IsDeleted = true;
                candidate.DeletionTime = now;
                candidate.DeleterUserId = userId;
            }
        }

        private async Task UpsertRecycleBinEntriesAsync(
            IReadOnlyCollection<FileUpload> files,
            long websiteId,
            DateTime now,
            long userId,
            string reason)
        {
            var fileIds = files.Select(item => item.Id).ToHashSet();
            var entries = await _dbContext.FileRecycleBinItems.IgnoreQueryFilters()
                .Where(item => item.FK_WebsiteId == websiteId
                    && fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var file in files)
            {
                var entry = entries.FirstOrDefault(item => item.FK_FileUploadId == file.Id);
                if (entry == null)
                {
                    _dbContext.FileRecycleBinItems.Add(new FileRecycleBinItem
                    {
                        FK_WebsiteId = websiteId,
                        FK_FileUploadId = file.Id,
                        RecycledTime = now,
                        OriginalPath = file.DownloadFileName ?? string.Empty,
                        Reason = reason,
                        CreatorUserId = userId
                    });
                    continue;
                }

                entry.IsDeleted = false;
                entry.DeletionTime = null;
                entry.DeleterUserId = null;
                entry.RecycledTime = now;
                entry.OriginalPath = file.DownloadFileName ?? entry.OriginalPath;
                entry.Reason = reason;
                entry.LastModificationTime = now;
                entry.LastModifierUserId = userId;
            }
        }

        private async Task CloseRecycleBinEntriesAsync(
            HashSet<long> fileIds,
            DateTime now,
            long userId)
        {
            var entries = await _dbContext.FileRecycleBinItems
                .Where(item => fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var entry in entries)
            {
                entry.IsDeleted = true;
                entry.DeletionTime = now;
                entry.DeleterUserId = userId;
            }
        }

        private FileCleanupItemDto CreateCleanupItem(
            FileUpload file,
            string orgName,
            DateTime? detectedTime = null,
            string reason = "",
            bool isRecycle = false)
        {
            var physicalExists = false;
            if (!string.IsNullOrWhiteSpace(file.DownloadFileName))
            {
                try
                {
                    var physicalPath = isRecycle
                        ? FileRecycleStorage.GetStoredRecyclePath(
                            _uploadPathResolver,
                            orgName,
                            file.DownloadFileName)
                        : _uploadPathResolver.GetPhysicalPathFromDownloadFileName(
                            orgName,
                            file.DownloadFileName);
                    physicalExists = System.IO.File.Exists(physicalPath);
                }
                catch
                {
                    physicalExists = false;
                }
            }
            var path = file.DownloadFileName ?? string.Empty;
            var url = isRecycle
                ? $"/api/FileManagement/RecycleThumbnail?fileUploadId={file.Id}"
                : path.Replace(
                    "/upload/",
                    $"/upload/{orgName}/",
                    StringComparison.OrdinalIgnoreCase);
            return new FileCleanupItemDto
            {
                Id = file.Id,
                Name = file.OriginalFileName ?? Path.GetFileName(path),
                Path = path,
                Url = url,
                ContentType = file.ContentType ?? string.Empty,
                Size = file.Size,
                CreationTime = file.CreationTime,
                DetectedTime = detectedTime,
                DeletionTime = file.DeletionTime,
                Reason = reason,
                PhysicalFileExists = physicalExists
            };
        }
    }
}
