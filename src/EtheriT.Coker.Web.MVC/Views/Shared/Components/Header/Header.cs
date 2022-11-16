using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Web.MVC.Models.Header;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {
        private readonly IAccountAppService accountAppService;
        public Header(IAccountAppService accountAppService) {
            this.accountAppService = accountAppService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await accountAppService.GetCurrentUser();
            HeaderModel model = new HeaderModel { 
                User = user
            };
            return View(model);
        }
    }
}
