using EtheriT.Coker.Application;
using EtheriT.Coker.Application.ArticleTemplates;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Shared.Dto.ArticleTemplate;
using Hangfire;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

// 目錄文章樣板：下拉選單清單、套用前的影響範圍、背景套用。
[ApiController, Authorize, Route("api/[controller]/[action]")]
public sealed class ArticleTemplateController(
    LoginUserData user,
    ArticleTemplateAppService articleTemplates,
    BackgroundTaskService tasks,
    IBackgroundJobClient jobs,
    IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public IActionResult GetList() => Ok(articleTemplates.GetTemplates());

    [HttpGet]
    public async Task<IActionResult> GetChangeInfo(long directoryId, string? templateKey, string? fromTemplateKey)
    {
        try
        {
            return Ok(await articleTemplates.GetChangeInfoAsync(directoryId, templateKey, fromTemplateKey));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Apply(ArticleTemplateApplyInputDto input)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return BadRequest(new { message = "頁面已過期，請重新整理後再試。" });
        }

        try
        {
            var info = await articleTemplates.GetChangeInfoAsync(input.DirectoryId, null, null);
            if (info.ApplicableCount == 0)
                return BadRequest(new { message = "這個目錄沒有可套用樣板的匯入文章。" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var websiteId = await user.GetWebsiteId();
        var userId = await user.GetUserId();
        try
        {
            var task = await tasks.CreateArticleTemplateApplyTaskAsync(websiteId, userId, input.DirectoryId);
            try
            {
                var jobId = jobs.Enqueue<ArticleTemplateApplyBackgroundJob>(job => job.Run(task.Id, input.DirectoryId));
                await tasks.SetHangfireJobIdAsync(task.Id, jobId);
                return Accepted(new { taskId = task.Id });
            }
            catch (Exception ex)
            {
                await tasks.FailAsync(task.Id, ex.Message);
                throw;
            }
        }
        catch (BackgroundTaskConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}