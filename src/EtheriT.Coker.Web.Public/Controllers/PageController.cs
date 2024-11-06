using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.i18n;
using System.Web;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Token;
using System.Linq;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;

namespace EtheriT.Coker.Web.Public.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PageController : Controller
    {
        private readonly ILogger<PageController> _logger;
        private readonly IFreightAppService freightAppService;
        private readonly IThirdPartyAppService thirdPartyAppService;
        private readonly IWebMenuApplication webMenuApplication;
        private readonly IConfiguration Configuration;
        private readonly IWebsiteApplication websiteApplication;
        private readonly IArticleAppService articleAppService;
        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IProductAppService productAppService;
        private readonly ICustSearchAppService custSearchAppService;
        private readonly IStoreSetAppService storeSetAppService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRemoteAppService RemoteAppService;
        private readonly ITechnicalCertificateAppService technicalCertificateAppService;
        private readonly ITokenAppService tokenAppService;
        private readonly IAdvertiseAppService advertiseAppService;
        private readonly StringHandler stringHandler;
        public PageController(
            ILogger<PageController> logger,
            IFreightAppService freightAppService,
            IThirdPartyAppService thirdpartyAppService,
            IWebMenuApplication webMenuApplication,
            IConfiguration configuration,
            IWebsiteApplication websiteApplication,
            IArticleAppService articleAppService,
            IHtmlContentAppService htmlContentAppService,
            IProductAppService productAppService,
            IStoreSetAppService storeSetAppService,
            ICustSearchAppService custSearchAppService,
            IHttpContextAccessor httpContextAccessor,
            IRemoteAppService RemoteAppService,
            ITechnicalCertificateAppService technicalCertificateAppService,
            IAdvertiseAppService advertiseAppService,
            ITokenAppService tokenAppService,
            StringHandler stringHandler
        )
        {
            this._logger = logger;
            this.freightAppService = freightAppService;
            this.thirdPartyAppService = thirdpartyAppService;
            this.webMenuApplication = webMenuApplication;
            this.Configuration = configuration;
            this.websiteApplication = websiteApplication;
            this.articleAppService = articleAppService;
            this.htmlContentAppService = htmlContentAppService;
            this.productAppService = productAppService;
            this.stringHandler = stringHandler;
            this.storeSetAppService = storeSetAppService;
            this.custSearchAppService = custSearchAppService;
            this.httpContextAccessor = httpContextAccessor;
            this.RemoteAppService = RemoteAppService;
            this.technicalCertificateAppService = technicalCertificateAppService;
            this.tokenAppService = tokenAppService;
            this.advertiseAppService = advertiseAppService;
        }
        private bool UseLegacyPathHandling(string website, string key, string option)
        {
            bool check = true;
            if (
                (!string.IsNullOrEmpty(website) && (website.IndexOf("..") >= 0 || website.IndexOf("//") >= 0)) ||
                (!string.IsNullOrEmpty(key) && (key.IndexOf("..") >= 0 || key.IndexOf("//") >= 0)) ||
                (!string.IsNullOrEmpty(option) && (option.IndexOf("..") >= 0 || option.IndexOf("//") >= 0))
            )
            {
                check = false;
            }
            return check;
        }

        public async Task<IActionResult> IndexAsync(string website, string key, string option, int id, string search)
        {
            if (string.IsNullOrEmpty(key)) key = "home";
            else if (string.IsNullOrEmpty(website) && !string.IsNullOrEmpty(key))
            {
                website = key;
                key = "home";
            }
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var defaultData = await websiteApplication.GetDefaultData(siteId, website);
            var freight = JsonConvert.DeserializeObject<List<FreightDisplayDto>>(JsonConvert.SerializeObject((await freightAppService.GetDisplay()).Value));
            var payment = JsonConvert.DeserializeObject<List<PaymentTypeItemOutputDto>>(JsonConvert.SerializeObject((await thirdPartyAppService.GetDisplayPayment()).Value));
            var enterAds = JsonConvert.DeserializeObject<List<AdvertiseDisplayDto>>(JsonConvert.SerializeObject((await advertiseAppService.GetDisplay(defaultData.Id, 1, 1)).Value));
            var SEO = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 1, SiteId = siteId });
            var GA4 = SEO.storeSetDetails?.Find(e => e.key == "GA4");
            var GoogleTranslate = SEO.storeSetDetails?.Find(e => e.key == "google.translate");
            var GTM = SEO.storeSetDetails?.Find(e => e.key == "GTM");

            var StoreSet = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 2, SiteId = siteId });
            var storeBuyState = StoreSet.storeSetDetails?.Find(e => e.key == "storeBuyState");
            var storeMemo = StoreSet.storeSetDetails?.Find(e => e.key == "storeMemo");
            var linkMore = StoreSet.storeSetDetails?.Find(e => e.key == "linkMore");

            RemoteInputDto remoteInputDto = new RemoteInputDto { FK_WebsiteId = siteId };
            if (defaultData.Id != siteId) foreach (var enterAd in enterAds) for (var i = 0; i < enterAd.FileLink.Count; i++) if (enterAd.FileLink[i].Link != null) enterAd.FileLink[i].Link = enterAd.FileLink[i].Link.Replace("upload", $"upload/{defaultData.OrgName}");
            PageViewModel model = new PageViewModel
            {
                id = id,
                orgName = defaultData.OrgName,
                search = search ?? "".Trim(),
                freightModels = freight,
                paymentModels = payment,
                enterAd = enterAds,
                layout = $"layout{defaultData.Layout_Type}",
                root = defaultData.Root,
                Level = defaultData.Level,
                locale = defaultData.locale,
                token = httpContextAccessor.HttpContext.Request.Cookies["XSRF-TOKEN"],
                storeSet = new StoreSetFrontDto
                {
                    GA4 = (GA4 != null && GA4.value != null) ? String.Join(",", GA4.value!) : "",
                    GoogleTranslate = (GoogleTranslate != null && GoogleTranslate.value != null) ? String.Join(",", GoogleTranslate.value!) : "",
                    GTM = (GTM != null && GTM.value != null) ? String.Join(",", GTM.value!) : "",
                    storeBuyState = (storeBuyState != null && storeBuyState.value != null) ? String.Join(",", storeBuyState.value!) : "",
                    storeMemo = (GA4 != null && storeMemo != null && storeMemo.value != null) ? String.Join(",", storeMemo.value!) : "",
                    linkMore = (linkMore != null && linkMore.value != null) ? String.Join(",", linkMore.value!) : ""
                }
            };
            string view;
            if (new List<string> { "article" }.Contains(key.ToLower()) && int.TryParse(option, out id))
            {
                option = key;
            }
            model.option = key;
            if (!UseLegacyPathHandling(website, key, option))
            {
                model.PageData = new GetFrontContenOutputDto { SiteName = L.get("PathError") };
                Response.StatusCode = 404;
                view = "../Error/NotFound";
            }
            else if (!string.IsNullOrEmpty(key))
            {
                if (string.IsNullOrEmpty(option)) option = "";
                switch (option.ToLower())
                {
                    case "article":
                        var PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                        remoteInputDto.FK_WebmenuId = PageData.Id;
                        model.MenuBread = await webMenuApplication.GetMenuBread(PageData.Id);
                        model.PageData = await articleAppService.GetFrontConten(new ArticleGetFrontContenInputDto { siteId = defaultData.Id, articleId = id });
                        remoteInputDto.FK_ArticleId = model.PageData.Id;
                        model.ParentData = PageData;
                        model.PageData.PageView = "Article";
                        model.PageData.LayoutType = defaultData.Layout_Type;
                        model.PageData.holdPage = Application.Shared.Dto.enumType.HoldPageNameEnum.Article;
                        if (key == "article")
                        {
                            model.PageData.VisibleHeader = true;
                            model.PageData.VisibleFooter = true;
                            model.PageData.VisibleTitle = true;
                        }
                        else
                        {
                            model.PageData.VisibleHeader = PageData.VisibleHeader;
                            model.PageData.VisibleFooter = PageData.VisibleFooter;
                            model.PageData.VisibleTitle = PageData.VisibleTitle;
                        }

                        if (string.IsNullOrEmpty(model.PageData.Html))
                        {
                            Response.StatusCode = 404;
                            view = "../Error/NotFound";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(model.PageData.Description))
                            {
                                string htmlString = stringHandler.HtmlDecode(model.PageData.Html);
                                model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                            }
                            view = "Index";
                        }
                        break;
                    case "product":
                        ViewData["linkMore"] = model.storeSet.linkMore;
                        if (id != 0)
                        {
                            var ProdPageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            remoteInputDto.FK_WebmenuId = ProdPageData.Id;
                            model.MenuBread = await webMenuApplication.GetMenuBread(ProdPageData.Id);
                            model.PageData = await productAppService.GetFrontConten(new ProdGetFrontContenInputDto { siteId = defaultData.Id, prodId = id });
                            if (model.PageData.Id == 0) view = "../Error/NotFound";
                            else
                            {
                                remoteInputDto.FK_ProdId = model.PageData.Id;
                                model.ParentData = ProdPageData;
                                model.PageData.PageView = "Product";
                                model.PageData.LayoutType = defaultData.Layout_Type;
                                model.PageData.holdPage = HoldPageNameEnum.Article;
                                if (key == "product")
                                {
                                    model.PageData.VisibleHeader = true;
                                    model.PageData.VisibleFooter = true;
                                    model.PageData.VisibleTitle = true;
                                }
                                else
                                {
                                    model.PageData.VisibleHeader = ProdPageData.VisibleHeader;
                                    model.PageData.VisibleFooter = ProdPageData.VisibleFooter;
                                    model.PageData.VisibleTitle = ProdPageData.VisibleTitle;
                                }

                                model.MenuBread.Add(new GetMenuBreadDto
                                {
                                    Title = model.PageData.Title,
                                    Link = "",
                                });

                                if (!string.IsNullOrEmpty(model.PageData.Html) && string.IsNullOrEmpty(model.PageData.Description))
                                {
                                    string htmlString = stringHandler.HtmlDecode(model.PageData.Html);
                                    model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                                }
                                view = "ProductContent";
                            }
                        }
                        else view = "../Error/NotFound";
                        break;
                    case "techcert":
                        var TechCertPageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                        remoteInputDto.FK_WebmenuId = TechCertPageData.Id;
                        model.MenuBread = await webMenuApplication.GetMenuBread(TechCertPageData.Id);
                        model.PageData = await technicalCertificateAppService.GetFrontConten(new TechCertGetFrontContenInputDto { siteId = defaultData.Id, TechCertId = id });
                        remoteInputDto.FK_TechCertId = model.PageData.Id;
                        model.ParentData = TechCertPageData;
                        model.PageData.PageView = "Techcert";
                        model.PageData.LayoutType = defaultData.Layout_Type;
                        model.PageData.holdPage = Application.Shared.Dto.enumType.HoldPageNameEnum.TechCert;
                        if (key.ToLower() == "techvert")
                        {
                            model.PageData.VisibleHeader = true;
                            model.PageData.VisibleFooter = true;
                            model.PageData.VisibleTitle = true;
                        }
                        else
                        {
                            model.PageData.VisibleHeader = TechCertPageData.VisibleHeader;
                            model.PageData.VisibleFooter = TechCertPageData.VisibleFooter;
                            model.PageData.VisibleTitle = TechCertPageData.VisibleTitle;
                        }

                        if (string.IsNullOrEmpty(model.PageData.Html))
                        {
                            Response.StatusCode = 404;
                            view = "../Error/NotFound";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(model.PageData.Description))
                            {
                                string htmlString = stringHandler.HtmlDecode(model.PageData.Html);
                                model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                            }
                            view = "Index";
                        }
                        break;
                    case "privacy":
                        model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                        remoteInputDto.FK_WebmenuId = model.PageData.Id;
                        view = "Index";
                        break;
                    default:
                        if (key.ToLower() == "search")
                        {
                            model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            model.PageData.PageView = "Search";
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            model.PageData.Title = L.get("SiteSearch");
                            model.SearchPalameter = new FrontSearchPalameterDro
                            {
                                SearchId = id,
                                SearchText = search ?? "",
                                Class = await custSearchAppService.GetSearchList(defaultData.Id)
                            };
                            if (model.SearchPalameter.Class.Exists(e => e.Id == 3) && model.SearchPalameter.SearchId == 0 && string.IsNullOrEmpty(option))
                                model.SearchPalameter.SearchId = 3;
                            view = "CustSearch";
                            int c;
                            int.TryParse(model.layout.Replace("layout", ""), out c);
                            if (c != 0) model.PageData.LayoutType = c;
                        }
                        else if (key.ToLower() == "shoppingcar")
                        {
                            ViewData["storeMemo"] = model.storeSet.storeMemo;
                            model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            view = "ShoppingCar";
                        }
                        else if (key.ToLower() == "member")
                        {
                            model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            view = "Member";
                        }
                        else if (key.ToLower() == "demosearch")
                        {
                            model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            model.PageData.Title = L.get("SiteSearch");
                            model.SearchPalameter = new FrontSearchPalameterDro
                            {
                                SearchId = id,
                                SearchText = search ?? "",
                                Class = await custSearchAppService.GetSearchList(defaultData.Id)
                            };
                            view = "Search";
                            int c;
                            int.TryParse(model.layout.Replace("layout", ""), out c);
                            if (c != 0) model.PageData.LayoutType = c;
                        }
                        else if (key == "ProductDemo" || key == "Favorites" || key == "Catalog" || key == "ExhibitionCenter" || key == "Terms" || key == "ColumnarSearch")
                        {
                            model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            view = key;
                        }
                        else
                        {
                            model.PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            model.ParentData = await webMenuApplication.GetParentConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                            model.MenuBread = await webMenuApplication.GetMenuBread(model.PageData.Id);
                            model.PageData.LayoutType = defaultData.Layout_Type;
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;

                            if (string.IsNullOrEmpty(model.PageData.Html))
                            {
                                Response.StatusCode = 404;
                                view = "../Error/NotFound";
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(model.PageData.Description))
                                {
                                    string htmlString = stringHandler.HtmlDecode(model.PageData.Html);
                                    model.PageData.Description = Regex.Replace(htmlString, @"(<(.|\n)*?>|\s)", "");
                                }
                                view = "Index";
                            }
                        }
                        break;
                }
                if (key.ToLower() == "search")
                {
                    model.PageData.VisibleHeader = true;
                    model.PageData.VisibleFooter = true;
                    model.PageData.VisibleTitle = true;
                }
                if (view.IndexOf("Error/") < 0)
                {
                    if (siteId != defaultData.Id && model.PageData != null)
                    {
                        model.PageData.Html = stringHandler.HtmlEncode(model.PageData.Html);
                        model.PageData.Html = Regex.Replace(model.PageData.Html, $"src=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"src=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                        model.PageData.Html = Regex.Replace(model.PageData.Html, $"href=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"href=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                        model.PageData.Html = Regex.Replace(model.PageData.Html, $"data-pdf-url=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"data-pdf-url=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                        model.PageData.Css = (model.PageData.Css ?? "").Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                    }
                    if (siteId != defaultData.Id && model.ParentData != null)
                    {
                        model.ParentData.Html = stringHandler.HtmlEncode(model.ParentData.Html);
                        model.ParentData.Html = model.ParentData.Html.Replace("src=&quot;/upload/", $"src=&quot;/upload/{defaultData.OrgName}/");
                        model.ParentData.Html = model.ParentData.Html.Replace("href=&quot;/upload/", $"href=&quot;/upload/{defaultData.OrgName}/");
                        model.ParentData.Html = model.ParentData.Html.Replace("data-pdf-url=&quot;/upload/", $"data-pdf-url=&quot;/upload/{defaultData.OrgName}/");
                        model.ParentData.Css = (model.ParentData.Css ?? "").Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                    }
                }
            }
            else
            {
                view = "index";
            }
            ViewBag.HasShoppingCar = await webMenuApplication.checkHasShoppingCar(siteId);
            ViewBag.LoginEnable = await webMenuApplication.checkHasMember(siteId);
            ViewBag.isLogin = false;
            try
            {
                var tokenItem = await tokenAppService.CreateToken();
                if (tokenItem != null)
                {
                    ViewBag.isLogin = tokenItem.IsLogin;
                }
                else throw new Exception();
            }
            catch
            {
                ViewBag.isLogin = false;
                ViewBag.LoginEnable = false;
            }

            await RemoteAppService.insertRemote(remoteInputDto);
            ViewData["SideName"] = model.PageData!.SiteName;
            ViewData["PageName"] = model.PageData.Title;
            ViewData["OrgName"] = model.orgName;
            ViewData["Layout"] = model.layout;
            ViewData["PageTagNameName"] = $"{model.PageData.Title} - 【{model.PageData.SiteName}】";
            ViewData["Description"] = model.PageData.Description;
            ViewData["GA4"] = model.storeSet.GA4;
            ViewData["GTM"] = model.storeSet.GTM;
            ViewData["google.translate"] = model.storeSet.GoogleTranslate;
            ViewData["CurrentUrl"] = model.PageData.CurrentUrl;
            ViewData["Root"] = model.root;
            ViewData["VisibleHeader"] = model.PageData.VisibleHeader;
            ViewData["VisibleFooter"] = model.PageData.VisibleFooter;
            ViewData["XSRF-TOKEN"] = model.token;
            ViewData["Locale"] = model.locale;
            ViewData["PageView"] = model.PageData.PageView;
            ViewData["Id"] = model.PageData.Id;
            ViewData["bodyClass"] = model.option?.ToLower() == "home" ? model.option.ToLower() : "page";
            var nonce = HttpContext.Items["CSPNonce"] as string;
            ViewBag.Nonce = nonce;
            ViewData["nonce"] = nonce;
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
            switch (Response.StatusCode)
            {
                case 404:
                    ViewData["VisibleHeader"] = true;
                    ViewData["VisibleFooter"] = true;
                    return View(view);
                default:
                    return View(view, model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
