using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Web.MVC.Models.Header;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {
        private readonly IAccountAppService accountAppService;
        private readonly LoginUserData loginUserData;
        public Header(IAccountAppService accountAppService, LoginUserData loginUserData) {
            this.accountAppService = accountAppService;
            this.loginUserData = loginUserData;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await accountAppService.GetCurrentUser();
            HeaderModel model = new HeaderModel { 
                User = user,
                DefaultUrl = await loginUserData.GetWebsiteUrl()
            };
            return View(model);
        }
    }
}
