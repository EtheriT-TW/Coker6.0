using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.FileManagement;
using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class FileCleanupWorking
    {
        private readonly CokerDbContext db;
        private readonly IFileReferenceScanner referenceScanner;
        private readonly IUploadPathResolver uploadPathResolver;
        private readonly FileCleanupOptions options;
        private readonly ILogger<FileCleanupWorking> logger;

        public FileCleanupWorking(
            CokerDbContext db,
            IFileReferenceScanner referenceScanner,
            IUploadPathResolver uploadPathResolver,
            IOptions<FileCleanupOptions> options,
            ILogger<FileCleanupWorking> logger)
        {
            this.db = db;
            this.referenceScanner = referenceScanner;
            this.uploadPathResolver = uploadPathResolver;
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
                "File reference scan completed. WebsiteId={WebsiteId}, Scanned={Scanned}, Unreferenced={Unreferenced}",
                websiteId,
                uploads.Count,
                unreferencedIds.Count);
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
    }
}
