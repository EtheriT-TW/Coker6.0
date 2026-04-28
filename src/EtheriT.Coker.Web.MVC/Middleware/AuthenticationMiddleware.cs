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

            bool isAccountPage =
                string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase);

            bool isLoginPage =
                string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(actionName, "Index", StringComparison.OrdinalIgnoreCase);

            bool isPublicAccountPage =
                string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase) &&
                (
                    string.Equals(actionName, "Index", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(actionName, "Register", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(actionName, "Forget", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(actionName, "NewPassword", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(actionName, "Privacy", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(actionName, "Error", StringComparison.OrdinalIgnoreCase)
                );

            // 未登入：只允許登入、註冊、忘記密碼等 Account 頁面
            if (!isAuthenticated.Success)
            {
                if (isPublicAccountPage)
                {
                    await _next(context);
                    return;
                }

                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                var loginUrl = "/Account/Index";

                if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl != "/")
                {
                    loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
                }

                context.Response.Redirect(loginUrl);
                return;
            }

            // 已登入：如果還進登入頁，就導到第一個有權限的頁面
            if (isLoginPage)
            {
                var navigation = scope.ServiceProvider.GetRequiredService<NavigationProvider>();
                var site = await navigation.getMenus();

                await navigation.SetPower(site);
                await navigation.SetWebsite(site);
                await navigation.setUserJob(site);

                var firstMenu = FindFirstVisibleMenu(site.Jobs);

                if (firstMenu == null)
                    context.Response.Redirect("/Welcome");
                else
                    context.Response.Redirect($"/{firstMenu.Controller}/{firstMenu.Action}");

                return;
            }

            // Welcome 不做選單權限檢查
            if (string.Equals(controllerName, "Welcome", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // 其他頁面：登入後才檢查選單權限
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
