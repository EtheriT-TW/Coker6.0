using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EtheriT.Coker.Web.MVC.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly NavigationProvider _navigation;
        private readonly IConfiguration _configuration;
        private readonly ITokenAppService _tokenAppService;
        private readonly IAccountAppService _accountAppService;
        public AuthenticationMiddleware(RequestDelegate next, NavigationProvider navigation, IConfiguration configuration, IAccountAppService accountAppService)
        {
            _next = next;
            _navigation = navigation;
            _configuration = configuration;
            _accountAppService = accountAppService;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            bool isApiRequest = context.Request.Path.StartsWithSegments("/api");
            if (isApiRequest)
            {
                await _next(context); return;
            }
            else
            {
                var isAuthenticated = await _accountAppService.Chech();
                var controllerName = context.GetRouteData()?.Values["controller"]?.ToString();
                var actionName = context.GetRouteData()?.Values["action"]?.ToString();
                if (isAuthenticated.Success && !string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName) && !(controllerName == "Account" && actionName == "Index"))
                {
                    var site = await _navigation.getMenus();
                    await _navigation.SetPower(site);
                    await _navigation.setUserJob(site);
                    var menu = _navigation.FindJob(site.Jobs, controllerName, actionName);
                    if (menu == null || !menu.CanVisble)
                    {
                        var firstMenu = site.Jobs.FirstOrDefault(m => m.CanVisble);
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
}
