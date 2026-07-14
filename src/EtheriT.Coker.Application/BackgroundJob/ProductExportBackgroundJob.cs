using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Product;
using Hangfire;
using Hangfire.Storage;
using System.Text.Json;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class ProductExportBackgroundJob
    {
        private readonly IProductAppService productAppService;
        private readonly BackgroundTaskService backgroundTaskService;
        private readonly BackgroundOperationContext operationContext;

        public ProductExportBackgroundJob(
            IProductAppService productAppService,
            BackgroundTaskService backgroundTaskService,
            BackgroundOperationContext operationContext)
        {
            this.productAppService = productAppService;
            this.backgroundTaskService = backgroundTaskService;
            this.operationContext = operationContext;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunExport(long taskId)
        {
            var task = await backgroundTaskService.GetAsync(taskId)
                ?? throw new InvalidOperationException("找不到商品匯出任務。");
            operationContext.Set(task.FK_WebsiteId, task.FK_UserId);

            try
            {
                using var connection = JobStorage.Current.GetConnection();
                using var operationLock = connection.AcquireDistributedLock(
                    $"product-data:{task.FK_WebsiteId}",
                    TimeSpan.FromSeconds(5));

                await backgroundTaskService.UpdateProgressAsync(taskId, 2, "伺服器已開始製作商品匯出檔");
                var lastProgress = -1;
                var content = await productAppService.ExportProductData(
                    task.FK_WebsiteId,
                    (progress, message) =>
                    {
                        if (progress == lastProgress) return;
                        lastProgress = progress;
                        backgroundTaskService
                            .UpdateProgressAsync(taskId, progress, message)
                            .GetAwaiter()
                            .GetResult();
                    });

                await backgroundTaskService.UpdateProgressAsync(taskId, 99, "正在儲存 Excel 檔案");
                var fileName = $"ProductData_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var file = await backgroundTaskService.PrepareResultFileAsync(taskId, fileName);
                var temporaryPath = $"{file.PhysicalPath}.{Guid.NewGuid():N}.tmp";
                await File.WriteAllBytesAsync(temporaryPath, content);
                File.Move(temporaryPath, file.PhysicalPath, true);

                await backgroundTaskService.CompleteAsync(
                    taskId,
                    "商品匯出完成，可下載 Excel 檔案。",
                    file.RelativePath,
                    fileName);
            }
            catch (Exception ex)
            {
                await backgroundTaskService.FailAsync(taskId, ex.Message);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunImport(long taskId)
        {
            var task = await backgroundTaskService.GetAsync(taskId)
                ?? throw new InvalidOperationException("找不到商品匯入任務。");
            operationContext.Set(task.FK_WebsiteId, task.FK_UserId);

            try
            {
                using var connection = JobStorage.Current.GetConnection();
                using var operationLock = connection.AcquireDistributedLock(
                    $"product-data:{task.FK_WebsiteId}",
                    TimeSpan.FromSeconds(5));

                await backgroundTaskService.UpdateProgressAsync(taskId, 2, "伺服器已開始匯入商品");
                var sourcePath = await backgroundTaskService.GetSourcePhysicalPathAsync(taskId);
                var lastProgress = -1;
                var result = await productAppService.ProdReplace(
                    sourcePath,
                    (progress, message) =>
                    {
                        if (progress == lastProgress) return;
                        lastProgress = progress;
                        backgroundTaskService
                            .UpdateProgressAsync(taskId, progress, message)
                            .GetAwaiter()
                            .GetResult();
                    });

                var resultJson = JsonSerializer.Serialize(new
                {
                    result.Success,
                    Errors = result.ErrorList ?? new List<ImportMassageItem>()
                });
                if (!result.Success)
                {
                    var error = result.ErrorList?.FirstOrDefault()?.Description
                        ?? result.Error
                        ?? "商品匯入失敗。";
                    await backgroundTaskService.FailAsync(taskId, error, resultJson);
                    return;
                }

                var errorCount = result.ErrorList?.Count ?? 0;
                var message = errorCount == 0
                    ? "商品匯入成功。"
                    : $"商品匯入完成，共有 {errorCount} 筆資料需要留意。";
                await backgroundTaskService.CompleteAsync(taskId, message, resultJson: resultJson);
            }
            catch (Exception ex)
            {
                await backgroundTaskService.FailAsync(taskId, ex.Message);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 1)]
        public Task CleanupExpiredFiles()
        {
            return backgroundTaskService.CleanupExpiredAsync();
        }
    }
}
