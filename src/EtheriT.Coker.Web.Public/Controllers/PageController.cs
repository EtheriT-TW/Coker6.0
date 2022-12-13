using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class PageController : Controller
    {
        private readonly ILogger<PageController> _logger;
        private readonly IFreightAppService freightAppService;
        private readonly IConfiguration Configuration;

        public PageController(
            ILogger<PageController> logger,
            IFreightAppService freightAppService,
            IConfiguration Configuration
            )
        {
            this._logger = logger;
            this.freightAppService = freightAppService;
            this.Configuration = Configuration;
        }
        public async Task<IActionResult> IndexAsync(string key, int id, string search)
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var freight = JsonConvert.DeserializeObject<List<FreightDisplayDto>>(JsonConvert.SerializeObject((await freightAppService.GetDisplay(siteId)).Value));
            PageViewModel model = new PageViewModel
            {
                id = id,
                search = search ?? "".Trim(),
                freightModels = freight
            };
            string view = string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                if (key == "Search" || key == "ShoppingCar" || key == "Favorites" || key == "Contact" || key == "Catalog" || key == "ExhibitionCenter")
                {
                    view = key;
                }
                else
                {
                    view = "Product";
                    if (id != 0) view = "ProductContent";
                }
            }
            else
            {
                view = "index";
            }
            return View(view, model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
