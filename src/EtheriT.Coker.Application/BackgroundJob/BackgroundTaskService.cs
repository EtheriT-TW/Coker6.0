using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class BackgroundTaskConflictException : Exception
    {
        public BackgroundTaskConflictException(string message) : base(message) { }
    }

    public sealed class BackgroundTaskService
    {
        private readonly DbContextOptions<CokerDbContext> dbOptions;
        private readonly IUploadPathResolver uploadPathResolver;

        public BackgroundTaskService(
            DbContextOptions<CokerDbContext> dbOptions,
            IUploadPathResolver uploadPathResolver)
        {
            this.dbOptions = dbOptions;
            this.uploadPathResolver = uploadPathResolver;
        }

        public async Task<BackgroundTaskRecord> CreateProductTaskAsync(
            long websiteId,
            long userId,
            BackgroundTaskTypeEnum type)
        {
            await using var db = CreateDb();
            var task = new BackgroundTaskRecord
            {
                FK_WebsiteId = websiteId,
                FK_UserId = userId,
                Type = type,
                Status = BackgroundTaskStatusEnum.Queued,
                Progress = 0,
                ActiveKey = $"product-data:{websiteId}",
                Message = "等待伺服器開始處理",
                CreationTime = DateTime.Now,
                CreatorUserId = userId,
                ExpireTime = DateTime.Now.AddDays(7)
            };

            db.BackgroundTasks.Add(task);
            try
            {
                await db.SaveChangesAsync();
                return task;
            }
            catch (DbUpdateException)
            {
                await using var checkDb = CreateDb();
                var hasActiveTask = await checkDb.BackgroundTasks
                    .AsNoTracking()
                    .AnyAsync(x => x.ActiveKey == task.ActiveKey);
                if (hasActiveTask)
                    throw new BackgroundTaskConflictException("目前已有商品匯入或匯出任務正在執行，請等待完成後再試。");
                throw;
            }
        }

        public async Task<(DateTime? CompletionTime, string? Message)> GetLatestSuccessfulProductImportAsync(long websiteId)
        {
            await using var db = CreateDb();
            var auditLog = await db.AuditLogs
                .AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId
                    && e.ServiceName == "ProductImport"
                    && e.MethodName == "Completed")
                .OrderByDescending(e => e.ExecutionTime)
                .FirstOrDefaultAsync();
            if (auditLog != null)
                return (auditLog.ExecutionTime, auditLog.ReturnValue);

            var task = await db.BackgroundTasks
                .AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId
                    && e.Type == BackgroundTaskTypeEnum.ProductImport
                    && e.Status == BackgroundTaskStatusEnum.Succeeded
                    && e.CompletionTime.HasValue)
                .OrderByDescending(e => e.CompletionTime)
                .FirstOrDefaultAsync();
            return (task?.CompletionTime, task?.Message);
        }

        public async Task SetHangfireJobIdAsync(long taskId, string jobId)
        {
            await using var db = CreateDb();
            var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            task.HangfireJobId = jobId;
            task.LastModificationTime = DateTime.Now;
            await db.SaveChangesAsync();
        }

        public async Task SetAwaitingConfirmationAsync(long taskId, string resultJson)
        {
            await using var db = CreateDb();
            var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            task.Status = BackgroundTaskStatusEnum.AwaitingConfirmation;
            task.Progress = 100;
            task.Message = "掃描完成，請確認匯入差異。";
            task.ResultJson = resultJson;
            task.ActiveKey = null;
            task.CompletionTime = null;
            task.LastModificationTime = DateTime.Now;
            await db.SaveChangesAsync();
        }

        public async Task QueueConfirmedImportAsync(long taskId)
        {
            await using var db = CreateDb();
            var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            if (task.Status != BackgroundTaskStatusEnum.AwaitingConfirmation)
                throw new InvalidOperationException("此商品匯入任務目前不在等待確認狀態。");

            task.Status = BackgroundTaskStatusEnum.Queued;
            task.Progress = 0;
            task.Message = "已確認，等待正式匯入。";
            task.Error = null;
            task.ResultJson = null;
            task.ActiveKey = $"product-data:{task.FK_WebsiteId}";
            task.LastModificationTime = DateTime.Now;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BackgroundTaskConflictException("目前有其他商品匯入或匯出任務正在處理，請稍後再確認。");
            }
        }

        public async Task<BackgroundTaskRecord?> GetAsync(long taskId)
        {
            await using var db = CreateDb();
            return await db.BackgroundTasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == taskId);
        }

        public async Task<BackgroundTaskRecord?> GetForUserAsync(
            long taskId,
            long websiteId,
            long userId)
        {
            await using var db = CreateDb();
            return await db.BackgroundTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == taskId
                    && x.FK_WebsiteId == websiteId
                    && x.FK_UserId == userId);
        }

        public async Task UpdateProgressAsync(long taskId, int progress, string message)
        {
            await using var db = CreateDb();
            var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            task.Status = BackgroundTaskStatusEnum.Running;
            task.Progress = Math.Clamp(progress, 0, 99);
            task.Message = message;
            task.StartTime ??= DateTime.Now;
            task.LastModificationTime = DateTime.Now;
            await db.SaveChangesAsync();
        }

        public async Task<string> SaveSourceFileAsync(
            long taskId,
            Stream stream,
            string originalFileName)
        {
            var task = await GetRequiredAsync(taskId);
            var extension = Path.GetExtension(originalFileName);
            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("商品匯入檔案必須為 .xlsx 格式。");

            var relativePath = Path.Combine("BackgroundTasks", task.StorageKey.ToString("N"), "source.xlsx");
            var physicalPath = await GetPhysicalPathAsync(task.FK_WebsiteId, relativePath);
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            await using (var output = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(output);
            }

            await using var db = CreateDb();
            var tracked = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            tracked.SourceFilePath = NormalizeRelativePath(relativePath);
            tracked.LastModificationTime = DateTime.Now;
            await db.SaveChangesAsync();
            return physicalPath;
        }

        public async Task<(string PhysicalPath, string RelativePath)> PrepareResultFileAsync(
            long taskId,
            string fileName)
        {
            var task = await GetRequiredAsync(taskId);
            var safeFileName = Path.GetFileName(fileName);
            var relativePath = Path.Combine("BackgroundTasks", task.StorageKey.ToString("N"), safeFileName);
            var physicalPath = await GetPhysicalPathAsync(task.FK_WebsiteId, relativePath);
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            return (physicalPath, NormalizeRelativePath(relativePath));
        }

        public async Task<string> GetSourcePhysicalPathAsync(long taskId)
        {
            var task = await GetRequiredAsync(taskId);
            if (string.IsNullOrWhiteSpace(task.SourceFilePath))
                throw new FileNotFoundException("找不到商品匯入來源檔案。");
            return await GetPhysicalPathAsync(task.FK_WebsiteId, task.SourceFilePath);
        }

        public async Task<string> GetResultPhysicalPathAsync(BackgroundTaskRecord task)
        {
            if (string.IsNullOrWhiteSpace(task.ResultFilePath))
                throw new FileNotFoundException("此任務沒有可下載的檔案。");
            return await GetPhysicalPathAsync(task.FK_WebsiteId, task.ResultFilePath);
        }

        public async Task CompleteAsync(
            long taskId,
            string message,
            string? resultFilePath = null,
            string? resultFileName = null,
            string? resultJson = null)
        {
            await using var db = CreateDb();
            var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            task.Status = BackgroundTaskStatusEnum.Succeeded;
            task.Progress = 100;
            task.Message = message;
            task.ResultFilePath = resultFilePath;
            task.ResultFileName = resultFileName;
            task.ResultJson = resultJson;
            task.ActiveKey = null;
            task.CompletionTime = DateTime.Now;
            task.LastModificationTime = DateTime.Now;

            if (task.Type == BackgroundTaskTypeEnum.ProductImport)
            {
                db.AuditLogs.Add(new Core.Models.AuditLog
                {
                    FK_WebsiteId = task.FK_WebsiteId,
                    UserId = task.FK_UserId,
                    ServiceName = "ProductImport",
                    MethodName = "Completed",
                    ExecutionTime = task.CompletionTime.Value,
                    Parameters = $"BackgroundTaskId={task.Id}",
                    ReturnValue = message
                });
            }

            db.Notifications.Add(new UserNotification
            {
                FK_WebsiteId = task.FK_WebsiteId,
                FK_UserId = task.FK_UserId,
                FK_BackgroundTaskId = task.Id,
                Type = NotificationTypeEnum.BackgroundTask,
                Title = task.Type == BackgroundTaskTypeEnum.ProductImport ? "商品匯入成功" : "商品匯出完成",
                Message = message,
                ActionUrl = task.Type == BackgroundTaskTypeEnum.ProductExport
                    ? $"/api/Product/DownloadProductTask?taskId={task.Id}"
                    : null,
                IsRead = false,
                CreationTime = DateTime.Now,
                CreatorUserId = task.FK_UserId
            });
            await db.SaveChangesAsync();
        }

        public async Task FailAsync(long taskId, string error, string? resultJson = null)
        {
            await using var db = CreateDb();
            var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
            task.Status = BackgroundTaskStatusEnum.Failed;
            task.Progress = 100;
            task.Message = task.Type == BackgroundTaskTypeEnum.ProductImport ? "商品匯入失敗" : "商品匯出失敗";
            task.Error = error.Length > 4000 ? error[..4000] : error;
            task.ResultJson = resultJson;
            task.ActiveKey = null;
            task.CompletionTime = DateTime.Now;
            task.LastModificationTime = DateTime.Now;

            db.Notifications.Add(new UserNotification
            {
                FK_WebsiteId = task.FK_WebsiteId,
                FK_UserId = task.FK_UserId,
                FK_BackgroundTaskId = task.Id,
                Type = NotificationTypeEnum.BackgroundTask,
                Title = task.Type == BackgroundTaskTypeEnum.ProductImport ? "商品匯入失敗" : "商品匯出失敗",
                Message = task.Error,
                IsRead = false,
                CreationTime = DateTime.Now,
                CreatorUserId = task.FK_UserId
            });
            await db.SaveChangesAsync();
        }

        public async Task CleanupExpiredAsync()
        {
            await using var db = CreateDb();
            var now = DateTime.Now;
            var staleBefore = now.AddHours(-6);
            var staleTasks = await db.BackgroundTasks
                .Where(x => x.ActiveKey != null)
                .Where(x => x.Status == BackgroundTaskStatusEnum.Queued || x.Status == BackgroundTaskStatusEnum.Running)
                .Where(x => (x.LastModificationTime ?? x.CreationTime) <= staleBefore)
                .ToListAsync();
            foreach (var staleTask in staleTasks)
            {
                staleTask.Status = BackgroundTaskStatusEnum.Failed;
                staleTask.Progress = 100;
                staleTask.Message = "背景任務逾時，系統已解除鎖定";
                staleTask.Error = "背景任務超過六小時未更新，已由定時清理程序結束。";
                staleTask.ActiveKey = null;
                staleTask.CompletionTime = now;
                staleTask.LastModificationTime = now;
                db.Notifications.Add(new UserNotification
                {
                    FK_WebsiteId = staleTask.FK_WebsiteId,
                    FK_UserId = staleTask.FK_UserId,
                    FK_BackgroundTaskId = staleTask.Id,
                    Type = NotificationTypeEnum.BackgroundTask,
                    Title = "背景任務逾時",
                    Message = staleTask.Error,
                    IsRead = false,
                    CreationTime = now,
                    CreatorUserId = staleTask.FK_UserId
                });
            }

            var tasks = await db.BackgroundTasks
                .Where(x => x.ExpireTime != null && x.ExpireTime <= now)
                .Where(x => x.Status == BackgroundTaskStatusEnum.Succeeded
                    || x.Status == BackgroundTaskStatusEnum.Failed
                    || x.Status == BackgroundTaskStatusEnum.AwaitingConfirmation)
                .ToListAsync();
            if (tasks.Count == 0)
            {
                if (staleTasks.Count > 0)
                    await db.SaveChangesAsync();
                return;
            }

            var websiteIds = tasks.Select(x => x.FK_WebsiteId).Distinct().ToList();
            var orgNames = await db.Websites
                .Where(x => websiteIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.OrgName);

            foreach (var task in tasks)
            {
                if (orgNames.TryGetValue(task.FK_WebsiteId, out var orgName))
                {
                    try
                    {
                        var directory = uploadPathResolver.GetDirectoryPath(
                            orgName,
                            Path.Combine("BackgroundTasks", task.StorageKey.ToString("N")));
                        if (System.IO.Directory.Exists(directory))
                            System.IO.Directory.Delete(directory, true);
                    }
                    catch
                    {
                        // 單一檔案清理失敗不影響其他任務。
                    }
                }

                task.Status = BackgroundTaskStatusEnum.Expired;
                task.SourceFilePath = null;
                task.ResultFilePath = null;
                task.ResultFileName = null;
                task.Message = "任務檔案已過期並清除";
                task.LastModificationTime = DateTime.Now;
            }
            await db.SaveChangesAsync();
        }

        private async Task<BackgroundTaskRecord> GetRequiredAsync(long taskId)
        {
            return await GetAsync(taskId) ?? throw new InvalidOperationException("找不到背景任務。");
        }

        private async Task<string> GetPhysicalPathAsync(long websiteId, string relativePath)
        {
            await using var db = CreateDb();
            var orgName = await db.Websites
                .Where(x => x.Id == websiteId)
                .Select(x => x.OrgName)
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(orgName))
                throw new InvalidOperationException("找不到背景任務所屬網站。");
            return uploadPathResolver.GetPhysicalPath(orgName, relativePath);
        }

        private CokerDbContext CreateDb() => new(dbOptions);

        private static string NormalizeRelativePath(string path) => path.Replace('\\', '/');
    }
}
