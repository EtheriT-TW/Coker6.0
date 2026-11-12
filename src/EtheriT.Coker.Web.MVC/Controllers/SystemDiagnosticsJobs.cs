using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Web.MVC.Diagnostics;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers;

public partial class SystemDiagnosticsController
{
    private DiagnosticsWorkStore WorkStore => HttpContext.RequestServices.GetRequiredService<DiagnosticsWorkStore>();

    private async Task<bool> AuthorizeWork(long websiteId)
    {
        if (!await loginUserData.IsPlatformAdministrator() || websiteId <= 0 ||
            websiteId != await loginUserData.GetWebsiteId()) return false;
        var user = await loginUserData.GetUserId();
        if (user <= 0) return false;
        HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(websiteId, user);
        return true;
    }

    private async Task<IActionResult> StartDiagnostics(long websiteId, string stamp, bool publicScan, bool retryOnly, bool functionScan = false)
    {
        Response.Headers.CacheControl = "no-store";
        if (!await AuthorizeWork(websiteId)) return Forbid();
        var user = await loginUserData.GetUserId();
        try
        {
            // Resolve abandoned Hangfire work before deciding whether a new run is allowed.
            WorkStore.Load(websiteId, user);
            using var connection = WorkStore.Connect();
            using var gate = WorkStore.Lock(connection, websiteId, user);
            if (WorkStore.Read(connection, websiteId, user)?.Active == true) return StatusCode(429);
            var plans = HttpContext.RequestServices.GetRequiredService<DiagnosticsPlanBuilder>();
            List<int> targets;
            PublicPlan? publicPlan = null;
            BackofficeTestListModel? backoffice = null;
            if (publicScan)
            {
                publicPlan = await plans.PublicAsync(websiteId, HttpContext.RequestAborted, functionScan);
                if (publicPlan.Error != null || publicPlan.Stamp != stamp) return StatusCode(409);
                var last = retryOnly ? await PublicStoreFor(functionScan).LoadAsync(websiteId, user, stamp) : null;
                targets = DiagnosticsWork.SelectTargets(publicPlan.Targets.Count, retryOnly, index =>
                {
                    var saved = last?.Pages.GetValueOrDefault(publicPlan.Targets[index].Url);
                    return (functionScan && (saved == null || !saved.Completed || saved.Findings.Any(finding => finding.Status == PublicCheckStatus.NotRun)))
                        || saved?.Findings.Any(finding => finding.Status is PublicCheckStatus.Failed or PublicCheckStatus.NeedsReview) == true;
                });
            }
            else
            {
                backoffice = await plans.BackofficeAsync();
                if (backoffice.ScopeStamp != stamp) return StatusCode(409);
                var last = retryOnly ? await ResultStore.LoadAsync(websiteId, user, stamp) : null;
                targets = DiagnosticsWork.SelectTargets(backoffice.Pages.Count, retryOnly, index =>
                    last?.Pages.GetValueOrDefault(backoffice.Pages[index].Id) is { } page &&
                    (page.BasicStatus == BackofficeTestStatus.Failed || page.Cases.Any(test => test.Status == BackofficeTestStatus.Failed)));
            }
            if (targets.Count == 0) return BadRequest();
            var work = new DiagnosticsWork(Guid.NewGuid().ToString("N"), publicScan, stamp, targets) { FunctionScan = publicScan && functionScan };
            WorkStore.Save(connection, websiteId, user, work);
            try
            {
                // Clear the persisted targets before enqueueing; old successes must not
                // reappear as current results when the user returns during a run.
                if (publicPlan != null)
                    await PublicStoreFor(functionScan).ResetAsync(websiteId, user, stamp, targets.Select(index => publicPlan.Targets[index].Url));
                else
                    await ResultStore.SaveAsync(websiteId, user, stamp, targets.Select(index => backoffice!.Pages[index]));
                var client = HttpContext.RequestServices.GetRequiredService<IBackgroundJobClient>();
                var jobId = client.Enqueue<DiagnosticsBackgroundJob>(job =>
                    job.ExecuteAsync(websiteId, user, work.Id, CancellationToken.None));
                WorkStore.Save(connection, websiteId, user, work with { JobId = jobId });
            }
            catch
            {
                WorkStore.Save(connection, websiteId, user, work with
                { State = "Interrupted", Message = "未能建立背景檢測，請稍後再試或聯絡系統維護人員。" });
                throw;
            }
            return Accepted(new { message = "已提交背景檢測，可離開頁面，稍後返回查看。" });
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Could not enqueue diagnostics for site {Site}", websiteId);
            return StatusCode(503, new { message = "無法提交背景檢測，請稍後再試或聯絡系統維護人員。" });
        }
    }

