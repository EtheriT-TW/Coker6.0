using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IArticleAppService articleAppService;
        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IProductAppService productAppService;
        public PageController(
            ILogger<PageController> logger,
            IFreightAppService freightAppService,
            IWebMenuApplication webMenuApplication,
            IConfiguration configuration,
            IWebsiteApplication websiteApplication,
            IArticleAppService articleAppService,
            IHtmlContentAppService htmlContentAppService,
            IProductAppService productAppService
        )
        {
            this._logger = logger;
            this.freightAppService = freightAppService;
            this.webMenuApplication = webMenuApplication;
            this.Configuration = configuration;
            this.websiteApplication = websiteApplication;
            this.articleAppService = articleAppService;
            this.htmlContentAppService = htmlContentAppService;
            this.productAppService = productAppService;

        }
        public async Task<IActionResult> IndexAsync(string website, string key, string option, int id, string search)
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var defaultData = await websiteApplication.GetDefaultData(siteId, website);

            var freight = JsonConvert.DeserializeObject<List<FreightDisplayDto>>(JsonConvert.SerializeObject((await freightAppService.GetDisplay()).Value));
            var enterAds = JsonConvert.DeserializeObject<List<HtmlContentDisplayDto>>(JsonConvert.SerializeObject((await htmlContentAppService.GetDisplay(defaultData.Id, 8, 1)).Value));
            if (defaultData.Id != siteId) foreach (var enterAd in enterAds) for (var i = 0; i < enterAd.Img.Count; i++) if (enterAd.Img[i] != null) enterAd.Img[i] = enterAd.Img[i].Replace("upload", $"upload/{defaultData.OrgName}");
            PageViewModel model = new PageViewModel
            {
                id = id,
                search = search ?? "".Trim(),
                freightModels = freight,
                enterAd = enterAds,
            };
            string view;
            Console.WriteLine(option);
            if (!string.IsNullOrEmpty(key))
            {
                switch (option)
                {
                    case "article":
                        model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                        model.MenuBread = await webMenuApplication.GetMenuBread(model.PageData.Id);
                        model.PageData = await articleAppService.GetFrontConten(new ArticleGetFrontContenInputDto { siteId = defaultData.Id, articleId = id });
                        model.PageData.LayoutType = defaultData.Layout_Type;
                        model.PageData.holdPage = Application.Shared.Dto.enumType.HoldPageNameEnum.Article;


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
                                if (siteId != defaultData.Id)
                                {
                                    model.PageData.Html = model.PageData.Html.Replace("src=&quot;/upload/", $"src=&quot;/upload/{defaultData.OrgName}/");
                                    model.PageData.Html = model.PageData.Html.Replace("href=&quot;/upload/", $"href=&quot;/upload/{defaultData.OrgName}/");
                                    model.PageData.Css = (model.PageData.Css??"").Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                                }
                            }
                            view = "Index";
                        }
                        break;
                    case "product":
                        view = "Product";
                        if (id != 0)
                        {
                            model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            model.MenuBread = await webMenuApplication.GetMenuBread(model.PageData.Id);
                            model.PageData = await productAppService.GetFrontConten(new ProdGetFrontContenInputDto { siteId = defaultData.Id, prodId = id });
                            model.MenuBread.Add(new GetMenuBreadDto
                            {
                                Title = model.PageData.Title,
                                Link = "",
                            });
                            model.PageData.LayoutType = defaultData.Layout_Type;

                            if (!string.IsNullOrEmpty(model.PageData.Html) && string.IsNullOrEmpty(model.PageData.Description))
                            {
                                string htmlString = HttpUtility.HtmlDecode(model.PageData.Html);
                                model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                                if (siteId != defaultData.Id)
                                {
                                    model.PageData.Html = model.PageData.Html.Replace("src=&quot;/upload/", $"src=&quot;/upload/{defaultData.OrgName}/");
                                    model.PageData.Html = model.PageData.Html.Replace("href=&quot;/upload/", $"href=&quot;/upload/{defaultData.OrgName}/");
                                    model.PageData.Css = model.PageData.Css.Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                                }
                            }
                            view = "ProductContent";
                        }
                        break;
                    default:
                        if (key == "Search" || key == "ShoppingCar" || key == "Favorites" || key == "Contact" || key == "Catalog" || key == "ExhibitionCenter" || key == "Terms" || key == "Test" || key == "ColumnarSearch")
                        {
                            view = key;
                        }
                        else
                        {
                            model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            model.MenuBread = await webMenuApplication.GetMenuBread(model.PageData.Id);
                            model.PageData.LayoutType = defaultData.Layout_Type;

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
                                if (siteId != defaultData.Id)
                                {
                                    model.PageData.Html = model.PageData.Html.Replace("src=&quot;/upload/", $"src=&quot;/upload/{defaultData.OrgName}/");
                                    model.PageData.Html = model.PageData.Html.Replace("href=&quot;/upload/", $"href=&quot;/upload/{defaultData.OrgName}/");
                                    model.PageData.Css = model.PageData.Css.Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                                }

                                view = "Index";
                            }
                        }
                        break;
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
