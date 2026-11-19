using EtheriT.Coker.Application;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.SiteCrawl;
using EtheriT.Coker.Core.Models;
using Hangfire;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

[ApiController, Authorize, Route("api/[controller]/[action]")]
public sealed class SiteCrawlController(LoginUserData user, BackgroundTaskService tasks,
    IBackgroundJobClient jobs, IAntiforgery antiforgery, ILogger<SiteCrawlController> logger) : ControllerBase
{
    private const int MaxProductLists = 2000;
    private const int MaxListKeyLength = 2100;
    private const int MaxMenus = 2000;
    private const int MaxMenuKeyLength = 2000;
    // 偵測任務一建立就標上 mode，畫面在排隊中就知道是第 2 步（偵測）而不是第 4 步（匯入）。
    private const string DetectTaskTag = """{"mode":"detect"}""";

    // productLists：確認畫面標成商品的列表（偵測結果的 key）；menus：舊站主選單的勾選與路徑名稱。
    [HttpPost]
    public async Task<IActionResult> Start(long websiteId, StartRequest input)
    {
        if (!await CanAccessAsync(websiteId)) return Forbid();
        if (!await ValidateRequestTokenAsync()) return InvalidRequestToken();
        if (ParseSource(input.Url) is not { } url)
            return BadRequest(new { message = "請輸入有效的 HTTP 或 HTTPS 網址。" });
        var productLists = (input.ProductLists ?? []).Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim()).Distinct(StringComparer.Ordinal).ToArray();
        if (productLists.Length > MaxProductLists || productLists.Any(key => key.Length > MaxListKeyLength))
            return BadRequest(new { message = "商品列表資料不正確，請重新偵測。" });
        if (ParseMenus(input.Menus, out var menus) is { } menuError)
            return BadRequest(new { message = menuError });
        return await CreateAndScheduleAsync(websiteId,
            task => jobs.Enqueue<SiteCrawlBackgroundJob>(job => job.Run(task, url, productLists, menus, CancellationToken.None)));
    }

    private static string? ParseMenus(SiteMenuChoice[]? input, out SiteMenuChoice[] menus)
    {
        menus = (input ?? []).Where(menu => menu is { Key.Length: > 0 })
            .DistinctBy(menu => menu.Key, StringComparer.Ordinal).ToArray();
        if (menus.Length > MaxMenus || menus.Any(menu => menu.Key.Length > MaxMenuKeyLength))
            return "選單資料不正確，請重新偵測。";
        var named = menus.Where(menu => menu.Include && !string.IsNullOrWhiteSpace(menu.RouterName))
            .Select(menu => menu.RouterName!.Trim()).ToList();
        if (named.FirstOrDefault(name => !SiteMenuSelection.IsValidRouterName(name)) is { } invalid)
            return $"路徑名稱「{invalid}」只能用英文、數字、「-」「_」，最多 50 字，且不能是 Home。";
        if (named.GroupBy(name => name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1) is { } duplicate)
            return $"路徑名稱「{duplicate.Key}」重複。";
        return null;
    }

    // 匯入前偵測列表（只讀舊站、不寫入新站），結果在任務的 resultJson（mode = detect）。
    [HttpPost]
    public async Task<IActionResult> Detect(long websiteId, DetectRequest input)
    {
        if (!await CanAccessAsync(websiteId)) return Forbid();
        if (!await ValidateRequestTokenAsync()) return InvalidRequestToken();
        if (ParseSource(input.Url) is not { } url)
            return BadRequest(new { message = "請輸入有效的 HTTP 或 HTTPS 網址。" });
        return await CreateAndScheduleAsync(websiteId,
            task => jobs.Enqueue<SiteCrawlBackgroundJob>(job => job.Detect(task, url, CancellationToken.None)), DetectTaskTag);
    }

    private static string? ParseSource(string? input)
    {
        var source = input?.Trim();
        if (string.IsNullOrEmpty(source) || source.Length > 2048 ||
            !Uri.TryCreate(source, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") || string.IsNullOrEmpty(uri.Host) ||
            uri.Port is not (80 or 443) || !string.IsNullOrEmpty(uri.UserInfo))
            return null;
        return new UriBuilder(uri) { Fragment = "" }.Uri.AbsoluteUri;
    }

    private async Task<IActionResult> CreateAndScheduleAsync(long websiteId, Func<long, string> enqueue,
        string? initialResult = null)
    {
        BackgroundTaskRecord task;
        try
        {
            task = await tasks.CreateSiteCrawlTaskAsync(websiteId, await user.GetUserId(), initialResult);
        }
        catch (BackgroundTaskConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        return await ScheduleAsync(task, enqueue);
    }

    private async Task<IActionResult> ScheduleAsync(BackgroundTaskRecord task, Func<long, string> enqueue)
    {
        string jobId;
        try
        {
            jobId = enqueue(task.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not enqueue site crawl {TaskId}", task.Id);
            await tasks.FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.Queued, BackgroundTaskStatusEnum.Failed,
                "無法排程網站匯入", SiteCrawlBackgroundJob.EmptyResult, "無法排程網站匯入，請稍後再試。");
            return StatusCode(503, new { message = "無法排程網站匯入，請稍後再試。" });
        }

        try
        {
            await tasks.SetHangfireJobIdAsync(task.Id, jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not save Hangfire job ID {JobId} for site crawl {TaskId}", jobId, task.Id);
        }
        return Accepted(new { taskId = task.Id.ToString() });
    }

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Latest(long websiteId)
    {
        if (!await CanAccessAsync(websiteId)) return Forbid();
        var task = await tasks.GetLatestSiteCrawlAsync(websiteId, await user.GetUserId());
        return task is null ? NoContent() : Ok(ToStatus(task));
    }

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetStatus(long taskId, long websiteId)
    {
        if (!await CanAccessAsync(websiteId)) return Forbid();
        var task = await GetOwnTaskAsync(taskId, websiteId);
        return task is null ? NotFound() : Ok(ToStatus(task));
    }

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> RequestToken(long websiteId)
    {
        if (!await CanAccessAsync(websiteId)) return Forbid();
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName });
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(long taskId, long websiteId)
    {
        if (!await CanAccessAsync(websiteId)) return Forbid();
        if (!await ValidateRequestTokenAsync()) return InvalidRequestToken();
        var task = await GetOwnTaskAsync(taskId, websiteId);
        if (task is null) return NotFound();
        await tasks.RequestSiteCrawlCancellationAsync(task);
        var current = await GetOwnTaskAsync(taskId, websiteId);
        return current is null ? NotFound() : Ok(ToStatus(current));
    }

    private async Task<bool> ValidateRequestTokenAsync()
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
            return true;
        }
        catch (AntiforgeryValidationException ex)
        {
            logger.LogWarning("Site crawl antiforgery validation failed on {Path}: {Reason}", Request.Path, ex.Message);
            return false;
        }
    }

    private IActionResult InvalidRequestToken() =>
        BadRequest(new { message = "防偽驗證失敗。請確認使用 HTTPS 且瀏覽器允許本站 Cookie。" });

    private async Task<bool> CanAccessAsync(long websiteId)
        => websiteId > 0 && websiteId == await user.GetWebsiteId() && await user.isSystemUser()
            && await user.CheckedWebSiteId(await user.GetUserId(), websiteId);

    private async Task<BackgroundTaskRecord?> GetOwnTaskAsync(long taskId, long websiteId)
    {
        var task = await tasks.GetForUserAsync(taskId, websiteId, await user.GetUserId());
        return task is { Type: BackgroundTaskTypeEnum.SiteCrawl, IsDeleted: false } ? task : null;
    }

    private static object ToStatus(BackgroundTaskRecord task) => new
    {
        taskId = task.Id.ToString(), status = task.Status.ToString(),
        progress = task.Progress, message = task.Message, error = task.Error, resultJson = task.ResultJson
    };

    public sealed record StartRequest(string? Url, string[]? ProductLists, SiteMenuChoice[]? Menus);
    public sealed record DetectRequest(string? Url);
}
