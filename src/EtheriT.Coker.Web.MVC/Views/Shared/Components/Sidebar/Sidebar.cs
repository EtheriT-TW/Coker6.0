using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Sidebar : ViewComponent
    {
        private readonly NavigationProvider navigation;
		public Sidebar(NavigationProvider navigation)
        {
            this.navigation = navigation;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var site = await navigation.getMenus();
            await navigation.SetPower(site);
            return View(site);
        }
    }
}
