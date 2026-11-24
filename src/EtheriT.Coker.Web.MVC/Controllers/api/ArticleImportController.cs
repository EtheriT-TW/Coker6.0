using EtheriT.Coker.Application.ArticleImports;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Shared.Dto.ArticleImport;
using Hangfire;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

// 目錄管理「匯入資料」：選目錄、讀 Excel 欄位、開始匯入、預覽與修改、確認或取消。
[ApiController, Authorize, Route("api/[controller]/[action]")]
public sealed class ArticleImportController(
    ArticleImportAppService imports,
    BackgroundTaskService tasks,
    IBackgroundJobClient jobs,
    IAntiforgery antiforgery,
    ILogger<ArticleImportController> logger) : ControllerBase
{
    // 比 Excel 上限（20 MB）多留表單欄位的空間。
    private const long MaxRequestBytes = ArticleImportExcelReader.MaxFileBytes + 1024 * 1024;

    // 送出 POST 前先向這裡取一組新的防偽權杖（比照 SiteCrawl），不依賴頁面載入時的欄位值。
    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult RequestToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName });
    }

    [HttpGet]
    public Task<IActionResult> GetDirectories() => Run(async () => Ok(await imports.GetDirectoriesAsync()));

    [HttpPost, RequestSizeLimit(MaxRequestBytes)]
    public Task<IActionResult> ReadColumns(IFormFile? file) => RunPost(async () =>
    {
        if (CheckFile(file) is { } error) return error;
        await using var stream = file!.OpenReadStream();
        return Ok(imports.ReadColumns(stream, file.FileName));
    });

    [HttpPost, RequestSizeLimit(MaxRequestBytes)]
    public Task<IActionResult> Start([FromForm] long directoryId, [FromForm] List<string>? columns, IFormFile? file) => RunPost(async () =>
    {
        if (CheckFile(file) is { } error) return error;
        ArticleImportStartResultDto result;
        await using (var stream = file!.OpenReadStream())
            result = await imports.StartAsync(directoryId, stream, file.FileName, columns);
        if (await Schedule(result.TaskId, client => client.Enqueue<ArticleImportBackgroundJob>(job => job.Scan(result.TaskId, result.BatchId))) is { } failed)
            return failed;
        return Accepted(result);
    });

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<IActionResult> GetCurrent() => Run(async () =>
        await imports.GetCurrentBatchIdAsync() is { } batchId ? Ok(new { batchId }) : NoContent());

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<IActionResult> GetBatch(long batchId) => Run(async () => Ok(await imports.GetBatchAsync(batchId)));

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<IActionResult> GetRow(long rowId) => Run(async () => Ok(await imports.GetRowAsync(rowId)));

    [HttpPost]
    public Task<IActionResult> UpdateRow(ArticleImportRowUpdateDto input) => RunPost(async () => Ok(await imports.UpdateRowAsync(input)));

    [HttpPost]
    public Task<IActionResult> Confirm(ArticleImportConfirmDto input) => RunPost(async () =>
    {
        var (batchId, taskId) = await imports.ConfirmAsync(input);
        if (await Schedule(taskId, client => client.Enqueue<ArticleImportBackgroundJob>(job => job.Confirm(taskId, batchId))) is { } failed)
            return failed;
        return Accepted(new { batchId, taskId });
    });

    [HttpPost]
    public Task<IActionResult> Cancel(ArticleImportBatchInputDto input) => RunPost(async () =>
    {
        var finished = await imports.CancelAsync(input.BatchId);
        return Ok(new { finished });
    });

    private async Task<IActionResult?> Schedule(long taskId, Func<IBackgroundJobClient, string> enqueue)
    {
        try
        {
            var jobId = enqueue(jobs);
            await tasks.SetHangfireJobIdAsync(taskId, jobId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not enqueue article import task {TaskId}", taskId);
            await tasks.FailAsync(taskId, "無法排程背景任務，請稍後再試。");
            return StatusCode(503, new { message = "無法排程背景任務，請稍後再試。" });
        }
    }

    private BadRequestObjectResult? CheckFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "請選擇要匯入的 Excel 檔案。" });
        if (file.Length > ArticleImportExcelReader.MaxFileBytes)
            return BadRequest(new { message = $"檔案不可超過 {ArticleImportExcelReader.MaxFileBytes / 1024 / 1024} MB。" });
        return null;
    }

    private async Task<IActionResult> RunPost(Func<Task<IActionResult>> action)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException ex)
        {
            logger.LogWarning("Article import antiforgery validation failed on {Path}: {Reason}", Request.Path, ex.Message);
            return BadRequest(new { message = "頁面已過期，請重新整理後再試。" });
        }
        return await Run(action);
    }

    private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (BackgroundTaskConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}