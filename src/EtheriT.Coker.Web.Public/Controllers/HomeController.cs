using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Remote;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Web.Public.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IProductAppService productAppService;
        private readonly IConfiguration Configuration;
        private readonly IWebsiteApplication websiteApplication;
        private readonly IWebMenuApplication webMenuApplication;
        private readonly IStoreSetAppService storeSetAppService;
        private readonly IHttpContextAccessor httpContextAccessor;
		private readonly IRemoteAppService RemoteAppService;
		public HomeController(
            ILogger<HomeController> logger,
            IHtmlContentAppService htmlContentAppService,
            IProductAppService productAppService,
            IConfiguration Configuration,
            IWebsiteApplication websiteApplication,
            IWebMenuApplication webMenuApplication,
            IStoreSetAppService storeSetAppService,
            IHttpContextAccessor httpContextAccessor,
			IRemoteAppService RemoteAppService
			)
        {
            this._logger = logger;
            this.htmlContentAppService = htmlContentAppService;
            this.productAppService = productAppService;
            this.Configuration = Configuration;
            this.websiteApplication = websiteApplication;
            this.webMenuApplication = webMenuApplication;
            this.storeSetAppService = storeSetAppService;
            this.httpContextAccessor= httpContextAccessor;
            this.RemoteAppService = RemoteAppService;
        }

        public async Task<IActionResult> IndexAsync(string key)
        {
            string view;
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var defaultData = await websiteApplication.GetDefaultData(siteId, key);
            var site_name = $"Layout_{defaultData.Id}_Site";
            var enterAds = JsonConvert.DeserializeObject<List<HtmlContentDisplayDto>>(JsonConvert.SerializeObject((await htmlContentAppService.GetDisplay(defaultData.Id, 8, 1)).Value));
            await webMenuApplication.CheckDisplayAll(siteId);
            if (defaultData.Id != siteId) foreach (var enterAd in enterAds) for (var i = 0; i < enterAd.Img.Count; i++) if (enterAd.Img[i] != null) enterAd.Img[i] = enterAd.Img[i].Replace("upload", $"upload/{defaultData.OrgName}");
            var guessLike = JsonConvert.DeserializeObject<List<ProdGetDisplayDto>>(JsonConvert.SerializeObject((await productAppService.GetRandomDIsplay(defaultData.Id, 3)).Value));
            var storeSet = await storeSetAppService.getValues(new StoreSetGetValueInput {key= "ga4",SiteId= siteId });
            await RemoteAppService.insertRemote(new Application.Shared.Dto.Remote.RemoteInputDto
            {
                FK_WebsiteId = siteId,
                FK_WebmenuId = defaultData.Id
			});
			HomeViewModel model = new HomeViewModel
            {
                site_name = site_name,
                OrgName= defaultData.OrgName,
				enterAd = enterAds,
                guessLike = guessLike,
                layout = $"layput{defaultData.Layout_Type}",
                Level = defaultData.Level,
                locale = defaultData.locale,
				token = httpContextAccessor.HttpContext.Request.Cookies["XSRF-TOKEN"],
                storeSet = new StoreSetFrontDto
                {
                    GA4 = (storeSet.Success && storeSet != null && storeSet.detailItem != null) ? storeSet.detailItem.value ?? "" : ""
                }
            };
            model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = "home", siteId = defaultData.Id });
            model.PageData.LayoutType = defaultData.Layout_Type;

            if (string.IsNullOrEmpty(model.PageData.Html)|| (key!=null && key != defaultData.OrgName))
            {
                Response.StatusCode = 404;
                view = "/Page/Error/404";
            }
            else
            {
                if (string.IsNullOrEmpty(model.PageData.Description))
                {
                    string htmlString = HttpUtility.HtmlDecode(model.PageData.Html);
                    model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                }
                if (key != null)
                {
                    model.PageData.Html = Regex.Replace(model.PageData.Html, $"src=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"src=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                    model.PageData.Html = Regex.Replace(model.PageData.Html, $"href=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"href=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                    model.PageData.Html = Regex.Replace(model.PageData.Html, $"href=&quot;(?!(http|/))", $"href=&quot;/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                    model.PageData.Css = model.PageData.Css.Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                }

                view = "Index";
            }

            return View(view, model);
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