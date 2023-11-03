using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class PowerManagementController
	{
		private readonly NavigationProvider navigation;
		public PowerManagementController(NavigationProvider navigation)
		{
			this.navigation = navigation;
		}
		[HttpGet]
		public async Task<Site> AllMenus() {
			return await navigation.getMenus();
		}
	}
}
