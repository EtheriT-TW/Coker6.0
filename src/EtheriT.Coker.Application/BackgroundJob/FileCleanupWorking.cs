using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.FileManagement;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class FileCleanupWorking
    {
        private readonly CokerDbContext db;
        private readonly IFileReferenceScanner referenceScanner;
        private readonly IUploadPathResolver uploadPathResolver;
        private readonly FileReferenceIndexingService referenceIndexingService;
        private readonly BackgroundTaskService backgroundTaskService;
        private readonly FileCleanupOptions options;
        private readonly ILogger<FileCleanupWorking> logger;

        public FileCleanupWorking(
            CokerDbContext db,
            IFileReferenceScanner referenceScanner,
            IUploadPathResolver uploadPathResolver,
            FileReferenceIndexingService referenceIndexingService,
            BackgroundTaskService backgroundTaskService,
            IOptions<FileCleanupOptions> options,
            ILogger<FileCleanupWorking> logger)
        {
            this.db = db;
            this.referenceScanner = referenceScanner;
            this.uploadPathResolver = uploadPathResolver;
            this.referenceIndexingService = referenceIndexingService;
            this.backgroundTaskService = backgroundTaskService;
            this.options = options.Value;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(7200)]
        public async Task ScanAllWebsitesAsync()
        {
            var websiteIds = await db.Websites.AsNoTracking()
                .Where(item => !item.IsDeleted)
                .Select(item => item.Id)
                .ToListAsync();
            foreach (var websiteId in websiteIds)
                await ScanWebsiteAsync(websiteId);
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(7200)]
        public async Task ScanWebsiteAsync(long websiteId)
        {
            var now = DateTime.Now;
            var cutoff = now.AddDays(-Math.Max(1, options.MinimumAgeDays));
            var maxFiles = Math.Clamp(options.MaxFilesPerScan, 100, 100_000);
            int registeredPhysicalFiles;
            try
            {
                // 先補登錄實體檔案，讓同一輪引用重建就能建立 FileUpload 關聯。
                registeredPhysicalFiles = await referenceIndexingService
                    .RegisterUntrackedPhysicalFilesAsync(websiteId);
            }
            catch (DirectoryNotFoundException ex)
            {
                // 本機或測試環境可能沒有複製所有網站的 Upload 目錄。
                // 當該網站實體檔案不完整時不進行掃描，避免產生錯誤的無引用判斷。
                logger.LogWarning(
                    ex,
                    "File reference scan skipped because the upload directory is unavailable. WebsiteId={WebsiteId}",
                    websiteId);
                return;
            }

            await referenceIndexingService.RebuildWebsiteAsync(websiteId);
            var referencedIds = await referenceScanner.GetReferencedFileIdsAsync(websiteId);
            var uploads = await db.FileUploads.AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId
                    && !item.IsDeleted
                    && item.CreationTime < cutoff
                    && item.DownloadFileName != null
                    && (item.DownloadFileName.StartsWith("/upload/")
                        || item.DownloadFileName.StartsWith("\\upload\\")))
                .OrderBy(item => item.Id)
                .Take(maxFiles)
                .Select(item => item.Id)
                .ToListAsync();
            var scannedIds = uploads.ToHashSet();
            var unreferencedIds = uploads.Where(id => !referencedIds.Contains(id)).ToHashSet();
            var candidates = await db.FileCleanupCandidates
                .IgnoreQueryFilters()
                .Where(item => item.FK_WebsiteId == websiteId)
                .ToListAsync();

            foreach (var candidate in candidates)
            {
                if (unreferencedIds.Contains(candidate.FK_FileUploadId))
                {
                    candidate.IsDeleted = false;
                    candidate.DeletionTime = null;
                    candidate.DeleterUserId = null;
                    candidate.LastConfirmedTime = now;
                    candidate.LastModificationTime = now;
                    candidate.Reason = "未在檔案綁定、草稿、已發布內容、CSS 或版型中找到引用";
                }
                else if (scannedIds.Contains(candidate.FK_FileUploadId) && !candidate.IsDeleted)
                {
                    candidate.IsDeleted = true;
                    candidate.DeletionTime = now;
                }
            }

            var knownIds = candidates.Select(item => item.FK_FileUploadId).ToHashSet();
            foreach (var fileId in unreferencedIds.Where(id => !knownIds.Contains(id)))
            {
                db.FileCleanupCandidates.Add(new FileCleanupCandidate
                {
                    FK_WebsiteId = websiteId,
                    FK_FileUploadId = fileId,
                    FirstDetectedTime = now,
                    LastConfirmedTime = now,
                    Reason = "未在檔案綁定、草稿、已發布內容、CSS 或版型中找到引用"
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation(
                "File reference scan completed. WebsiteId={WebsiteId}, Registered={Registered}, Scanned={Scanned}, Unreferenced={Unreferenced}",
                websiteId,
                registeredPhysicalFiles,
                uploads.Count,
                unreferencedIds.Count);
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(7200)]
        public async Task MoveSelectedToRecycleBinAsync(
            long taskId,
            long websiteId,
            long userId,
            long[] selectedFileIds)
        {
            try
            {
                var fileIds = selectedFileIds.Where(id => id > 0).Distinct().ToArray();
                await backgroundTaskService.UpdateProgressAsync(
                    taskId,
                    1,
                    $"正在檢查 {fileIds.Length} 筆檔案的引用狀態");

                var orgName = await db.Websites.AsNoTracking()
                    .Where(item => item.Id == websiteId && !item.IsDeleted)
                    .Select(item => item.OrgName)
                    .FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(orgName))
                    throw new InvalidOperationException("找不到檔案所屬網站。");

                // 批次只掃描一次，避免每個檔案都重新掃描整站內容。
                var referencedIds = await referenceScanner.GetReferencedFileIdsAsync(websiteId);
                var familyGraph = await GetFileFamilyGraphAsync(websiteId);
                var candidateIds = (await db.FileCleanupCandidates.AsNoTracking()
                    .Where(item => item.FK_WebsiteId == websiteId
                        && fileIds.Contains(item.FK_FileUploadId)
                        && !item.IsDeleted)
                    .Select(item => item.FK_FileUploadId)
                    .ToListAsync())
                    .ToHashSet();

                var processedIds = new HashSet<long>();
                var movedCount = 0;
                var missingCount = 0;
                var skippedCount = 0;
                var failedCount = 0;

                for (var index = 0; index < fileIds.Length; index++)
                {
                    var rootId = fileIds[index];
                    var familyIds = ExpandFileFamily(rootId, familyGraph);
                    if (!candidateIds.Contains(rootId) || familyIds.All(processedIds.Contains))
                    {
                        skippedCount++;
                    }
                    else if (familyIds.Any(referencedIds.Contains))
                    {
                        await HideCleanupCandidatesAsync(familyIds, DateTime.Now, userId);
                        await db.SaveChangesAsync();
                        db.ChangeTracker.Clear();
                        processedIds.UnionWith(familyIds);
                        skippedCount++;
                    }
                    else
                    {
                        IReadOnlyList<FileRecycleMove> movedFiles = Array.Empty<FileRecycleMove>();
                        try
                        {
                            var now = DateTime.Now;
                            var files = await db.FileUploads
                                .Where(item => item.FK_WebsiteId == websiteId
                                    && familyIds.Contains(item.Id))
                                .ToListAsync();
                            var existingFiles = files
                                .Where(item => !string.IsNullOrWhiteSpace(item.DownloadFileName)
                                    && File.Exists(FileRecycleStorage.GetStoredRecyclePath(
                                        uploadPathResolver,
                                        orgName,
                                        item.DownloadFileName!)))
                                .ToList();
                            var existingIds = existingFiles.Select(item => item.Id).ToHashSet();
                            var missingFiles = files.Where(item => !existingIds.Contains(item.Id)).ToList();
                            movedFiles = FileRecycleStorage.MoveToRecycleBin(
                                uploadPathResolver,
                                orgName,
                                existingFiles);

                            foreach (var file in files)
                            {
                                file.IsDeleted = true;
                                file.DeletionTime = now;
                                file.DeleterUserId = userId;
                            }
                            await UpsertRecycleBinEntriesAsync(
                                existingFiles,
                                websiteId,
                                now,
                                userId);
                            await RemoveMissingFileRecordsAsync(
                                missingFiles,
                                websiteId,
                                now,
                                userId);
                            await HideCleanupCandidatesAsync(familyIds, now, userId);

                            await db.SaveChangesAsync();
                            db.ChangeTracker.Clear();

                            processedIds.UnionWith(familyIds);
                            movedCount += existingFiles.Count;
                            missingCount += missingFiles.Count;
                        }
                        catch (Exception ex)
                        {
                            FileRecycleStorage.RestoreMovedFiles(movedFiles);
                            failedCount++;
                            db.ChangeTracker.Clear();
                            logger.LogError(
                                ex,
                                "Failed to recycle file family. WebsiteId={WebsiteId}, FileId={FileId}",
                                websiteId,
                                rootId);
                        }
                    }

                    var completed = index + 1;
                    await backgroundTaskService.UpdateProgressAsync(
                        taskId,
                        Math.Max(1, completed * 99 / Math.Max(1, fileIds.Length)),
                        $"已處理 {completed} / {fileIds.Length} 筆；搬移實體檔 {movedCount}、清除缺檔 {missingCount}、略過 {skippedCount}、失敗 {failedCount}");
                }

                var message = $"完成 {fileIds.Length} 筆：搬移實體檔 {movedCount}、清除缺少實體檔紀錄 {missingCount}、略過 {skippedCount}、失敗 {failedCount}";
                await backgroundTaskService.CompleteAsync(
                    taskId,
                    message,
                    resultJson: JsonSerializer.Serialize(new
                    {
                        MovedCount = movedCount,
                        MissingCount = missingCount,
                        SkippedCount = skippedCount,
                        FailedCount = failedCount
                    }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "File cleanup background task failed. TaskId={TaskId}", taskId);
                await backgroundTaskService.FailAsync(taskId, ex.Message);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(7200)]
        public async Task PermanentlyDeleteSelectedAsync(
            long taskId,
            long websiteId,
            long userId,
            long[] selectedFileIds)
        {
            try
            {
                var fileIds = selectedFileIds.Where(id => id > 0).Distinct().ToArray();
                await backgroundTaskService.UpdateProgressAsync(
                    taskId,
                    1,
                    $"正在檢查 {fileIds.Length} 筆回收桶項目");

                var orgName = await db.Websites.AsNoTracking()
                    .Where(item => item.Id == websiteId && !item.IsDeleted)
                    .Select(item => item.OrgName)
                    .FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(orgName))
                    throw new InvalidOperationException("找不到檔案所屬網站。");

                var referencedIds = await referenceScanner.GetReferencedFileIdsAsync(websiteId);
                var familyGraph = await GetFileFamilyGraphAsync(websiteId);
                var recycleIds = (await db.FileRecycleBinItems.AsNoTracking()
                    .Where(item => item.FK_WebsiteId == websiteId
                        && fileIds.Contains(item.FK_FileUploadId)
                        && !item.IsDeleted)
                    .Select(item => item.FK_FileUploadId)
                    .ToListAsync())
                    .ToHashSet();

                var processedIds = new HashSet<long>();
                var deletedFileCount = 0;
                var clearedEntryCount = 0;
                var skippedCount = 0;
                var failedCount = 0;

                for (var index = 0; index < fileIds.Length; index++)
                {
                    var rootId = fileIds[index];
                    var familyIds = ExpandFileFamily(rootId, familyGraph);
                    if (!recycleIds.Contains(rootId) || familyIds.All(processedIds.Contains))
                    {
                        skippedCount++;
                    }
                    else
                    {
                        try
                        {
                            var now = DateTime.Now;
                            var files = await db.FileUploads.IgnoreQueryFilters()
                                .Where(item => item.FK_WebsiteId == websiteId
                                    && familyIds.Contains(item.Id))
                                .ToListAsync();
                            var quarantinedFiles = files.Where(item => item.IsDeleted).ToList();
                            if (quarantinedFiles.Count > 0 && familyIds.Any(referencedIds.Contains))
                            {
                                skippedCount++;
                            }
                            else
                            {
                                foreach (var file in quarantinedFiles)
                                {
                                    if (string.IsNullOrWhiteSpace(file.DownloadFileName))
                                        continue;
                                    FileRecycleStorage.PermanentlyDelete(
                                        uploadPathResolver,
                                        orgName,
                                        file.DownloadFileName);
                                    deletedFileCount++;
                                }

                                if (quarantinedFiles.Count > 0)
                                {
                                    var relations = await db.FileBindMores.IgnoreQueryFilters()
                                        .Where(item => !item.IsDeleted
                                            && item.FK_FileUploadId.HasValue
                                            && familyIds.Contains(item.FK_FileUploadId.Value))
                                        .ToListAsync();
                                    foreach (var relation in relations)
                                    {
                                        relation.IsDeleted = true;
                                        relation.DeletionTime = now;
                                        relation.DeleterUserId = userId;
                                    }
                                }

                                clearedEntryCount += await CloseRecycleRecordsAsync(
                                    familyIds,
                                    websiteId,
                                    now,
                                    userId);
                                await HideCleanupCandidatesAsync(familyIds, now, userId);
                                await db.SaveChangesAsync();
                                db.ChangeTracker.Clear();
                                processedIds.UnionWith(familyIds);
                            }
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            db.ChangeTracker.Clear();
                            logger.LogError(
                                ex,
                                "Failed to permanently delete recycle-bin family. WebsiteId={WebsiteId}, FileId={FileId}",
                                websiteId,
                                rootId);
                        }
                    }

                    var completed = index + 1;
                    await backgroundTaskService.UpdateProgressAsync(
                        taskId,
                        Math.Max(1, completed * 99 / Math.Max(1, fileIds.Length)),
                        $"已處理 {completed} / {fileIds.Length} 筆；永久刪除實體檔 {deletedFileCount}、清除紀錄 {clearedEntryCount}、略過 {skippedCount}、失敗 {failedCount}");
                }

                var message = $"完成 {fileIds.Length} 筆：永久刪除實體檔 {deletedFileCount}、清除回收紀錄 {clearedEntryCount}、略過 {skippedCount}、失敗 {failedCount}";
                await backgroundTaskService.CompleteAsync(
                    taskId,
                    message,
                    resultJson: JsonSerializer.Serialize(new
                    {
                        DeletedFileCount = deletedFileCount,
                        ClearedEntryCount = clearedEntryCount,
                        SkippedCount = skippedCount,
                        FailedCount = failedCount
                    }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Recycle-bin cleanup background task failed. TaskId={TaskId}", taskId);
                await backgroundTaskService.FailAsync(taskId, ex.Message);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(7200)]
        public async Task PurgeExpiredRecycleBinAsync()
        {
            var cutoff = DateTime.Now.AddDays(-Math.Max(1, options.RecycleBinRetentionDays));
            var websiteIds = await db.FileRecycleBinItems
                .Where(item => !item.IsDeleted && item.RecycledTime < cutoff)
                .Select(item => item.FK_WebsiteId)
                .Distinct()
                .ToListAsync();

            foreach (var websiteId in websiteIds)
                await PurgeWebsiteAsync(websiteId, cutoff);
        }

        private async Task PurgeWebsiteAsync(long websiteId, DateTime cutoff)
        {
            var website = await db.Websites.AsNoTracking()
                .Where(item => item.Id == websiteId)
                .Select(item => new { item.OrgName })
                .FirstOrDefaultAsync();
            if (website == null)
                return;

            var referencedIds = await referenceScanner.GetReferencedFileIdsAsync(websiteId);
            var expired = await (
                from recycle in db.FileRecycleBinItems
                join file in db.FileUploads.IgnoreQueryFilters()
                    on recycle.FK_FileUploadId equals file.Id
                where recycle.FK_WebsiteId == websiteId
                    && !recycle.IsDeleted
                    && recycle.RecycledTime < cutoff
                orderby recycle.Id
                select new { recycle, file }
            )
                .Take(500)
                .ToListAsync();

            var purgedIds = new HashSet<long>();
            var closedIds = new HashSet<long>();
            foreach (var item in expired)
            {
                var file = item.file;
                if (!file.IsDeleted)
                {
                    // 檔案仍被其他內容使用，只清除已過期的「關聯還原」紀錄。
                    closedIds.Add(file.Id);
                    item.recycle.IsDeleted = true;
                    item.recycle.DeletionTime = DateTime.Now;
                    continue;
                }
                if (referencedIds.Contains(file.Id))
                    continue;

                if (!string.IsNullOrWhiteSpace(file.DownloadFileName))
                {
                    FileRecycleStorage.PermanentlyDelete(
                        uploadPathResolver,
                        website.OrgName,
                        file.DownloadFileName);
                }
                purgedIds.Add(file.Id);
                closedIds.Add(file.Id);
                item.recycle.IsDeleted = true;
                item.recycle.DeletionTime = DateTime.Now;
            }

            if (purgedIds.Count > 0)
            {
                var relations = await db.FileBindMores.IgnoreQueryFilters()
                    .Where(item => !item.IsDeleted
                        && item.FK_FileUploadId.HasValue
                        && purgedIds.Contains(item.FK_FileUploadId.Value))
                    .ToListAsync();
                foreach (var relation in relations)
                {
                    relation.IsDeleted = true;
                    relation.DeletionTime = DateTime.Now;
                }
            }
            if (closedIds.Count > 0)
            {
                var bindingEntries = await db.FileRecycleBinBindings
                    .Where(item => !item.IsDeleted
                        && closedIds.Contains(item.FK_FileUploadId))
                    .ToListAsync();
                foreach (var entry in bindingEntries)
                {
                    entry.IsDeleted = true;
                    entry.DeletionTime = DateTime.Now;
                }
            }
            await db.SaveChangesAsync();

            logger.LogInformation(
                "Expired recycle-bin files checked. WebsiteId={WebsiteId}, Count={Count}",
                websiteId,
                expired.Count);
        }

        private async Task<Dictionary<long, HashSet<long>>> GetFileFamilyGraphAsync(long websiteId)
        {
            var uploads = await db.FileUploads.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(item => item.FK_WebsiteId == websiteId)
                .Select(item => new { item.Id, item.GuidKey })
                .ToListAsync();
            var uploadByGuid = uploads.ToDictionary(item => item.GuidKey, item => item.Id);
            var relations = await (
                from relation in db.FileBindMores.IgnoreQueryFilters().AsNoTracking()
                join upload in db.FileUploads.IgnoreQueryFilters().AsNoTracking()
                    on relation.FK_FileUploadId equals (long?)upload.Id
                where !relation.IsDeleted && upload.FK_WebsiteId == websiteId
                select new { relation.FK_FileBindGuid, FileId = upload.Id }
            ).ToListAsync();

            var graph = new Dictionary<long, HashSet<long>>();
            foreach (var relation in relations)
            {
                if (!uploadByGuid.TryGetValue(relation.FK_FileBindGuid, out var parentId))
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

        private async Task HideCleanupCandidatesAsync(
            HashSet<long> fileIds,
            DateTime now,
            long userId)
        {
            var candidates = await db.FileCleanupCandidates
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
            long userId)
        {
            if (files.Count == 0)
                return;
            var fileIds = files.Select(item => item.Id).ToHashSet();
            var entries = await db.FileRecycleBinItems.IgnoreQueryFilters()
                .Where(item => item.FK_WebsiteId == websiteId
                    && fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var file in files)
            {
                var entry = entries.FirstOrDefault(item => item.FK_FileUploadId == file.Id);
                if (entry == null)
                {
                    db.FileRecycleBinItems.Add(new FileRecycleBinItem
                    {
                        FK_WebsiteId = websiteId,
                        FK_FileUploadId = file.Id,
                        RecycledTime = now,
                        OriginalPath = file.DownloadFileName ?? string.Empty,
                        Reason = "由查無引用清單批次移入",
                        CreatorUserId = userId
                    });
                    continue;
                }
                entry.IsDeleted = false;
                entry.DeletionTime = null;
                entry.DeleterUserId = null;
                entry.RecycledTime = now;
                entry.OriginalPath = file.DownloadFileName ?? entry.OriginalPath;
                entry.Reason = "由查無引用清單批次移入";
                entry.LastModificationTime = now;
                entry.LastModifierUserId = userId;
            }
        }

        private async Task RemoveMissingFileRecordsAsync(
            IReadOnlyCollection<FileUpload> missingFiles,
            long websiteId,
            DateTime now,
            long userId)
        {
            if (missingFiles.Count == 0)
                return;
            var missingIds = missingFiles.Select(item => item.Id).ToHashSet();
            var missingGuids = missingFiles.Select(item => item.GuidKey).ToHashSet();
            var relations = await db.FileBindMores.IgnoreQueryFilters()
                .Where(item => !item.IsDeleted
                    && ((item.FK_FileUploadId.HasValue
                            && missingIds.Contains(item.FK_FileUploadId.Value))
                        || missingGuids.Contains(item.FK_FileBindGuid)))
                .ToListAsync();
            foreach (var relation in relations)
            {
                relation.IsDeleted = true;
                relation.DeletionTime = now;
                relation.DeleterUserId = userId;
            }

            var staleRecycleEntries = await db.FileRecycleBinItems
                .Where(item => item.FK_WebsiteId == websiteId
                    && missingIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var entry in staleRecycleEntries)
            {
                entry.IsDeleted = true;
                entry.DeletionTime = now;
                entry.DeleterUserId = userId;
            }

            var staleRecycleBindings = await db.FileRecycleBinBindings
                .Where(item => item.FK_WebsiteId == websiteId
                    && missingIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var binding in staleRecycleBindings)
            {
                binding.IsDeleted = true;
                binding.DeletionTime = now;
                binding.DeleterUserId = userId;
            }
        }

        private async Task<int> CloseRecycleRecordsAsync(
            HashSet<long> fileIds,
            long websiteId,
            DateTime now,
            long userId)
        {
            var entries = await db.FileRecycleBinItems
                .Where(item => item.FK_WebsiteId == websiteId
                    && fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var entry in entries)
            {
                entry.IsDeleted = true;
                entry.DeletionTime = now;
                entry.DeleterUserId = userId;
            }

            var bindings = await db.FileRecycleBinBindings
                .Where(item => item.FK_WebsiteId == websiteId
                    && fileIds.Contains(item.FK_FileUploadId))
                .ToListAsync();
            foreach (var binding in bindings)
            {
                binding.IsDeleted = true;
                binding.DeletionTime = now;
                binding.DeleterUserId = userId;
            }
            return entries.Count;
        }
    }
}
