using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.BackgroundJob;

// 文章資料匯入：AI 整理（第一個任務）→ 等待確認 → 建立文章（確認任務）。
// 同一站同時只能有一批：執行中靠 ActiveKey，等待確認中靠建立時檢查。
public sealed partial class BackgroundTaskService
{
    internal static string ArticleImportKey(long websiteId) => $"article-import:{websiteId}";

    public async Task<BackgroundTaskRecord> CreateArticleImportTaskAsync(long websiteId, long userId)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        var waiting = await db.BackgroundTasks.AsNoTracking().AnyAsync(x =>
            !x.IsDeleted
            && x.FK_WebsiteId == websiteId
            && x.Type == BackgroundTaskTypeEnum.ArticleImport
            && x.Status == BackgroundTaskStatusEnum.AwaitingConfirmation
            && (x.ExpireTime == null || x.ExpireTime > now));
        if (waiting)
            throw new BackgroundTaskConflictException("上一批匯入資料還在等待確認，請先確認匯入或取消後再上傳。");

        var task = new BackgroundTaskRecord
        {
            FK_WebsiteId = websiteId,
            FK_UserId = userId,
            Type = BackgroundTaskTypeEnum.ArticleImport,
            Status = BackgroundTaskStatusEnum.Queued,
            ActiveKey = ArticleImportKey(websiteId),
            Message = "等待伺服器開始整理資料",
            CreationTime = now,
            CreatorUserId = userId,
            ExpireTime = now.AddDays(7)
        };
        db.BackgroundTasks.Add(task);
        await SaveArticleImportTaskAsync(db, task);
        return task;
    }

    // 預覽頁按確認：第一個任務結束（不發通知），另建一個寫入文章的任務。
    public async Task<BackgroundTaskRecord> CreateArticleImportConfirmTaskAsync(long websiteId, long userId, long scanTaskId)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        var scan = await db.BackgroundTasks.FirstOrDefaultAsync(x =>
            x.Id == scanTaskId && x.FK_WebsiteId == websiteId && x.Type == BackgroundTaskTypeEnum.ArticleImport);
        if (scan?.Status != BackgroundTaskStatusEnum.AwaitingConfirmation)
            throw new InvalidOperationException("這批匯入資料目前不在等待確認狀態，請重新整理頁面。");

        scan.Status = BackgroundTaskStatusEnum.Succeeded;
        scan.Message = "已確認，開始建立文章";
        scan.CompletionTime = now;
        scan.LastModificationTime = now;

        var task = new BackgroundTaskRecord
        {
            FK_WebsiteId = websiteId,
            FK_UserId = userId,
            Type = BackgroundTaskTypeEnum.ArticleImport,
            Status = BackgroundTaskStatusEnum.Queued,
            ActiveKey = ArticleImportKey(websiteId),
            Message = "等待伺服器開始建立文章",
            CreationTime = now,
            CreatorUserId = userId,
            ExpireTime = now.AddDays(7)
        };
        db.BackgroundTasks.Add(task);
        await SaveArticleImportTaskAsync(db, task);
        return task;
    }

    // 進度更新：使用者按了取消（CancelRequested）就保留取消狀態，由背景任務在下一組資料前停止。
    public async Task UpdateArticleImportProgressAsync(long taskId, int progress, string message)
    {
        await using var db = CreateDb();
        var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
        if (task.Status == BackgroundTaskStatusEnum.Queued) task.Status = BackgroundTaskStatusEnum.Running;
        task.Progress = Math.Clamp(progress, 0, 99);
        task.Message = task.Status == BackgroundTaskStatusEnum.CancelRequested ? "正在停止匯入" : message;
        task.StartTime ??= DateTime.Now;
        task.LastModificationTime = DateTime.Now;
        await db.SaveChangesAsync();
    }

    public async Task<bool> IsCancelRequestedAsync(long taskId)
    {
        await using var db = CreateDb();
        return await db.BackgroundTasks.AsNoTracking()
            .AnyAsync(x => x.Id == taskId && x.Status == BackgroundTaskStatusEnum.CancelRequested);
    }

    // AI 整理完成，等使用者在預覽頁確認；通知連到預覽頁。
    public async Task SetArticleImportAwaitingAsync(long taskId, long batchId, string message, string resultJson)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
        task.Status = BackgroundTaskStatusEnum.AwaitingConfirmation;
        task.Progress = 100;
        task.Message = message;
        task.ResultJson = resultJson;
        task.ActiveKey = null;
        task.CompletionTime = null;
        task.ExpireTime = now.AddDays(7);
        task.LastModificationTime = now;
        db.Notifications.Add(new UserNotification
        {
            FK_WebsiteId = task.FK_WebsiteId,
            FK_UserId = task.FK_UserId,
            FK_BackgroundTaskId = task.Id,
            Type = NotificationTypeEnum.BackgroundTask,
            Title = "文章資料整理完成，請確認",
            Message = message,
            ActionUrl = $"/ContentManagement/Directory#Import_{batchId}",
            IsRead = false,
            CreationTime = now,
            CreatorUserId = task.FK_UserId
        });
        await db.SaveChangesAsync();
    }

    // 使用者取消：排隊中、等待確認中直接結束（回傳 true）；執行中改成「要求取消」（回傳 false），由背景任務自己停。
    public async Task<bool> CancelArticleImportAsync(long taskId, long websiteId)
    {
        await using var db = CreateDb();
        var task = await db.BackgroundTasks.FirstOrDefaultAsync(x =>
                x.Id == taskId && x.FK_WebsiteId == websiteId && x.Type == BackgroundTaskTypeEnum.ArticleImport)
            ?? throw new InvalidOperationException("找不到匯入任務。");
        switch (task.Status)
        {
            case BackgroundTaskStatusEnum.Queued:
            case BackgroundTaskStatusEnum.AwaitingConfirmation:
                SetCanceled(task, "已取消文章資料匯入，未建立任何文章。");
                await db.SaveChangesAsync();
                return true;
            case BackgroundTaskStatusEnum.Running:
                task.Status = BackgroundTaskStatusEnum.CancelRequested;
                task.Message = "正在停止匯入";
                task.LastModificationTime = DateTime.Now;
                await db.SaveChangesAsync();
                return false;
            case BackgroundTaskStatusEnum.CancelRequested:
                return false;
            default:
                throw new InvalidOperationException("這批匯入已經結束，無法取消。");
        }
    }

    public async Task FinishArticleImportCanceledAsync(long taskId)
    {
        await using var db = CreateDb();
        var task = await db.BackgroundTasks.FirstAsync(x => x.Id == taskId);
        SetCanceled(task, "已取消文章資料匯入，未建立任何文章。");
        await db.SaveChangesAsync();
    }

    private static void SetCanceled(BackgroundTaskRecord task, string message)
    {
        task.Status = BackgroundTaskStatusEnum.Canceled;
        task.Message = message;
        task.ActiveKey = null;
        task.CompletionTime = DateTime.Now;
        task.LastModificationTime = DateTime.Now;
    }

    private async Task SaveArticleImportTaskAsync(Microsoft.EntityFrameworkCore.DbContext db, BackgroundTaskRecord task)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            await using var checkDb = CreateDb();
            if (await checkDb.BackgroundTasks.AnyAsync(x => !x.IsDeleted && x.ActiveKey == task.ActiveKey))
                throw new BackgroundTaskConflictException("目前已有文章資料匯入正在執行，請等待完成後再試。");
            throw;
        }
    }
}