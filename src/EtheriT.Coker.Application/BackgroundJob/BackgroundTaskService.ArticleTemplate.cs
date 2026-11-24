using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.BackgroundJob;

public sealed partial class BackgroundTaskService
{
    // 同一個目錄同時只能有一個套用任務。
    internal static string ArticleTemplateApplyKey(long websiteId, long directoryId)
        => $"article-template:{websiteId}:{directoryId}";

    public async Task<BackgroundTaskRecord> CreateArticleTemplateApplyTaskAsync(long websiteId, long userId, long directoryId)
    {
        await using var db = CreateDb();
        var now = DateTime.Now;
        var task = new BackgroundTaskRecord
        {
            FK_WebsiteId = websiteId,
            FK_UserId = userId,
            Type = BackgroundTaskTypeEnum.ArticleTemplateApply,
            Status = BackgroundTaskStatusEnum.Queued,
            ActiveKey = ArticleTemplateApplyKey(websiteId, directoryId),
            Message = "等待伺服器開始套用文章樣板",
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
                throw new BackgroundTaskConflictException("這個目錄已經有文章樣板套用任務正在執行，請等待完成後再試。");
            throw;
        }
    }
}