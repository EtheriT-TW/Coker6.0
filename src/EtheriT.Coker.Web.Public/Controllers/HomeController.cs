using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.HtmlContent;
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
        private readonly IConfiguration Configuration;

        public HomeController(
            ILogger<HomeController> logger,
            IHtmlContentAppService htmlContentAppService,
            IConfiguration Configuration
            )
        {
            this._logger = logger;
            this.htmlContentAppService = htmlContentAppService;
            this.Configuration = Configuration;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var enterAd = JsonConvert.DeserializeObject<List<HtmlContentDisplayDto>>(JsonConvert.SerializeObject((await htmlContentAppService.GetDisplay(siteId, 8, 1)).Value));
            HomeViewModel model = new HomeViewModel
            {
                enterAd = enterAd
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