    [HttpGet("/api/SystemDiagnostics/Status")]
    public async Task<IActionResult> DiagnosticsStatus(long websiteId, bool publicScan, string scopeStamp, bool functionScan = false)
    {
        Response.Headers.CacheControl = "no-store";
        if (!await AuthorizeWork(websiteId)) return Forbid();
        try
        {
            var plans = HttpContext.RequestServices.GetRequiredService<DiagnosticsPlanBuilder>();
            var stamp = publicScan ? (await plans.PublicAsync(websiteId, HttpContext.RequestAborted, functionScan)).Stamp
                : (await plans.BackofficeAsync()).ScopeStamp;
            if (scopeStamp != stamp) return StatusCode(409);
            var work = WorkStore.Load(websiteId, await loginUserData.GetUserId());
            if (work == null) return Json(new { active = false, message = "尚無背景檢測工作。", revision = "none" });
            if (work.PublicScan != publicScan || work.FunctionScan != functionScan || work.Stamp != stamp)
                return Json(new { active = work.Active, message = work.Active
                    ? "此帳號在目前站台另有檢測執行中，請至對應檢測頁查看。"
                    : "檢測範圍已變更或目前類型沒有工作，可重新開始檢測。", revision = "other" });
            // Do not expose Hangfire IDs, scope internals, URLs or exception details.
            return Json(new { active = work.Active, stopping = work.CancelRequested,
                completed = work.Completed, total = work.Targets.Count, message = work.Message,
                revision = work.Id + ":" + work.UpdatedUtc.Ticks });
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Could not read diagnostics status for site {Site}", websiteId);
            return StatusCode(503);
        }
    }

    [HttpPost("/api/SystemDiagnostics/Stop"), ValidateAntiForgeryToken]
    public async Task<IActionResult> StopDiagnostics([FromForm] long websiteId, [FromForm] bool publicScan,
        [FromForm] string scopeStamp, [FromForm] bool functionScan = false)
    {
        Response.Headers.CacheControl = "no-store";
        if (!await AuthorizeWork(websiteId)) return Forbid();
        try
        {
            var user = await loginUserData.GetUserId();
            var work = WorkStore.Load(websiteId, user);
            if (work == null || work.PublicScan != publicScan || work.FunctionScan != functionScan || work.Stamp != scopeStamp) return StatusCode(409);
            WorkStore.Update(websiteId, user, work.Id, latest => latest with
            {
                CancelRequested = true,
                State = latest.State == "Queued" ? "Stopped" : latest.State,
                Message = latest.State == "Queued" ? "已取消排隊中的檢測。" : "已要求停止，正在等待目前檢測安全結束。"
            });
            return Accepted();
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Could not stop diagnostics for site {Site}", websiteId);
            return StatusCode(503);
        }
    }

    [HttpGet("/api/SystemDiagnostics/Results")]
    public async Task<IActionResult> DiagnosticsResults(long websiteId, bool publicScan, bool functionScan = false)
    {
        Response.Headers.CacheControl = "no-store";
        if (!await AuthorizeWork(websiteId)) return Forbid();
        return publicScan ? await PublicPage(false, true, functionScan) : await BuildBackofficePage(false, "_BackofficeResults");
    }
}
