using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Diagnostics;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EtheriT.Coker.Application.Common;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    [Authorize]
    public partial class SystemDiagnosticsController : Controller
    {
        private readonly LoginUserData loginUserData;
        private readonly IActionDescriptorCollectionProvider actionDescriptors;
        private readonly NavigationProvider navigation;
        private readonly ILogger<SystemDiagnosticsController> logger;
        private BackofficeResultStore ResultStore => new(
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath);

        public SystemDiagnosticsController(LoginUserData loginUserData,
            IActionDescriptorCollectionProvider actionDescriptors, NavigationProvider navigation,
            ILogger<SystemDiagnosticsController> logger)
        {
            this.loginUserData = loginUserData;
            this.actionDescriptors = actionDescriptors;
            this.navigation = navigation;
            this.logger = logger;
        }

        [HttpGet]
        public Task<IActionResult> Backoffice(bool showAll = false) => BuildBackofficePage(showAll, "Backoffice");

        private async Task<IActionResult> BuildBackofficePage(bool showAll, string viewName)
        {
            if (!await loginUserData.IsPlatformAdministrator()) return Forbid();

            var websiteId = await loginUserData.GetWebsiteId();
            var userId = await loginUserData.GetUserId();
            HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(websiteId, userId);
            ViewData["Title"] = "BackofficeFunctionTests";
            var scope = await navigation.BuildDiagnosticsScopeAsync();
            var model = BackofficeTestCatalog.Build(actionDescriptors, scope.Jobs, true) with { ShowAll = showAll };
            try
            {
                var last = await ResultStore.LoadAsync(websiteId, userId, model.ScopeStamp);
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
            return viewName == "_BackofficeResults" ? PartialView(viewName, model) : View(viewName, model);
        }

        [HttpPost("/api/SystemDiagnostics/Run"), ValidateAntiForgeryToken]
        public Task<IActionResult> Run([FromForm] long websiteId, [FromForm] string scopeStamp,
            [FromForm] bool retryOnly = false) => StartDiagnostics(websiteId, scopeStamp, false, retryOnly);

        [HttpGet]
        public Task<IActionResult> Public(bool showAll = false) => PublicPage(showAll);

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
