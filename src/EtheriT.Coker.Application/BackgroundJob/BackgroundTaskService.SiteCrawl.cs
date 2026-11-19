using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.BackgroundJob;

public sealed partial class BackgroundTaskService
{
    internal static string SiteCrawlKey(long websiteId) => $"site-crawl:{websiteId}";

    public async Task<BackgroundTaskRecord> CreateSiteCrawlTaskAsync(long websiteId, long userId)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        var task = new BackgroundTaskRecord
        {
            FK_WebsiteId = websiteId,
            FK_UserId = userId,
            Type = BackgroundTaskTypeEnum.SiteCrawl,
            Status = BackgroundTaskStatusEnum.Queued,
            ActiveKey = SiteCrawlKey(websiteId),
            Message = "等待伺服器開始處理",
            CreationTime = now,
            CreatorUserId = userId,
            ExpireTime = now.AddDays(7)
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
            if (await checkDb.BackgroundTasks.AnyAsync(x => !x.IsDeleted && x.ActiveKey == task.ActiveKey))
                throw new BackgroundTaskConflictException("目前已有網站爬蟲匯入任務正在執行。");
            throw;
        }
    }

    public async Task<BackgroundTaskRecord?> GetLatestSiteCrawlAsync(long websiteId, long userId)
    {
        await using var db = CreateDb();
        return await db.BackgroundTasks.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Type == BackgroundTaskTypeEnum.SiteCrawl
                && x.FK_WebsiteId == websiteId && x.FK_UserId == userId
                && x.ExpireTime > DateTime.Now)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    internal async Task<bool> StartSiteCrawlAsync(BackgroundTaskRecord task)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        return await SiteCrawlQuery(db, task)
            .Where(x => x.Status == BackgroundTaskStatusEnum.Queued || x.Status == BackgroundTaskStatusEnum.Running)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Status, BackgroundTaskStatusEnum.Running)
                .SetProperty(x => x.Progress, 10)
                .SetProperty(x => x.Message, "正在準備匯入")
                .SetProperty(x => x.StartTime, x => x.StartTime ?? now)
                .SetProperty(x => x.LastModificationTime, now)) == 1;
    }

    internal async Task<bool> UpdateSiteCrawlProgressAsync(BackgroundTaskRecord task, int progress, string message)
    {
        await using var db = CreateDb();
        return await SiteCrawlQuery(db, task).Where(x => x.Status == BackgroundTaskStatusEnum.Running)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Progress, progress)
                .SetProperty(x => x.Message, message)
                .SetProperty(x => x.LastModificationTime, DateTime.Now)) == 1;
    }

    public async Task RequestSiteCrawlCancellationAsync(BackgroundTaskRecord task)
    {
        if (await FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.Queued,
            BackgroundTaskStatusEnum.Canceled, "網站匯入已取消", SiteCrawlBackgroundJob.EmptyResult))
            return;

        await using var db = CreateDb();
        await SiteCrawlQuery(db, task).Where(x => x.Status == BackgroundTaskStatusEnum.Running)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Status, BackgroundTaskStatusEnum.CancelRequested)
                .SetProperty(x => x.Message, "正在停止網站匯入")
                .SetProperty(x => x.LastModificationTime, DateTime.Now));
    }

    public async Task<bool> FinishSiteCrawlAsync(BackgroundTaskRecord task, BackgroundTaskStatusEnum expected,
        BackgroundTaskStatusEnum status, string message, string resultJson, string? error = null)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        return await SiteCrawlQuery(db, task).Where(x => x.Status == expected)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Status, status)
                .SetProperty(x => x.Progress, x => status == BackgroundTaskStatusEnum.Succeeded ? 100 : x.Progress)
                .SetProperty(x => x.Message, message)
                .SetProperty(x => x.Error, error)
                .SetProperty(x => x.ResultJson, resultJson)
                .SetProperty(x => x.ActiveKey, (string?)null)
                .SetProperty(x => x.CompletionTime, now)
                .SetProperty(x => x.LastModificationTime, now)) == 1;
    }

    private static Task<int> CloseStaleSiteCrawlAsync(
        CokerDbContext db, BackgroundTaskRecord task, DateTime staleBefore, DateTime now)
    {
        var canceled = task.Status == BackgroundTaskStatusEnum.CancelRequested;
        return SiteCrawlQuery(db, task)
            .Where(x => x.Status == task.Status && (x.LastModificationTime ?? x.CreationTime) <= staleBefore)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Status, canceled ? BackgroundTaskStatusEnum.Canceled : BackgroundTaskStatusEnum.Failed)
                .SetProperty(x => x.Message, canceled ? "網站匯入已取消" : "網站匯入任務逾時")
                .SetProperty(x => x.ActiveKey, (string?)null)
                .SetProperty(x => x.ResultJson, x => x.ResultJson ?? SiteCrawlBackgroundJob.EmptyResult)
                .SetProperty(x => x.CompletionTime, now)
                .SetProperty(x => x.LastModificationTime, now));
    }

    private static IQueryable<BackgroundTaskRecord> SiteCrawlQuery(CokerDbContext db, BackgroundTaskRecord task)
        => db.BackgroundTasks.Where(x => x.Id == task.Id && x.Type == BackgroundTaskTypeEnum.SiteCrawl
            && x.FK_WebsiteId == task.FK_WebsiteId && x.FK_UserId == task.FK_UserId
            && !x.IsDeleted && x.ActiveKey != null);
}
