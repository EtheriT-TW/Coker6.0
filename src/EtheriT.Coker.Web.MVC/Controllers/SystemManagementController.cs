using EtheriT.Coker.Application;
using EtheriT.Coker.Application.StoreSet;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class SystemManagementController : Controller
    {
        private readonly IStoreSetAppService _storeSetAppService;
        private readonly IWebsiteApplication _WebsiteApplication;
        public SystemManagementController(IStoreSetAppService _storeSetAppService, IWebsiteApplication _WebsiteApplication) { 
            this._storeSetAppService = _storeSetAppService;
            this._WebsiteApplication = _WebsiteApplication;
		}
        public async Task<IActionResult> Index()
        {
            var response = await _WebsiteApplication.GetWebsiteData();
            if (response.Success)
            {
                return View("WebData", response);
            }
            else {
                return Redirect("/Welcome");
            }
            
        }
        public async Task<IActionResult> SEO()
        {
            var response = await _storeSetAppService.getAll(1);
			return View("SEO", response);
        }
    }
}
