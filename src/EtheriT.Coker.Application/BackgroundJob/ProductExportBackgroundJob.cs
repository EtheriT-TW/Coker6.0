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
        private readonly IProductImportAppService productImportAppService;
        private readonly BackgroundTaskService backgroundTaskService;
        private readonly BackgroundOperationContext operationContext;

        public ProductExportBackgroundJob(
            IProductAppService productAppService,
            IProductImportAppService productImportAppService,
            BackgroundTaskService backgroundTaskService,
            BackgroundOperationContext operationContext)
        {
            this.productAppService = productAppService;
            this.productImportAppService = productImportAppService;
            this.backgroundTaskService = backgroundTaskService;
            this.operationContext = operationContext;
        }

        [AutomaticRetry(Attempts = 0)]
        public Task RunExport(long taskId)
            => RunExport(taskId, "full");

        [AutomaticRetry(Attempts = 0)]
        public async Task RunExport(long taskId, string exportVersion)
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
                    exportVersion,
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
                var priceAndStockOnly = string.Equals(exportVersion, "price", StringComparison.OrdinalIgnoreCase);
                var fileName = priceAndStockOnly
                    ? $"ProductPriceData_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    : $"ProductData_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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
        public async Task RunImportAnalysis(long taskId, long templateId)
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

                var sourcePath = await backgroundTaskService.GetSourcePhysicalPathAsync(taskId);
                var lastProgress = -1;
                var analysis = await productImportAppService.AnalyzeProductImport(
                    sourcePath,
                    templateId,
                    new List<ProductImportIgnoredRowDto>(),
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
                    Kind = "analysis",
                    TemplateId = templateId,
                    analysis.CanImport,
                    analysis.Errors,
                    analysis.Differences,
                    analysis.Summary
                });
                await backgroundTaskService.SetAwaitingConfirmationAsync(taskId, resultJson);
            }
            catch (Exception ex)
            {
                await backgroundTaskService.FailAsync(taskId, ex.Message);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 0)]
        public Task RunImport(long taskId, long templateId)
        {
            return RunImport(taskId, templateId, false, false);
        }

        [AutomaticRetry(Attempts = 0)]
        public Task RunImport(long taskId, long templateId, bool overwriteExisting)
        {
            return RunImport(taskId, templateId, overwriteExisting, false);
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunImport(
            long taskId,
            long templateId,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles)
        {
            await RunImport(
                taskId,
                templateId,
                overwriteExisting,
                allowDuplicateMenuTitles,
                true,
                true,
                true,
                true,
                true,
                new List<ProductImportIgnoredRowDto>());
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunImport(
            long taskId,
            long templateId,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles,
            bool overwriteExistingMenuParents,
            bool overwriteExistingProductNames,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices,
            bool overwriteExistingTechnicalCertificates,
            List<ProductImportIgnoredRowDto> ignoredRows)
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
                var result = await productImportAppService.ProdReplace(
                    sourcePath,
                    templateId,
                    overwriteExisting,
                    allowDuplicateMenuTitles,
                    overwriteExistingMenuParents,
                    overwriteExistingProductNames,
                    overwriteExistingSpecs,
                    overwriteExistingPrices,
                    overwriteExistingTechnicalCertificates,
                    ignoredRows,
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
                    Errors = result.ErrorList ?? new List<ImportMassageItem>(),
                    result.Summary
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
                var summary = result.Summary;
                var message = $"商品匯入完成：商品 {summary.ProductCount} 隻（新增 {summary.ProductAddedCount}、更新 {summary.ProductUpdatedCount}），選單 {summary.MenuCount} 個（新增 {summary.MenuAddedCount}）。";
                if (errorCount > 0)
                    message += $"另有 {errorCount} 筆資料需要留意。";
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
