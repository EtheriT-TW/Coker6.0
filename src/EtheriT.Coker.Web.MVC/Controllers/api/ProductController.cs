using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
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
            [FromForm] bool overwriteExisting = false)
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
                    job => job.RunImport(task.Id, templateId, overwriteExisting));
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
        public async Task<IActionResult> StartProductExport()
        {
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
                        job => job.RunExport(task.Id));
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
