using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Hangfire;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.BackgroundJob;

// 把目錄目前的文章樣板套用到目錄內所有有欄位資料的文章（草稿＋發布一起改）。
public sealed class ArticleTemplateApplyBackgroundJob(
    BackgroundTaskService tasks,
    BackgroundOperationContext operationContext,
    ArticleTemplateAppService articleTemplates)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(long taskId, long directoryId)
    {
        var task = await tasks.GetAsync(taskId);
        if (task is null
            || task.Type != BackgroundTaskTypeEnum.ArticleTemplateApply
            || task.Status != BackgroundTaskStatusEnum.Queued)
            return;

        operationContext.Set(task.FK_WebsiteId, task.FK_UserId);
        try
        {
            await tasks.UpdateProgressAsync(taskId, 5, "正在讀取目錄文章");
            var result = await articleTemplates.ApplyToDirectoryAsync(
                directoryId,
                (progress, message) => tasks.UpdateProgressAsync(taskId, progress, message));

            var message = $"已套用 {result.AppliedCount} 篇";
            if (result.NoDataCount > 0) message += $"，略過 {result.NoDataCount} 篇非匯入文章";
            if (result.NoPermissionCount > 0) message += $"，略過 {result.NoPermissionCount} 篇無編輯權限";
            if (result.Failures.Count > 0) message += $"，{result.Failures.Count} 篇失敗";
            await tasks.CompleteAsync(taskId, message + "。", resultJson: JsonConvert.SerializeObject(result));
        }
        catch (Exception ex)
        {
            await tasks.FailAsync(taskId, ex.Message);
        }
    }
}