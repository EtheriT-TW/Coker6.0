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
            bool isApiRequest = context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/front") || (context.Request.Path.Value??"").StartsWith("/DXX", StringComparison.OrdinalIgnoreCase);
            if (isApiRequest)
            {
                await _next(context); return;
            }
            else
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _accountAppService = scope.ServiceProvider.GetRequiredService<IAccountAppService>();
                    var isAuthenticated = await _accountAppService.Chech();
                    var controllerName = context.GetRouteData()?.Values["controller"]?.ToString();
                    var actionName = context.GetRouteData()?.Values["action"]?.ToString();
                    if (isAuthenticated.Success && !string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName) && !(controllerName == "Account" && actionName == "Index") && controllerName != "Welcome")
                    {
                        var _navigation = scope.ServiceProvider.GetRequiredService<NavigationProvider>();
                        var site = await _navigation.getMenus();
                        await _navigation.SetPower(site);
                        await _navigation.SetWebsite(site);
                        await _navigation.setUserJob(site);
                        var menu = _navigation.FindJob(site.Jobs, controllerName, actionName);
                        if (menu == null || !menu.CanVisble)
                        {
                            var firstMenu = FindFirstVisibleMenu(site.Jobs);
                            if (firstMenu == null) context.Response.Redirect($"/Welcome");
                            else context.Response.Redirect($"/{firstMenu.Controller}/{firstMenu.Action}");
                            return;
                        }
                        // 用戶已登入，跳過
                        await _next(context);
                    }
                    else
                    {
                        // 如果未登入，進行第三方認證或顯示登入頁面
                        var redirectUrl = context.Request.Path.ToString();
                        var returnUrl = context.Request.Query["returnUrl"];

                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            // 這裡可以根據需要處理認證重定向邏輯
                            context.Response.Redirect("/");
                        }

                        await _next(context);
                    }
                }
            }
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
