using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EtheriT.Coker.Web.MVC.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        public AuthenticationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            bool isApiRequest =
                context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/front") ||
                path.StartsWith("/DXX", StringComparison.OrdinalIgnoreCase);

            if (isApiRequest)
            {
                await _next(context);
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var accountAppService = scope.ServiceProvider.GetRequiredService<IAccountAppService>();
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

            // Welcome 不做選單權限檢查
            if (isWelcomeController)
            {
                await _next(context);
                return;
            }

            // 已登入：檢查選單權限
            if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
            {
                var navigation = scope.ServiceProvider.GetRequiredService<NavigationProvider>();
                var site = await navigation.getMenus();

                await navigation.SetPower(site);
                await navigation.SetWebsite(site);
                await navigation.setUserJob(site);

                var menu = navigation.FindJob(site.Jobs, controllerName, actionName);

                if (menu == null || !menu.CanVisble)
                {
                    var firstMenu = FindFirstVisibleMenu(site.Jobs);

                    if (firstMenu == null)
                        context.Response.Redirect("/Welcome");
                    else
                        context.Response.Redirect($"/{firstMenu.Controller}/{firstMenu.Action}");

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
