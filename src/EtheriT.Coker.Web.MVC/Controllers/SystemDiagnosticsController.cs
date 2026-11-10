using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Diagnostics;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Controllers;
using AutoMapper;
using EtheriT.Coker.Application.Common;
using Newtonsoft.Json.Linq;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    [Authorize]
    public partial class SystemDiagnosticsController : Controller
    {
        private readonly LoginUserData loginUserData;
        private readonly IActionDescriptorCollectionProvider actionDescriptors;
        private readonly NavigationProvider navigation;
        private readonly IRazorViewEngine viewEngine;
        private readonly ILogger<SystemDiagnosticsController> logger;
        private readonly BackofficeSandboxFactory sandboxFactory;
        private readonly IMapper mapper;
        private BackofficeResultStore ResultStore => new(
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath);

        public SystemDiagnosticsController(LoginUserData loginUserData,
            IActionDescriptorCollectionProvider actionDescriptors, NavigationProvider navigation,
            IRazorViewEngine viewEngine, ILogger<SystemDiagnosticsController> logger,
            BackofficeSandboxFactory sandboxFactory, IMapper mapper)
        {
            this.loginUserData = loginUserData;
            this.actionDescriptors = actionDescriptors;
            this.navigation = navigation;
            this.viewEngine = viewEngine;
            this.logger = logger;
            this.sandboxFactory = sandboxFactory;
            this.mapper = mapper;
        }

        [HttpGet]
        public Task<IActionResult> Backoffice(bool showAll = false) => BuildBackofficePage(showAll, "Backoffice");

        private async Task<IActionResult> BuildBackofficePage(bool showAll, string viewName)
        {
            if (!await loginUserData.IsPlatformAdministrator()) return Forbid();

            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(websiteId, userId);
            ViewData["Title"] = viewName == "History" ? "SystemDiagnosticsHistory" : "BackofficeFunctionTests";
            var scope = await navigation.BuildDiagnosticsScopeAsync();
            var model = BackofficeTestCatalog.Build(actionDescriptors, scope.Jobs, true) with { ShowAll = showAll };
            try
            {
                var last = await ResultStore.LoadAsync(websiteId, userId, viewName == "History" ? null : model.ScopeStamp);
                if (last != null)
                {
                    ViewData["LastDiagnosticsAt"] = last.AtUtc.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
                    if (last.ScopeStamp != model.ScopeStamp)
                    {
                        ViewData["DiagnosticsHistoryWarning"] = "已找到上次紀錄，但目前可用功能或檢測規格已變更，請重新檢測；舊結果不列入目前狀態。";
                    }
                    else
                    {
                    // Restore results only, keeping the current catalog and scope authoritative.
                    model = model with { Pages = model.Pages.Select(page => last.Pages.TryGetValue(page.Id, out var saved)
                        ? page with { BasicStatus = saved.BasicStatus, BasicMessage = saved.BasicMessage,
                            Cases = page.Cases.Select(test => saved.Cases.FirstOrDefault(candidate => candidate.Id == test.Id) is { } old
                                ? test with { Status = old.Status, ResultMessage = old.ResultMessage } : test).ToList() }
                        : page).ToList() };
                    }
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                logger.LogWarning(exception, "Could not load diagnostics history for site {WebsiteId}", websiteId);
                ViewData["DiagnosticsHistoryWarning"] = "無法載入上次檢測紀錄，請聯絡系統維護人員；仍可執行新檢測。";
            }
            Response.Headers.CacheControl = "no-store";
            if (viewName == "History")
            {
                var publicPlan = await BuildPublicPlan(websiteId);
                PublicReport? publicReport = null;
                string? publicWarning = publicPlan.Error;
                try { publicReport = await PublicStore.LoadAsync(websiteId, userId, publicPlan.Stamp); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
                { logger.LogWarning(exception, "Could not load public history for site {Site}", websiteId); publicWarning = "無法載入前台檢測紀錄。"; }
                ViewData["PublicDiagnostics"] = new PublicDiagnosticsModel(publicPlan, publicReport, showAll, publicWarning);
            }
            return View(viewName, model);
        }

        [HttpPost("/api/SystemDiagnostics/Run"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Run([FromForm] long websiteId, [FromForm] int? pageIndex, [FromForm] string? scopeStamp)
        {
            Response.Headers.CacheControl = "no-store";
            if (!await loginUserData.IsPlatformAdministrator()) return Forbid();
            if (websiteId <= 0 || websiteId != await loginUserData.GetWebsiteId())
                return StatusCode(409);
            var userId = await loginUserData.GetUserId();
            // Pin this request to the authorized site even if another browser tab
            // changes the shared login token while the scan is in progress.
            HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(websiteId, userId);

            var scope = await navigation.BuildDiagnosticsScopeAsync();
            var model = BackofficeTestCatalog.Build(actionDescriptors, scope.Jobs, true);
            if (scopeStamp != model.ScopeStamp) return StatusCode(409);
            if (!pageIndex.HasValue || pageIndex.Value < 0 || pageIndex.Value >= model.Pages.Count) return BadRequest();
            var selected = model.Pages[pageIndex.Value];
            using var runGate = DiagnosticsRunGate.TryEnter(websiteId, userId);
            if (runGate == null) return StatusCode(429);
            var level = await loginUserData.GetWebsiteLevel();
            var actions = actionDescriptors.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                .Select(action => (action.ControllerName, action.ActionName)).ToHashSet();
            var pages = new List<BackofficeTestPage>();
            var errors = new Dictionary<string, string>();
            foreach (var page in new[] { selected })
            {
                var passed = false;
                try
                {
                    var views = page.Sources.Where(source => source.StartsWith("Views/", StringComparison.Ordinal)
                        && source.EndsWith(".cshtml", StringComparison.Ordinal)).ToList();
                    passed = actions.Contains((page.Controller, page.Action)) && views.Count > 0 &&
                        views.All(view => viewEngine.GetView(null, "~/" + view, true).Success);
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Diagnostics view lookup failed for {PageId}", page.Id);
                    errors[page.Id] = exception.ToString();
                }
                var cases = new List<BackofficeTestCase>();
                var samples = new Dictionary<string, JArray>();
                foreach (var test in page.Cases)
                {
                    HttpContext.RequestAborted.ThrowIfCancellationRequested();
                    var status = BackofficeTestStatus.Skipped;
                    var message = "尚未實作此項自動檢測，需另行驗證。";
                    try
                    {
                        var check = await BackofficeReadOnlyChecks.ExecuteAsync(page.Id, test.Executor,
                            HttpContext.RequestServices, HttpContext.RequestAborted, samples);
                        check ??= await BackofficeObjectChecks.ExecuteAsync(page.Id, test.Executor,
                            HttpContext.RequestServices, HttpContext.RequestAborted);
                        check ??= await BackofficeSandboxChecks.ExecuteAsync(page.Id, test.Executor,
                            sandboxFactory, mapper, level, HttpContext.RequestAborted);
                        if (check != null)
                        {
                            status = check.Status;
                            message = check.Message;
                            if (status == BackofficeTestStatus.Failed)
                            {
                                errors[test.Id] = message;
                                logger.LogWarning("Diagnostics assertion failed for {CaseId} on site {WebsiteId}: {Summary}",
                                    test.Id, websiteId, message);
                            }
                        }
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        logger.LogWarning(exception, "Diagnostics failed for {CaseId} on site {WebsiteId}", test.Id, websiteId);
                        errors[test.Id] = exception.ToString();
                        status = BackofficeTestStatus.Failed;
                        message = "此項檢測未通過，請聯絡系統維護人員。";
                    }
                    finally
                    {
                        // A blocked SaveChanges must not leave mutated tracked
                        // entities available to a later check in this request.
                        HttpContext.RequestServices.GetRequiredService<CokerDbContext>().ChangeTracker.Clear();
                    }
                    cases.Add(test with { Status = status, ResultMessage = message });
                }
                pages.Add(page with
                {
                    BasicStatus = passed ? BackofficeTestStatus.Passed : BackofficeTestStatus.Failed,
                    BasicMessage = passed ? "頁面基本檢查通過；完整操作尚未驗證。" : "頁面基本檢查未通過，請聯絡系統維護人員。",
                    Cases = cases
                });
            }
            try { await ResultStore.SaveAsync(websiteId, userId, model.ScopeStamp, pages, errors); }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
            {
                logger.LogWarning(exception, "Could not save diagnostics history for site {WebsiteId}", websiteId);
                ViewData["DiagnosticsHistoryWarning"] = "本次結果未能儲存，重新整理後可能無法還原，請聯絡系統維護人員。";
            }
            return PartialView("_BackofficeResults", model with
            {
                Pages = pages, HasRun = true, TotalCases = selected.Cases.Count,
                PendingCases = pages.Sum(page => page.Cases.Count(test => test.Status is BackofficeTestStatus.NotRun or BackofficeTestStatus.Skipped)),
                FailedCases = pages.Count(page => page.BasicStatus == BackofficeTestStatus.Failed) +
                    pages.Sum(page => page.Cases.Count(test => test.Status == BackofficeTestStatus.Failed))
            });
        }

        [HttpGet]
        public Task<IActionResult> Public(bool showAll = false) => PublicPage(showAll);

        [HttpGet]
        public Task<IActionResult> History(bool showAll = false) => BuildBackofficePage(showAll, "History");

        private async Task<IActionResult> ShowPage(string pageName, string heading, string message)
        {
            if (!await loginUserData.IsPlatformAdministrator()) return Forbid();

            ViewData["Title"] = pageName;
            ViewData["PageHeading"] = heading;
            ViewData["EmptyMessage"] = message;
            return View("Placeholder");
        }
    }
}
