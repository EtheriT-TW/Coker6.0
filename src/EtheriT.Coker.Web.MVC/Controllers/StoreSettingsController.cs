using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.StoreSet;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class StoreSettingsController : Controller
    {
        private readonly IStoreSetAppService _storeSetAppService;
        private readonly IThirdPartyAppService _thirdPartyAppService;
        public StoreSettingsController(IStoreSetAppService storeSetAppService, IThirdPartyAppService thirdPartyAppService) {
            _storeSetAppService = storeSetAppService;
            _thirdPartyAppService = thirdPartyAppService;
        }
        public async Task<IActionResult> Index()
        {
            var response = await _storeSetAppService.getAll(new List<long> { 2 });
            response.StoreSetGroupId = 2;
            ViewData["Title"] = "StoreSet";
            return View("~/Views/SystemManagement/SEO.cshtml", response);
        }
        public async Task<IActionResult> PaymentSettings()
        {
            var response = await _thirdPartyAppService.GetAllThirdParty();
            ViewData["Title"] = "PaymentSettings";
            return View("PaymentSettings", response);
        }
        public IActionResult FreightSettings()
        {
            return View("FreightSettings");
        }
        public async Task<IActionResult> SMTPSettings()
        {
            var response = await _storeSetAppService.getAll(new List<long> { 3 });
            response.StoreSetGroupId = 3;
            ViewData["Title"] = "SMTPServer";
            return View("~/Views/SystemManagement/SEO.cshtml", response);
        }
        public IActionResult BonusSettings() {
            return View();
        }
    }
}
