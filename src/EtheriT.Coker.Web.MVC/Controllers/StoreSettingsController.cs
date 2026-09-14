using EtheriT.Coker.Application.Shared.Currency;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Web.MVC.Models.StoreSettings;
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
            return View("Index", response);
        }
        public async Task<IActionResult> PaymentSettings()
        {
            var response = await _thirdPartyAppService.GetAllThirdParty(ThirdPartyServiceTypeEnum.Payment);
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
            ViewData["PageHeading"] = "信件伺服器設定";
            return View("~/Views/SystemManagement/SEO.cshtml", response);
        }
        public IActionResult BonusSettings() {
            return View();
        }
        public async Task<IActionResult> LogisticsSettings()
        {
            var response = await _thirdPartyAppService.GetAllThirdParty(ThirdPartyServiceTypeEnum.Logistics);
            ViewData["Title"] = "LogisticsSettings";
            return View("LogisticsSettings", response);
        }
        public async Task<IActionResult> MarketingSettings()
        {
            var priceCurrencySetting = await _storeSetAppService.getValues(new StoreSetGetValueInput { key = "priceCurrency" });
            var priceCurrency = CurrencyCatalog.Resolve(priceCurrencySetting.detailItem?.value?.FirstOrDefault());

            return View(new MarketingSettingsModel
            {
                CurrencySymbol = priceCurrency.Symbol,
                PriceDecimalDigits = priceCurrency.DecimalDigits
            });
        }
        public IActionResult LogisticsBox() {
            return View();
        }
    }
}
