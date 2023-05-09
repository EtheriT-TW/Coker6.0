using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Web.Public.Models;
using EtheriT.Coker.Web.Public.Views.Shared.Components.Footer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class PageController : Controller
    {
        private readonly ILogger<PageController> _logger;
        private readonly IFreightAppService freightAppService;
        private readonly IWebMenuApplication webMenuApplication;
        private readonly IConfiguration Configuration;
        private readonly IWebsiteApplication websiteApplication;
        public PageController(
            ILogger<PageController> logger,
            IFreightAppService freightAppService,
            IWebMenuApplication webMenuApplication,
            IConfiguration configuration,
            IWebsiteApplication websiteApplication
        )
        {
            this._logger = logger;
            this.freightAppService = freightAppService;
            this.webMenuApplication = webMenuApplication;
            this.Configuration = configuration;
            this.websiteApplication = websiteApplication;
        }
        public async Task<IActionResult> IndexAsync(string website, string key, string option, int id, string search)
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var defaultData = await websiteApplication.GetDefaultData(siteId, website);

            var freight = JsonConvert.DeserializeObject<List<FreightDisplayDto>>(JsonConvert.SerializeObject((await freightAppService.GetDisplay()).Value));
            PageViewModel model = new PageViewModel
            {
                id = id,
                search = search ?? "".Trim(),
                freightModels = freight
            };
            switch (option)
            {
                case "":
                    break;
                case "prod":
                    break;
            }
            string view;
            if (!string.IsNullOrEmpty(key))
            {
                if (key == "Search" || key == "ShoppingCar" || key == "Favorites" || key == "Contact" || key == "Catalog" || key == "ExhibitionCenter" || key == "Terms" || key == "Test")
                {
                    view = key;
                }
                else if (key == "Toilet")
                {
                    view = "Product";
                    if (id != 0) view = "ProductContent";
                }
                else
                {
                    model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });

                    if (string.IsNullOrEmpty(model.PageData.Html))
                    {
                        Response.StatusCode = 404;
                        view = "Error/404";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.PageData.Description))
                        {
                            string htmlString = HttpUtility.HtmlDecode(model.PageData.Html);
                            model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                        }
                        view = "Index";
                    }
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
