using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EtheriT.Coker.Web.MVC.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            bool isApiRequest =
                context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/front") ||
                path.StartsWith("/DXX", StringComparison.OrdinalIgnoreCase);

            // The selected website lives on the shared login token. Bind requests to the
            // website that was active when this browser tab loaded, so an old tab cannot
            // silently operate on a website selected by another tab.
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var loginUserData = context.RequestServices.GetRequiredService<LoginUserData>();
                var currentWebsiteId = await loginUserData.GetWebsiteId();

                if (isApiRequest &&
                    !context.Request.Path.StartsWithSegments("/api/Website/Exchange") &&
                    context.Request.Headers.TryGetValue("X-Coker-Website-Id", out var websiteHeader) &&
                    long.TryParse(websiteHeader.FirstOrDefault(), out var pageWebsiteId) &&
                    pageWebsiteId != currentWebsiteId)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    context.Response.Headers["X-Coker-Website-Mismatch"] = "true";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        error = "網站已在其他分頁切換，此分頁已停止載入與儲存資料。請重新整理頁面。"
                    });
                    return;
                }

                if (!isApiRequest &&
                    context.Request.Query.TryGetValue("_site", out var siteQuery) &&
                    long.TryParse(siteQuery.FirstOrDefault(), out var requestedWebsiteId) &&
                    requestedWebsiteId != currentWebsiteId)
                {
                    var currentPage = context.Request.PathBase + context.Request.Path;
                    context.Response.Redirect($"{currentPage}?siteChanged=true");
                    return;
                }
            }

            if (isApiRequest)
            {
                await _next(context);
                return;
            }

            var accountAppService = context.RequestServices.GetRequiredService<IBackstageAccountAppService>();
            var isAuthenticated = await accountAppService.Chech();

            var controllerName = context.GetRouteData()?.Values["controller"]?.ToString();
            var actionName = context.GetRouteData()?.Values["action"]?.ToString();

            bool isAccountController =
                string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase);

            bool isWelcomeController =
                string.Equals(controllerName, "Welcome", StringComparison.OrdinalIgnoreCase);

            // Account 全部放行：
            // Index / Register / Forget / NewPassword / Privacy / Error
            // 不在 Middleware 裡面重新導頁，避免影響 token 更新或登入頁流程
            if (isAccountController)
            {
                await _next(context);
                return;
            }

            // 未登入：非 Account 頁面才導回登入頁
            if (!isAuthenticated.Success)
            {
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                var loginUrl = "/Account/Index";

                if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl != "/")
                {
                    loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
                }

                context.Response.Redirect(loginUrl);
                return;
            }

            // 已登入：一律初始化本次 Request 的選單與權限
            var navigation = context.RequestServices.GetRequiredService<NavigationProvider>();

            var site = await navigation.BuildAuthorizedSiteAsync();

            // 供 Sidebar、Layout、View 共用
            context.Items[CokerContextKeys.NavigationSite] = site;


            // Welcome 有 Sidebar，但不需要檢查目前頁面的選單權限
            if (isWelcomeController)
            {
                await _next(context);
                return;
            }


            // 一般頁面才檢查目前頁面是否有 View 權限
            if (!string.IsNullOrEmpty(controllerName) &&
                !string.IsNullOrEmpty(actionName))
            {
                var menu = navigation.FindJob(
                    site.Jobs,
                    controllerName,
                    actionName);

                if (menu == null || !menu.CanVisble)
                {
                    var firstMenu = FindFirstVisibleMenu(site.Jobs);

                    if (firstMenu == null)
                    {
                        context.Response.Redirect("/Welcome");
                    }
                    else
                    {
                        context.Response.Redirect(
                            $"/{firstMenu.Controller}/{firstMenu.Action}");
                    }

                    return;
                }
            }

            await _next(context);
        }
        private JobMenu? FindFirstVisibleMenu(IEnumerable<JobMenu> menus)
        {
            foreach (var menu in menus)
            {
                // 檢查當前節點是否符合條件
                if (menu.CanVisble && !string.IsNullOrEmpty(menu.Action) && menu.IsView)
                {
                    return menu;
                }

                // 如果當前節點有子節點，遞迴搜尋
                if (menu.jobItemModels != null)
                {
                    var foundMenu = FindFirstVisibleMenu(menu.jobItemModels);
                    if (foundMenu != null)
                    {
                        return foundMenu;
                    }
                }
            }
            return null; // 沒有符合條件的項目
        }
    }
}
