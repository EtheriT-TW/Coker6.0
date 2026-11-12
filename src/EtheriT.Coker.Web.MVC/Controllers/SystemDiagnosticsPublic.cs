using System.Text.Json;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Web.MVC.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers;

public partial class SystemDiagnosticsController
{
    private PublicResultStore PublicStoreFor(bool functionScan) => new(
        HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath, functionScan);

    private Task<PublicPlan> BuildPublicPlan(long websiteId, bool functionScan = false) =>
        HttpContext.RequestServices.GetRequiredService<DiagnosticsPlanBuilder>().PublicAsync(websiteId, HttpContext.RequestAborted, functionScan);

    private async Task<IActionResult> PublicPage(bool showAll, bool partial = false, bool functionScan = false)
    {
        if (!await loginUserData.IsPlatformAdministrator()) return Forbid();
        Response.Headers.CacheControl = "no-store";
        var site = await loginUserData.GetWebsiteId();
        var user = await loginUserData.GetUserId();
        HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(site, user);
        var plan = await BuildPublicPlan(site, functionScan);
        PublicReport? report = null;
        string? warning = null;
        try { report = await PublicStoreFor(functionScan).LoadAsync(site, user, plan.Stamp); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        { logger.LogWarning(error, "Could not load public diagnostics for site {Site}", site); warning = "無法讀取上次前台紀錄，仍可重新檢測。"; }
        ViewData["Title"] = functionScan ? "PublicFunctionTests" : "PublicResourceTests";
        var model = new PublicDiagnosticsModel(plan, report, showAll, warning) { FunctionScan = functionScan };
        return partial ? PartialView("_PublicResults", model) : View("Public", model);
    }

    [HttpPost("/api/SystemDiagnostics/RunPublic"), ValidateAntiForgeryToken]
    public Task<IActionResult> RunPublic([FromForm] long websiteId, [FromForm] string scopeStamp,
        [FromForm] bool retryOnly = false) => StartDiagnostics(websiteId, scopeStamp, true, retryOnly);

    [HttpGet]
    public Task<IActionResult> Functions(bool showAll = false) => PublicPage(showAll, functionScan: true);

    [HttpPost("/api/SystemDiagnostics/RunFunctions"), ValidateAntiForgeryToken]
    public Task<IActionResult> RunFunctions([FromForm] long websiteId, [FromForm] string scopeStamp,
        [FromForm] bool retryOnly = false) => StartDiagnostics(websiteId, scopeStamp, true, retryOnly, true);
}
