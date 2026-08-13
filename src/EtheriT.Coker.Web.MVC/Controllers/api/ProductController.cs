using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Core.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductAppService productAppService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly LoginUserData loginUserData;
        private readonly BackgroundTaskService backgroundTaskService;
        public ProductController(
            IProductAppService productAppService,
            IBackgroundJobClient backgroundJobClient,
            LoginUserData loginUserData,
            BackgroundTaskService backgroundTaskService
            )
        {
            this.productAppService = productAppService;
            this.backgroundJobClient = backgroundJobClient;
            this.loginUserData = loginUserData;
            this.backgroundTaskService = backgroundTaskService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ProductAddUp(ProdAddUpDto dto)
        {
            return await productAppService.ProductAddUp(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ProdPriceAddUp(List<ProductPriceDto> dto)
        {
            return await productAppService.PriceAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(
            DataSourceLoadOptions loadOptions,
            [FromQuery] string? pids = null,
            string? tagIds = null,
            bool excludeUnavailable = false)
        {
            return await productAppService.GetAllList(loadOptions, pids, tagIds, excludeUnavailable);
        }

        [HttpGet]
        public async Task<IActionResult> GetLastProductImport()
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var lastImport = await backgroundTaskService.GetLatestSuccessfulProductImportAsync(websiteId);
            return Ok(new
            {
                hasImport = lastImport.CompletionTime.HasValue,
                completionTime = lastImport.CompletionTime,
                message = lastImport.Message
            });
        }
        [HttpGet]
        public async Task<JsonResult> SaleQuantityStaging(DataSourceLoadOptions loadOptions)
        {
            return await productAppService.SaleQuantityStaging(loadOptions);
        }
        [HttpGet]
        public async Task<ProdGetDataDto> GetProdDataOne(long Id)
        {
            return await productAppService.GetProdDataOne(Id);
        }
        [HttpGet]
        public async Task<List<ProductStockDto>> GetStockDataAll(long PId)
        {
            return await productAppService.GetStockDataAll(PId);
        }
        [HttpGet]
        public async Task<List<ProductPriceDto>> GetPriceDataAll(long PSId)
        {
            return await productAppService.GetPriceDataAll(PSId);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ProdDelete(long Id)
        {
            return await productAppService.ProdDelete(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> StockDelete(long Id)
        {
            return await productAppService.StockDelete(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PriceDelete(long Id)
        {
            return await productAppService.PriceDelete(Id);
        }
        [HttpPost]
        public async Task<IActionResult> ProdReplace(
            IList<IFormFile> files,
            [FromForm] long templateId,
            [FromForm] bool overwriteExisting = false,
            [FromForm] bool allowDuplicateMenuTitles = false)
        {
            if (files.Count != 1)
                return BadRequest(new { message = "請選擇一個商品 Excel 檔案。" });
            if (templateId <= 0)
                return BadRequest(new { message = "請選擇商品匯入版型。" });

            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            BackgroundTaskRecord? task = null;
            try
            {
                task = await backgroundTaskService.CreateProductTaskAsync(
                    websiteId,
                    userId,
                    BackgroundTaskTypeEnum.ProductImport);
                await using var stream = files[0].OpenReadStream();
                await backgroundTaskService.SaveSourceFileAsync(task.Id, stream, files[0].FileName);
                var jobId = backgroundJobClient.Enqueue<ProductExportBackgroundJob>(
                    job => job.RunImportAnalysis(task.Id, templateId));
                await backgroundTaskService.SetHangfireJobIdAsync(task.Id, jobId);
                return Accepted(new { taskId = task.Id });
            }
            catch (BackgroundTaskConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                if (task != null)
                    await backgroundTaskService.FailAsync(task.Id, ex.Message);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmProductImport(ConfirmProductImportDto dto)
        {
            dto.IgnoredRows ??= new List<ProductImportIgnoredRowDto>();
            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            var task = await backgroundTaskService.GetForUserAsync(dto.TaskId, websiteId, userId);
            if (task == null)
                return NotFound();
            // 使用者連點或瀏覽器重送確認時，沿用已啟動的同一任務。
            if (task.Status is BackgroundTaskStatusEnum.Queued or BackgroundTaskStatusEnum.Running)
                return Accepted(new { taskId = task.Id });
            if (task.Status == BackgroundTaskStatusEnum.Succeeded)
                return Ok(new { taskId = task.Id });
            if (task.Status != BackgroundTaskStatusEnum.AwaitingConfirmation)
                return Conflict(new { message = "此商品匯入任務已不在等待確認狀態。" });

            if (string.IsNullOrWhiteSpace(task.ResultJson))
                return Conflict(new { message = "找不到商品匯入掃描結果，請重新上傳檔案。" });

            long analyzedTemplateId;
            var differenceCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allowedIgnoredRows = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var hasNonIgnorableErrors = false;
            using (var result = System.Text.Json.JsonDocument.Parse(task.ResultJson))
            {
                analyzedTemplateId = result.RootElement.GetProperty("TemplateId").GetInt64();
                if (result.RootElement.TryGetProperty("Errors", out var errors))
                {
                    foreach (var error in errors.EnumerateArray())
                    {
                        var canIgnore = error.TryGetProperty("CanIgnore", out var canIgnoreValue)
                            && canIgnoreValue.GetBoolean();
                        var sheet = error.TryGetProperty("Sheet", out var sheetValue)
                            ? sheetValue.GetString() ?? string.Empty
                            : string.Empty;
                        if (!canIgnore)
                        {
                            hasNonIgnorableErrors = true;
                            continue;
                        }
                        if (!error.TryGetProperty("RowNumbers", out var rowNumbers)) continue;
                        foreach (var rowNumber in rowNumbers.EnumerateArray())
                        {
                            var key = $"{sheet}|{rowNumber.GetInt32()}";
                            allowedIgnoredRows.Add(key);
                        }
                    }
                }
                if (result.RootElement.TryGetProperty("PreviouslyIgnoredRows", out var previouslyIgnoredRows))
                {
                    foreach (var row in previouslyIgnoredRows.EnumerateArray())
                    {
                        var sheet = row.GetProperty("Sheet").GetString() ?? string.Empty;
                        var rowNumber = row.GetProperty("RowNumber").GetInt32();
                        allowedIgnoredRows.Add($"{sheet}|{rowNumber}");
                    }
                }
                if (result.RootElement.TryGetProperty("Differences", out var differences))
                {
                    foreach (var difference in differences.EnumerateArray())
                    {
                        if (difference.TryGetProperty("Code", out var code))
                            differenceCodes.Add(code.GetString() ?? string.Empty);
                    }
                }
            }
            if (dto.TemplateId != analyzedTemplateId)
                return BadRequest(new { message = "匯入版型與掃描時不同，請重新掃描。" });
            if (hasNonIgnorableErrors && dto.IgnoredRows.Count == 0)
                return BadRequest(new { message = "Excel 內仍有不可忽略的結構衝突，請修正後重新掃描。" });

            var requestedIgnoredRows = dto.IgnoredRows
                .Where(e => allowedIgnoredRows.Contains($"{e.Sheet}|{e.RowNumber}"))
                .GroupBy(e => $"{e.Sheet}|{e.RowNumber}", StringComparer.OrdinalIgnoreCase)
                .Select(e => e.First())
                .ToList();
            if (requestedIgnoredRows.Count == 0 && allowedIgnoredRows.Count > 0)
                return BadRequest(new { message = "請先選擇要忽略的 Excel 資料列，或修改檔案後重新掃描。" });

            var sourcePath = await backgroundTaskService.GetSourcePhysicalPathAsync(task.Id);
            var filteredAnalysis = await productAppService.AnalyzeProductImport(
                sourcePath,
                dto.TemplateId,
                requestedIgnoredRows,
                null);
            if (filteredAnalysis.Errors.Count > 0)
            {
                await backgroundTaskService.SetAwaitingConfirmationAsync(
                    task.Id,
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Kind = "analysis",
                        TemplateId = dto.TemplateId,
                        filteredAnalysis.CanImport,
                        filteredAnalysis.Errors,
                        filteredAnalysis.Differences,
                        filteredAnalysis.Summary,
                        PreviouslyIgnoredRows = requestedIgnoredRows
                    }));
                return Conflict(new
                {
                    message = "排除所選資料列後仍有 Excel 衝突，請繼續選擇要忽略的列或修改 Excel。",
                    analysis = filteredAnalysis
                });
            }
            differenceCodes = filteredAnalysis.Differences
                .Select(e => e.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            try
            {
                await backgroundTaskService.QueueConfirmedImportAsync(task.Id);
                var jobId = backgroundJobClient.Enqueue<ProductExportBackgroundJob>(job => job.RunImport(
                    task.Id,
                    dto.TemplateId,
                    dto.OverwriteExistingDirectoryPages
                        && differenceCodes.Contains(ProductImportDifferenceCodes.DirectoryPage),
                    dto.AllowDuplicateMenuTitles
                        && differenceCodes.Contains(ProductImportDifferenceCodes.DuplicateMenuTitle),
                    dto.OverwriteExistingMenuParents
                        && differenceCodes.Contains(ProductImportDifferenceCodes.MenuParent),
                    dto.OverwriteExistingProductNames
                        && differenceCodes.Contains(ProductImportDifferenceCodes.ProductName),
                    dto.OverwriteExistingSpecs
                        && differenceCodes.Contains(ProductImportDifferenceCodes.ProductSpec),
                    dto.OverwriteExistingPrices
                        && differenceCodes.Contains(ProductImportDifferenceCodes.ProductPrice),
                    dto.OverwriteExistingTechnicalCertificates
                        && differenceCodes.Contains(ProductImportDifferenceCodes.TechnicalCertificate),
                    requestedIgnoredRows));
                await backgroundTaskService.SetHangfireJobIdAsync(task.Id, jobId);
                return Accepted(new { taskId = task.Id });
            }
            catch (BackgroundTaskConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // 兩個確認請求可能同時通過前段檢查；若另一個請求已成功
                // 將任務排隊，就回傳同一任務，避免誤標為失敗。
                var latestTask = await backgroundTaskService.GetForUserAsync(
                    dto.TaskId,
                    websiteId,
                    userId);
                if (latestTask?.Status is BackgroundTaskStatusEnum.Queued
                    or BackgroundTaskStatusEnum.Running)
                    return Accepted(new { taskId = dto.TaskId });
                if (latestTask?.Status == BackgroundTaskStatusEnum.Succeeded)
                    return Ok(new { taskId = dto.TaskId });
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                await backgroundTaskService.FailAsync(task.Id, ex.Message);
                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> StartProductExport([FromQuery] string version = "full")
        {
            version = version.Trim().ToLowerInvariant();
            if (version is not ("full" or "price"))
                return BadRequest(new { message = "不支援的商品匯出版本。" });
            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            try
            {
                var task = await backgroundTaskService.CreateProductTaskAsync(
                    websiteId,
                    userId,
                    BackgroundTaskTypeEnum.ProductExport);
                try
                {
                    var jobId = backgroundJobClient.Enqueue<ProductExportBackgroundJob>(
                        job => job.RunExport(task.Id, version));
                    await backgroundTaskService.SetHangfireJobIdAsync(task.Id, jobId);
                    return Accepted(new { taskId = task.Id });
                }
                catch (Exception ex)
                {
                    await backgroundTaskService.FailAsync(task.Id, ex.Message);
                    throw;
                }
            }
            catch (BackgroundTaskConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProductTaskStatus(long taskId)
        {
            var task = await backgroundTaskService.GetForUserAsync(
                taskId,
                await loginUserData.GetWebsiteId(),
                await loginUserData.GetUserId());
            if (task == null)
                return NotFound();

            return Ok(new
            {
                status = task.Status.ToString().ToLowerInvariant(),
                type = task.Type.ToString(),
                task.Progress,
                task.Message,
                task.Error,
                task.ResultJson,
                canDownload = task.Status == BackgroundTaskStatusEnum.Succeeded
                    && !string.IsNullOrWhiteSpace(task.ResultFilePath)
            });
        }
        [HttpGet]
        public async Task<IActionResult> DownloadProductTask(long taskId)
        {
            var task = await backgroundTaskService.GetForUserAsync(
                taskId,
                await loginUserData.GetWebsiteId(),
                await loginUserData.GetUserId());
            if (task == null || task.Status != BackgroundTaskStatusEnum.Succeeded)
                return NotFound();

            var path = await backgroundTaskService.GetResultPhysicalPathAsync(task);
            if (!System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(
                path,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                task.ResultFileName ?? "ProductData.xlsx");
        }
        [HttpPost]
        public async Task<GetProdContenDto> GetConten(SearchIDDto dto)
        {
            return await productAppService.GetConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ImportConten(ProdSaveContenDto dto)
        {
            return await productAppService.ImportConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(ProdSaveContenDto dto)
        {
            return await productAppService.SaveConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> StockBatchSet(List<StockBatchSetDto> dto) {
            return await productAppService.StockBatchSet(dto);
        }
        [HttpGet]
        public async Task<List<TagGetSelectedDto>> GetProductListTags() {
            return await productAppService.GetProductListTags();
        }
    }
}
