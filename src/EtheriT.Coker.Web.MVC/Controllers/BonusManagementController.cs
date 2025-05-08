using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Web.MVC.Models.BonusManagement;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class BonusManagementController : Controller
    {
        private readonly IStoreSetAppService _storeSetAppService;
        public BonusManagementController(IStoreSetAppService storeSetAppService) {
            _storeSetAppService = storeSetAppService;
        }
        public async Task<IActionResult> Settings()
        {
            var bonusSettingData = await _storeSetAppService.getAll(new List<long> { 6 });


            //response.StoreSetGroupId = 6;
            //ViewData["Title"] = "BonusSettings";
            //return View("~/Views/SystemManagement/SEO.cshtml", response);

            SettingsModel model = new SettingsModel();
            return View(model);
        }
        public IActionResult Transaction()
        {
            return View();
        }
        public IActionResult Record()
        {
            return View();
        }
    }
}
