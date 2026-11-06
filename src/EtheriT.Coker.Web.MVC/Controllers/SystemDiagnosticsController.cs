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

namespace EtheriT.Coker.Web.MVC.Controllers
{
    [Authorize]
    public class SystemDiagnosticsController : Controller
    {
        private readonly LoginUserData loginUserData;
        private readonly IActionDescriptorCollectionProvider actionDescriptors;
        private readonly NavigationProvider navigation;
        private readonly IRazorViewEngine viewEngine;
        private readonly ILogger<SystemDiagnosticsController> logger;
        private readonly BackofficeSandboxFactory sandboxFactory;
        private readonly IMapper mapper;

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
        public async Task<IActionResult> Backoffice(bool showAll = false)
        {
            if (!await loginUserData.IsPlatformAdministrator()) return Forbid();

            ViewData["Title"] = "BackofficeFunctionTests";
            var scope = await navigation.BuildDiagnosticsScopeAsync();
            return View(BackofficeTestCatalog.Build(actionDescriptors, scope.Jobs, true) with { ShowAll = showAll });
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
                                logger.LogWarning("Diagnostics assertion failed for {CaseId} on site {WebsiteId}: {Summary}",
                                    test.Id, websiteId, message);
                        }
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        logger.LogWarning(exception, "Diagnostics failed for {CaseId} on site {WebsiteId}", test.Id, websiteId);
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
            return PartialView("_BackofficeResults", model with
            {
                Pages = pages, HasRun = true, TotalCases = selected.Cases.Count,
                PendingCases = pages.Sum(page => page.Cases.Count(test => test.Status is BackofficeTestStatus.NotRun or BackofficeTestStatus.Skipped)),
                FailedCases = pages.Count(page => page.BasicStatus == BackofficeTestStatus.Failed) +
                    pages.Sum(page => page.Cases.Count(test => test.Status == BackofficeTestStatus.Failed))
            });
        }

        [HttpGet]
        public Task<IActionResult> Public() => ShowPage(
            "PublicFunctionTests", "前台功能測試",
            "尚未建立前台功能檢測案例，目前不會執行任何功能測試。");

        [HttpGet]
        public Task<IActionResult> History() => ShowPage(
            "SystemDiagnosticsHistory", "檢測紀錄",
            "目前尚未提供檢測執行與紀錄功能。");

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
