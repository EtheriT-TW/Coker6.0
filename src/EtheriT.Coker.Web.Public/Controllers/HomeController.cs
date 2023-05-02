using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IProductAppService productAppService;
        private readonly IConfiguration Configuration;
        private readonly IWebsiteApplication websiteApplication;

        public HomeController(
            ILogger<HomeController> logger,
            IHtmlContentAppService htmlContentAppService,
            IProductAppService productAppService,
            IConfiguration Configuration,
            IWebsiteApplication websiteApplication
            )
        {
            this._logger = logger;
            this.htmlContentAppService = htmlContentAppService;
            this.productAppService = productAppService;
            this.Configuration = Configuration;
            this.websiteApplication = websiteApplication;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var site_name = $"Layout_{await websiteApplication.GetLayoutType(siteId)}_Site";
            var enterAd = JsonConvert.DeserializeObject<List<HtmlContentDisplayDto>>(JsonConvert.SerializeObject((await htmlContentAppService.GetDisplay(siteId, 8, 1)).Value));
            var guessLike = JsonConvert.DeserializeObject<List<ProdGetDisplayDto>>(JsonConvert.SerializeObject((await productAppService.GetRandomDIsplay(siteId, 3)).Value));
            HomeViewModel model = new HomeViewModel
            {
                site_name = site_name,
                enterAd = enterAd,
                guessLike = guessLike,
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}