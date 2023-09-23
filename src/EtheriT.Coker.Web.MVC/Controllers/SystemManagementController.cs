using EtheriT.Coker.Application.StoreSet;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class SystemManagementController : Controller
    {
        private readonly IStoreSetAppService _storeSetAppService;
        public SystemManagementController(IStoreSetAppService _storeSetAppService) { 
            this._storeSetAppService = _storeSetAppService;
		}
        public IActionResult Index()
        {
            return View("WebData");
        }
        public async Task<IActionResult> SEO()
        {
            var response = await _storeSetAppService.getAll(1);
			return View("SEO", response);
        }
    }
}
