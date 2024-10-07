using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Remote;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.NetworkInformation;
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
        private readonly IAdvertiseAppService advertiseAppService;
        private readonly ITokenAppService tokenAppService;
        public HomeController(
            ILogger<HomeController> logger,
            IHtmlContentAppService htmlContentAppService,
            IProductAppService productAppService,
            IConfiguration Configuration,
            IWebsiteApplication websiteApplication,
            IWebMenuApplication webMenuApplication,
            IStoreSetAppService storeSetAppService,
            IHttpContextAccessor httpContextAccessor,
            IRemoteAppService RemoteAppService,
            IAdvertiseAppService advertiseAppService,
            ITokenAppService tokenAppService
        )
        {
            this._logger = logger;
            this.htmlContentAppService = htmlContentAppService;
            this.productAppService = productAppService;
            this.Configuration = Configuration;
            this.websiteApplication = websiteApplication;
            this.webMenuApplication = webMenuApplication;
            this.storeSetAppService = storeSetAppService;
            this.httpContextAccessor = httpContextAccessor;
            this.RemoteAppService = RemoteAppService;
            this.tokenAppService = tokenAppService;
            this.advertiseAppService = advertiseAppService;
        }

        public async Task<IActionResult> IndexAsync(string key)
        {
            string view;
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var defaultData = await websiteApplication.GetDefaultData(siteId, key);
            var site_name = $"Layout_{defaultData.Id}_Site";
            var enterAds = JsonConvert.DeserializeObject<List<AdvertiseDisplayDto>>(JsonConvert.SerializeObject((await advertiseAppService.GetDisplay(defaultData.Id, 1, 1)).Value));
            await webMenuApplication.CheckDisplayAll(siteId);
            if (defaultData.Id != siteId) foreach (var enterAd in enterAds) for (var i = 0; i < enterAd.FileLink.Count; i++) if (enterAd.FileLink[i].Link != null) enterAd.FileLink[i].Link = enterAd.FileLink[i].Link.Replace("upload", $"upload/{defaultData.OrgName}");
            var guessLike = JsonConvert.DeserializeObject<List<ProdGetDisplayDto>>(JsonConvert.SerializeObject((await productAppService.GetRandomDIsplay(defaultData.Id, 3)).Value));
            var SEO = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 1, SiteId = siteId });
            var GA4 = SEO.storeSetDetails?.Find(e => e.key == "GA4");
            var GoogleTranslate = SEO.storeSetDetails?.Find(e => e.key == "google.translate");
            var GTM = SEO.storeSetDetails?.Find(e => e.key == "GTM");
            ViewBag.HasShoppingCar = await webMenuApplication.checkHasShoppingCar(siteId);
			ViewBag.LoginEnable = await webMenuApplication.checkHasMember(siteId);

            var StoreSet = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 2, SiteId = siteId });
            var storeBuyState = StoreSet.storeSetDetails?.Find(e => e.key == "storeBuyState");
            var storeMemo = StoreSet.storeSetDetails?.Find(e => e.key == "storeMemo");
            var linkMore = StoreSet.storeSetDetails?.Find(e => e.key == "linkMore");

            HomeViewModel model = new HomeViewModel
            {
                site_name = site_name,
                OrgName = defaultData.OrgName,
                enterAd = enterAds,
                //guessLike = guessLike,
                layout = $"layput{defaultData.Layout_Type}",
                Level = defaultData.Level,
                locale = defaultData.locale,
                token = httpContextAccessor.HttpContext.Request.Cookies["XSRF-TOKEN"],
                PageView = "Home",
                storeSet = new StoreSetFrontDto
                {
                    GA4 = (GA4 != null && GA4.value != null) ? String.Join(",", GA4.value!) : "",
                    GoogleTranslate = (GoogleTranslate != null && GoogleTranslate.value != null) ? String.Join(",", GoogleTranslate.value!) : "",
                    GTM = (GTM != null && GTM.value != null) ? String.Join(",", GTM.value!) : "",
                    storeBuyState = (storeBuyState != null && storeBuyState.value != null) ? String.Join(",", storeBuyState.value!) : "",
                    storeMemo = (GA4 != null && storeMemo.value != null) ? String.Join(",", storeMemo.value!) : "",
                    linkMore = (linkMore != null && linkMore.value != null) ? String.Join(",", linkMore.value!) : ""
                }
            };
            ViewBag.isLogin = false;
            Guid guid = new Guid();
            var t = httpContextAccessor.HttpContext.Request.Cookies["Token"];
            if (ViewBag.LoginEnable)
            {
                if (!string.IsNullOrEmpty(t) && Guid.TryParse(t, out guid))
                {
                    var tokenItem = await tokenAppService.CheckToken(guid);
                    if (tokenItem.Success)
                    {
                        ViewBag.isLogin = tokenItem.IsLogin;
                        httpContextAccessor.HttpContext.Response.Cookies.Append("Token", tokenItem.Token, new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddMinutes(15) // 只需設定過期時間即可
                        });
                    }
                }
                else {
                    var token = await tokenAppService.CreateToken();
                    if (token.Success) {
						httpContextAccessor.HttpContext.Response.Cookies.Append("Token", token.Token, new CookieOptions
						{
							Expires = DateTimeOffset.UtcNow.AddMinutes(15) // 只需設定過期時間即可
						});
					}
				}
            }
            model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = "home", siteId = defaultData.Id });
            model.PageData.LayoutType = defaultData.Layout_Type;
            var saveRemote = await RemoteAppService.insertRemote(new Application.Shared.Dto.Remote.RemoteInputDto
            {
                FK_WebsiteId = siteId,
                FK_WebmenuId = model.PageData.Id
            });
            if (!string.IsNullOrEmpty(defaultData.Description)) model.PageData.Description = defaultData.Description;
            if (string.IsNullOrEmpty(model.PageData.Html) || (key != null && key != defaultData.OrgName))
            {
                Response.StatusCode = 404;
                return View("../Error/NotFound");
            }
            else
            {
                if (string.IsNullOrEmpty(model.PageData.Description))
                {
                    string htmlString = HttpUtility.HtmlDecode(model.PageData.Html);
                    model.PageData.Description = Regex.Replace(htmlString, @"(<(.|\n)*?>|\s|navigate_before|navigate_next)", "");
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
            ViewData["SideName"] = model.PageData!.SiteName;
            ViewData["PageName"] = model.PageData.Title;
            ViewData["OrgName"] = model.OrgName;
            ViewData["Layout"] = model.layout;
            ViewData["PageTagNameName"] = $"{model.PageData.Title} - 【{model.PageData.SiteName}】";
            ViewData["Description"] = model.PageData.Description;
            ViewData["GA4"] = model.storeSet.GA4;
            ViewData["GTM"] = model.storeSet.GTM;
            ViewData["google.translate"] = model.storeSet.GoogleTranslate;
            ViewData["CurrentUrl"] = model.PageData.CurrentUrl;
            ViewData["Root"] = defaultData.Root;
            ViewData["VisibleHeader"] = model.PageData.VisibleHeader;
            ViewData["VisibleFooter"] = model.PageData.VisibleFooter;
            ViewData["XSRF-TOKEN"] = model.token;
            ViewData["Locale"] = model.locale;
            ViewData["PageView"] = model.PageData.PageView;
            ViewData["Id"] = model.PageData.Id;
            ViewData["bodyClass"] = "home";
            var nonce = HttpContext.Items["CSPNonce"] as string;
            ViewBag.Nonce = nonce;
            ViewBag.storeBuyState = model.storeSet.storeBuyState;
            switch (model.Level)
            {
                case WebsiteLevelEnum.會員:
                    ViewBag.ShoppingEnable = false;
                    break;
                case WebsiteLevelEnum.購物:
                    ViewBag.ShoppingEnable = true;
                    break;
                default:
					ViewBag.LoginEnable = false;
                    ViewBag.ShoppingEnable = false;
                    break;
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