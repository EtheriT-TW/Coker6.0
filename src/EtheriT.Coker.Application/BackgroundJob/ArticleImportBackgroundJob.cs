using EtheriT.Coker.Application.ArticleImports;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.BackgroundJob;

// 文章資料匯入的兩段背景任務：Scan＝下載圖片＋AI 填欄位；Confirm＝建立／更新文章。
public sealed class ArticleImportBackgroundJob(
    BackgroundTaskService tasks,
    BackgroundOperationContext operationContext,
    CokerDbContext db,
    ArticleImportProcessor processor,
    ArticleImportWriter writer,
    ArticleImportFileCleaner cleaner)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task Scan(long taskId, long batchId)
    {
        var task = await tasks.GetAsync(taskId);
        if (task is null || task.Type != BackgroundTaskTypeEnum.ArticleImport || task.Status != BackgroundTaskStatusEnum.Queued)
            return;

        // AI 服務與上傳服務都依這裡設定的站台與使用者運作。
        operationContext.Set(task.FK_WebsiteId, task.FK_UserId);
        try
        {
            await tasks.UpdateArticleImportProgressAsync(taskId, 5, "正在讀取匯入資料");
            var result = await processor.ProcessAsync(
                task.FK_WebsiteId,
                task.FK_UserId,
                batchId,
                (progress, message) => tasks.UpdateArticleImportProgressAsync(taskId, progress, message),
                () => tasks.IsCancelRequestedAsync(taskId),
                CancellationToken.None);

            var message = $"已整理 {result.ReadyCount} 筆";
            if (result.FailedCount > 0) message += $"，{result.FailedCount} 筆失敗";
            await tasks.SetArticleImportAwaitingAsync(taskId, batchId, message + "，請到預覽頁確認後匯入。", JsonConvert.SerializeObject(result));
        }
        catch (OperationCanceledException)
        {
            await CleanupAsync(batchId);
            await tasks.FinishArticleImportCanceledAsync(taskId);
        }
        catch (Exception ex)
        {
            await CleanupAsync(batchId);
            await tasks.FailAsync(taskId, ex.Message);
        }
    }

    [AutomaticRetry(Attempts = 0)]
    public async Task Confirm(long taskId, long batchId)
    {
        var task = await tasks.GetAsync(taskId);
        if (task is null || task.Type != BackgroundTaskTypeEnum.ArticleImport || task.Status != BackgroundTaskStatusEnum.Queued)
            return;

        operationContext.Set(task.FK_WebsiteId, task.FK_UserId);
        try
        {
            await tasks.UpdateArticleImportProgressAsync(taskId, 5, "正在建立文章");
            var result = await writer.WriteAsync(
                batchId,
                (progress, message) => tasks.UpdateArticleImportProgressAsync(taskId, progress, message));

            var message = $"新增 {result.CreatedCount} 篇、更新 {result.UpdatedCount} 篇";
            if (result.ExcludedCount > 0) message += $"，取消 {result.ExcludedCount} 筆";
            if (result.Failures.Count > 0) message += $"，{result.Failures.Count} 筆失敗";
            await tasks.CompleteAsync(taskId, message + "。", resultJson: JsonConvert.SerializeObject(result));
        }
        catch (Exception ex)
        {
            await tasks.FailAsync(taskId, ex.Message);
        }
    }

    // 失敗或取消：這批下載的圖片都不會用到，移到資源回收桶。清理失敗不影響任務結果。
    private async Task CleanupAsync(long batchId)
    {
        try
        {
            db.ChangeTracker.Clear();
            await cleaner.RecycleUnusedAsync(batchId);
        }
        catch (Exception)
        {
            // 留在上傳目錄的檔案之後可用檔案清理處理。
        }
    }
}