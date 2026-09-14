using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Web.MVC.Models.Header;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {
        private readonly IBackstageAccountAppService accountAppService;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;

        public Header(
            IBackstageAccountAppService accountAppService,
            LoginUserData loginUserData,
            IConfiguration configuration) {
            this.accountAppService = accountAppService;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await accountAppService.GetCurrentUser();
            HeaderModel model = new HeaderModel { 
                User = user,
                DefaultUrl = await loginUserData.GetWebsiteUrl(),
                CanAccessPlatform = await loginUserData.isSystemUser(),
                PlatformUrl = configuration["SystemLinks:PlatformUrl"] ?? string.Empty
            };
            return View(model);
        }
    }
}
