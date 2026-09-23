using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.Application.Shared.JsonObject;
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
        private readonly IWebsiteCacheStateAppService _websiteCacheStateAppService;
        private readonly FileAllow _fileAllow;
        private IThumbnailGeneratorService _thumbnailGenerator { get; }

        public FileManagementAppService(
            IConfiguration configuration,
            LoginUserData loginUserData,
            CokerDbContext dbContext,
            IThumbnailGeneratorService thumbnailGenerator,
            IUploadPathResolver uploadPathResolver,
            IFileReferenceScanner fileReferenceScanner,
            IWebsiteCacheStateAppService websiteCacheStateAppService,
            IOptions<VirtualDirectory> virtualDirectory)
        {
            _configuration = configuration;
            _loginUserData = loginUserData;
            _dbContext = dbContext;
            _thumbnailGenerator = thumbnailGenerator;
            _uploadPathResolver = uploadPathResolver;
            _fileReferenceScanner = fileReferenceScanner;
            _websiteCacheStateAppService = websiteCacheStateAppService;
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
            var rows = await (
                from recycle in _dbContext.FileRecycleBinItems.AsNoTracking()
                join file in _dbContext.FileUploads.IgnoreQueryFilters().AsNoTracking()
                    on recycle.FK_FileUploadId equals file.Id
                where recycle.FK_WebsiteId == websiteId
                    && !recycle.IsDeleted
                    && file.FK_WebsiteId == websiteId
                orderby recycle.RecycledTime descending
                select new { recycle, file }
            )
                .Take(2_000)
                .ToListAsync();

            return rows.Select(row => CreateCleanupItem(
                    row.file,
                    orgName,
                    reason: row.file.IsDeleted
                        ? row.recycle.Reason
                        : $"{row.recycle.Reason}；原檔仍被其他內容使用，本項只會還原關聯",
                    isRecycle: true,
                    recycleTime: row.recycle.RecycledTime))
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

            var recycleBindings = await _dbContext.FileRecycleBinBindings
                .Where(item => item.FK_WebsiteId == websiteId
                    && familyIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            var bindingGuids = recycleBindings
                .Select(item => item.FK_FileBindGuid)
                .ToHashSet();
            var bindings = await _dbContext.FileBinds.IgnoreQueryFilters()
                .Where(item => bindingGuids.Contains(item.Guid))
                .ToListAsync();
            await ValidateRestoreBindingsAsync(
                websiteId,
                bindings,
                bindingGuids);

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
            foreach (var binding in bindings)
            {
                binding.IsDeleted = false;
                binding.DeletionTime = null;
                binding.DeleterUserId = null;
                binding.LastModificationTime = DateTime.Now;
                binding.LastModifierUserId = userId;
            }
            await RestoreBoundFileSlotsAsync(websiteId, bindings, files);
            foreach (var recycleBinding in recycleBindings)
            {
                recycleBinding.IsDeleted = true;
                recycleBinding.DeletionTime = DateTime.Now;
                recycleBinding.DeleterUserId = userId;
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
            if (bindings.Any(item => item.type == (int)FileBindTypeEnum.選單圖
                || item.type == (int)FileBindTypeEnum.選單覆蓋
                || item.type == (int)FileBindTypeEnum.選單Icon))
            {
                await _websiteCacheStateAppService.TouchByWebsiteIdAsync(
                    websiteId,
                    WebsiteCacheKeys.Menu);
            }
        }

        public async Task PermanentlyDeleteAsync(long fileUploadId)
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
                throw new InvalidOperationException("找不到資源回收桶中的檔案。");

            var quarantinedFiles = files.Where(item => item.IsDeleted).ToList();
            if (quarantinedFiles.Count > 0)
            {
                var referencedIds = await _fileReferenceScanner
                    .GetReferencedFileIdsAsync(websiteId);
                if (familyIds.Any(referencedIds.Contains))
                    throw new InvalidOperationException("檔案目前已有引用，無法永久刪除。");

                foreach (var file in quarantinedFiles)
                {
                    if (string.IsNullOrWhiteSpace(file.DownloadFileName))
                        continue;
                    FileRecycleStorage.PermanentlyDelete(
                        _uploadPathResolver,
                        orgName,
                        file.DownloadFileName);
                }
            }

            if (quarantinedFiles.Count > 0)
            {
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
            }
            await CloseRecycleBinEntriesAsync(familyIds, DateTime.Now, userId);
            await CloseRecycleBinBindingsAsync(familyIds, DateTime.Now, userId);
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

        private async Task CloseRecycleBinBindingsAsync(
            HashSet<long> fileIds,
            DateTime now,
            long userId)
        {
            var entries = await _dbContext.FileRecycleBinBindings
                .Where(item => fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var entry in entries)
            {
                entry.IsDeleted = true;
                entry.DeletionTime = now;
                entry.DeleterUserId = userId;
            }
        }

        private async Task ValidateRestoreBindingsAsync(
            long websiteId,
            IReadOnlyCollection<FileBind> bindings,
            HashSet<Guid> expectedBindingGuids)
        {
            if (bindings.Count != expectedBindingGuids.Count)
                throw new InvalidOperationException("部分原始檔案關聯已不存在，無法完整還原。");
            var duplicateExclusiveSlot = bindings
                .Where(item => IsExclusiveFileBindType(item.type))
                .GroupBy(item => new { item.type, item.Sid })
                .Any(group => group.Count() > 1);
            if (duplicateExclusiveSlot)
                throw new InvalidOperationException("回收紀錄包含重複的單一圖片欄位，無法安全還原。");

            foreach (var binding in bindings)
            {
                if (!binding.FK_FileUploadId.HasValue)
                    throw new InvalidOperationException("原始檔案關聯不完整，無法還原。");
                if (!await RestoreTargetExistsAsync(websiteId, binding))
                    throw new InvalidOperationException(
                        $"原關聯目標已不存在，無法還原「{binding.Name}」。");

                var conflictQuery = _dbContext.FileBinds.AsNoTracking()
                    .Where(item => item.Guid != binding.Guid
                        && item.Sid == binding.Sid
                        && item.type == binding.type
                        && !item.IsDeleted);
                var hasConflict = IsExclusiveFileBindType(binding.type)
                    ? await conflictQuery.AnyAsync()
                    : await conflictQuery.AnyAsync(item =>
                        item.FK_FileUploadId == binding.FK_FileUploadId);
                if (hasConflict || await HasBoundSlotValueAsync(websiteId, binding))
                {
                    throw new InvalidOperationException(
                        $"「{GetFileBindTypeName(binding.type)}」目前已有其他檔案，無法還原舊關聯。");
                }
            }
        }

        private async Task<bool> RestoreTargetExistsAsync(long websiteId, FileBind binding)
        {
            switch ((FileBindTypeEnum)binding.type)
            {
                case FileBindTypeEnum.網站圖示:
                case FileBindTypeEnum.網站Logo:
                    return binding.Sid == websiteId
                        && await _dbContext.Websites.AnyAsync(item => item.Id == websiteId);
                case FileBindTypeEnum.選單圖:
                case FileBindTypeEnum.選單覆蓋:
                case FileBindTypeEnum.選單Icon:
                    return await _dbContext.WebMenus.AnyAsync(item =>
                        item.Id == binding.Sid
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);
                case FileBindTypeEnum.產品:
                case FileBindTypeEnum.產品檔案:
                    return await _dbContext.Prods.AnyAsync(item =>
                        item.Id == binding.Sid
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);
                case FileBindTypeEnum.產品規格圖:
                    return await (
                        from stock in _dbContext.Prod_Stocks
                        join product in _dbContext.Prods on stock.FK_Pid equals product.Id
                        where stock.Id == binding.Sid
                            && !stock.IsDeleted
                            && !product.IsDeleted
                            && product.FK_WebsiteId == websiteId
                        select stock.Id
                    ).AnyAsync();
                case FileBindTypeEnum.文章管理:
                case FileBindTypeEnum.文章檔案:
                    return await _dbContext.Article.AnyAsync(item =>
                        item.Id == binding.Sid
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);
                case FileBindTypeEnum.技術證照:
                    return await _dbContext.TechnicalCertificates.AnyAsync(item =>
                        item.Id == binding.Sid
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);
                case FileBindTypeEnum.右側浮動廣告:
                case FileBindTypeEnum.進入廣告:
                case FileBindTypeEnum.Html:
                    return await _dbContext.Html_Contents.AnyAsync(item =>
                        item.Id == binding.Sid
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);
                case FileBindTypeEnum.自訂廣告:
                    return await _dbContext.Advertise.AnyAsync(item =>
                        item.Id == binding.Sid
                        && item.FK_WebsiteId == websiteId
                        && !item.IsDeleted);
                default:
                    return true;
            }
        }

        private async Task<bool> HasBoundSlotValueAsync(long websiteId, FileBind binding)
        {
            if (!IsExclusiveFileBindType(binding.type))
                return false;

            switch ((FileBindTypeEnum)binding.type)
            {
                case FileBindTypeEnum.網站圖示:
                    return await _dbContext.Websites.AnyAsync(item =>
                        item.Id == websiteId && !string.IsNullOrEmpty(item.Icon));
                case FileBindTypeEnum.網站Logo:
                    return await _dbContext.Websites.AnyAsync(item =>
                        item.Id == websiteId && !string.IsNullOrEmpty(item.Logo));
                case FileBindTypeEnum.選單圖:
                    return await _dbContext.WebMenus.AnyAsync(item =>
                        item.Id == binding.Sid && item.FK_WebsiteId == websiteId
                        && item.ImgId.HasValue);
                case FileBindTypeEnum.選單覆蓋:
                    return await _dbContext.WebMenus.AnyAsync(item =>
                        item.Id == binding.Sid && item.FK_WebsiteId == websiteId
                        && item.OverImgId.HasValue);
                case FileBindTypeEnum.選單Icon:
                    return await _dbContext.WebMenus.AnyAsync(item =>
                        item.Id == binding.Sid && item.FK_WebsiteId == websiteId
                        && item.icon != null && item.icon != "" && item.icon != "empty");
                case FileBindTypeEnum.右側浮動廣告:
                case FileBindTypeEnum.進入廣告:
                    return await _dbContext.Html_Contents.AnyAsync(item =>
                        item.Id == binding.Sid && item.FK_WebsiteId == websiteId
                        && !string.IsNullOrEmpty(item.Img));
                default:
                    return false;
            }
        }

        private async Task RestoreBoundFileSlotsAsync(
            long websiteId,
            IReadOnlyCollection<FileBind> bindings,
            IReadOnlyCollection<FileUpload> files)
        {
            var filesById = files.ToDictionary(item => item.Id);
            foreach (var binding in bindings)
            {
                if (!binding.FK_FileUploadId.HasValue
                    || !filesById.TryGetValue(binding.FK_FileUploadId.Value, out var file))
                    continue;
                switch ((FileBindTypeEnum)binding.type)
                {
                    case FileBindTypeEnum.網站圖示:
                        var websiteIcon = await _dbContext.Websites
                            .FirstOrDefaultAsync(item => item.Id == websiteId);
                        if (websiteIcon != null) websiteIcon.Icon = file.DownloadFileName;
                        break;
                    case FileBindTypeEnum.網站Logo:
                        var websiteLogo = await _dbContext.Websites
                            .FirstOrDefaultAsync(item => item.Id == websiteId);
                        if (websiteLogo != null) websiteLogo.Logo = file.DownloadFileName;
                        break;
                    case FileBindTypeEnum.選單圖:
                        var menuImage = await _dbContext.WebMenus.FirstAsync(item =>
                            item.Id == binding.Sid && item.FK_WebsiteId == websiteId);
                        menuImage.ImgId = file.Id;
                        break;
                    case FileBindTypeEnum.選單覆蓋:
                        var menuOverImage = await _dbContext.WebMenus.FirstAsync(item =>
                            item.Id == binding.Sid && item.FK_WebsiteId == websiteId);
                        menuOverImage.OverImgId = file.Id;
                        break;
                    case FileBindTypeEnum.選單Icon:
                        var menuIcon = await _dbContext.WebMenus.FirstAsync(item =>
                            item.Id == binding.Sid && item.FK_WebsiteId == websiteId);
                        menuIcon.icon = $"IconId:{file.Id}";
                        break;
                    case FileBindTypeEnum.右側浮動廣告:
                    case FileBindTypeEnum.進入廣告:
                        var html = await _dbContext.Html_Contents.FirstAsync(item =>
                            item.Id == binding.Sid && item.FK_WebsiteId == websiteId);
                        html.Img = file.DownloadFileName;
                        break;
                }
            }
        }

        private static bool IsExclusiveFileBindType(int type)
            => type == (int)FileBindTypeEnum.網站圖示
                || type == (int)FileBindTypeEnum.網站Logo
                || type == (int)FileBindTypeEnum.選單圖
                || type == (int)FileBindTypeEnum.選單覆蓋
                || type == (int)FileBindTypeEnum.選單Icon
                || type == (int)FileBindTypeEnum.右側浮動廣告
                || type == (int)FileBindTypeEnum.進入廣告
                || type == (int)FileBindTypeEnum.自訂廣告
                || type == (int)FileBindTypeEnum.分享圖示
                || type == (int)FileBindTypeEnum.大頭貼;

        private static string GetFileBindTypeName(int type)
            => Enum.IsDefined(typeof(FileBindTypeEnum), type)
                ? ((FileBindTypeEnum)type).ToString()
                : $"檔案類型 {type}";

        private FileCleanupItemDto CreateCleanupItem(
            FileUpload file,
            string orgName,
            DateTime? detectedTime = null,
            string reason = "",
            bool isRecycle = false,
            DateTime? recycleTime = null)
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
            var previewUrl = isRecycle
                ? $"/api/FileManagement/RecyclePreview?fileUploadId={file.Id}"
                : url;
            return new FileCleanupItemDto
            {
                Id = file.Id,
                Name = file.OriginalFileName ?? Path.GetFileName(path),
                Path = path,
                Url = url,
                PreviewUrl = previewUrl,
                ContentType = file.ContentType ?? string.Empty,
                Size = file.Size,
                CreationTime = file.CreationTime,
                DetectedTime = detectedTime,
                DeletionTime = recycleTime ?? file.DeletionTime,
                Reason = reason,
                PhysicalFileExists = physicalExists,
                IsQuarantined = file.IsDeleted
            };
        }
    }
}
