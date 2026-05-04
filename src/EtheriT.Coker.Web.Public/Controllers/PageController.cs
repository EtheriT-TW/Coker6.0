using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Advertise;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.BonusManagement;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Permissions;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Permissions;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Templates;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

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
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ITemplatesApplicationService templatesApplicationService;
        private readonly IAccountAppService accountAppService;
        private readonly IPermissionsAppService permissionsAppService;
        private readonly IBonusManagementAppService bonusManagementAppService;
        private readonly StringHandler stringHandler;
        private readonly LoginUserData loginUserData;
        private readonly IWebHostEnvironment _env;

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
            IBonusManagementAppService bonusManagementAppService,
            IFileUploadAppService fileUploadAppService,
            ITemplatesApplicationService templatesApplicationService,
            IAccountAppService accountAppService,
            IPermissionsAppService permissionsAppService,
            StringHandler stringHandler,
            LoginUserData loginUserData,
            IWebHostEnvironment env
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
            this.fileUploadAppService = fileUploadAppService;
            this.templatesApplicationService = templatesApplicationService;
            this.accountAppService = accountAppService;
            this.permissionsAppService = permissionsAppService;
            this.bonusManagementAppService = bonusManagementAppService;
            this.loginUserData = loginUserData;
            this._env = env;
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
        public async Task<IActionResult> EmbedAsync(long id) {
            var orgName = await loginUserData.GetWebsiteOrgName();
            var o = await articleAppService.FindArticleOrgName(id);
            if (o.Success && !string.IsNullOrEmpty(o.Message)) orgName = o.Message;
            var resule = await IndexAsync(orgName, "search", "article", id);
            ViewData["VisibleHeader"] = false;
            ViewData["VisibleFooter"] = false;
            ViewBag.ShowSwitchPage = false;
            return resule;
        }

        public async Task<IActionResult> IndexAsync(string? website, string? key, string? option, long? detailId, string? search = null)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status404NotFound);
            if (string.IsNullOrEmpty(key)) key = "home";
            else if (string.IsNullOrEmpty(website) && !string.IsNullOrEmpty(key) && string.IsNullOrEmpty(option))
            {
                website = key;
                key = "home";
            }
            long id = detailId ?? 0;
            var rootSiteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var defaultData = await websiteApplication.GetDefaultData(rootSiteId, website);
            if (website != defaultData.OrgName) website = defaultData.OrgName;
            var siteId = defaultData.Id;
            var freight = JsonConvert.DeserializeObject<List<FreightDisplayDto>>(JsonConvert.SerializeObject((await freightAppService.GetDisplay()).Value));
            var payment = JsonConvert.DeserializeObject<List<PaymentTypeItemOutputDto>>(JsonConvert.SerializeObject((await thirdPartyAppService.GetDisplayPayment()).Value));
            var enterAds = JsonConvert.DeserializeObject<List<AdvertiseDisplayDto>>(JsonConvert.SerializeObject((await advertiseAppService.GetDisplay(defaultData.Id, 1, 1)).Value));
            var SEO = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 1, SiteId = siteId });
            var SystemSet = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 5, SiteId = siteId });
            var MemberSet = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 7, SiteId = siteId, RenderTextareaAsHtml = true });
            var GA4 = SEO.storeSetDetails?.Find(e => e.key == "GA4");
            var GoogleTranslate = SEO.storeSetDetails?.Find(e => e.key == "google.translate");
            var GTM = SEO.storeSetDetails?.Find(e => e.key == "GTM");
            var GoogleAds = SEO.storeSetDetails?.Find(e => e.key == "GoogleAds");
            var NoCopyItem = SystemSet.storeSetDetails?.Find(e => e.key == "NoCopy");
            var MemberRegister = string.Join(",", MemberSet.storeSetDetails?.Find(e => e.key == "MemberRegister")?.value ?? Enumerable.Empty<string>()) == "3";
            var membershipTerms = MemberSet.storeSetDetails?.Find(e => e.key == "membershipTerms");
            var privacyPolicy = MemberSet.storeSetDetails?.Find(e => e.key == "PrivacyPolicy");

            var StoreSet = await storeSetAppService.getValues(new StoreSetGetValueInput { StoreSetGroupId = 2, SiteId = siteId });
            var storeBuyState = StoreSet.storeSetDetails?.Find(e => e.key == "storeBuyState");
            var storeMemo = StoreSet.storeSetDetails?.Find(e => e.key == "storeMemo");
            var linkMore = StoreSet.storeSetDetails?.Find(e => e.key == "linkMore");
            var prodCatalog = StoreSet.storeSetDetails?.Find(e => e.key == "prodCatalog");
            var priceOrder = StoreSet.storeSetDetails?.Find(e => e.key == "priceOrder");
            var HasInvoice = string.Join(",", StoreSet.storeSetDetails?.Find(e => e.key == "HasInvoice")?.value ?? Enumerable.Empty<string>()) != "DisabledInvoice";
            var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
            List<string> Carrier = StoreSet.storeSetDetails?.Find(e => e.key == "ExtraInviiceCarrier")?.value ?? new List<string>();

            var shareImage = await fileUploadAppService.getImgFiles(new FileGetImgInputDto { Sid = siteId, Type = 13 });
            var template = await templatesApplicationService.GetDefaultTemplatesAsync();
            ViewBag.ShowPagePath = true;
            ViewBag.BackstageUrl = Configuration["BACKSTAGE_URL"] ?? Configuration.GetValue<string>("WebConfig:BackstageUrl");
            ViewBag.OAuthError = TempData["OAuthError"];
            ViewBag.OAuthSuccess = TempData["OAuthSuccess"];
            ViewBag.priceOrder = priceOrder != null && priceOrder.value != null && priceOrder.value.Any() && priceOrder.value.Contains("LtoH");
            ViewBag.MemberRegister = !MemberRegister;
            ViewBag.PrivacyPolicy = privacyPolicy != null && privacyPolicy.value != null && privacyPolicy.value.Any() ? string.Join(",", privacyPolicy.value) : "";
            ViewBag.HasInvoice = HasInvoice;
            ViewBag.Carrier = Carrier;
            ViewBag.BonusEnabled = bonusSetting.BonusEnabled;
            if (template != null)
            {
                var header = template.templateSections.FirstOrDefault(e => e.sectionType == SectionTypeEnum.表頭);
                if (header != null) {
                    var ContentConfig = JsonConvert.DeserializeObject<HeaderContentConfigDto>(header.ContentConfig);
                    if(ContentConfig!=null) ViewBag.ShowPagePath = ContentConfig.ShowPagePath;
                }
            }
            if (string.IsNullOrEmpty(defaultData.Root) || !defaultData.Root.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var request = HttpContext.Request;
                var baseUri = new Uri($"{request.Scheme}://{request.Host}/");
                var combinedUri = new Uri(baseUri, defaultData.Root);
                defaultData.Root = combinedUri.ToString();
            }
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
                    GoogleAds = (GoogleAds != null && GoogleAds.value != null) ? String.Join(",", GoogleAds.value!) : "",
                    GoogleTranslate = (GoogleTranslate != null && GoogleTranslate.value != null) ? String.Join(",", GoogleTranslate.value!) : "",
                    GTM = (GTM != null && GTM.value != null) ? String.Join(",", GTM.value!) : "",
                    storeBuyState = (storeBuyState != null && storeBuyState.value != null) ? String.Join(",", storeBuyState.value!) : "",
                    storeMemo = (storeMemo != null && storeMemo.value != null) ? String.Join(",", storeMemo.value!) : "",
                    linkMore = (linkMore != null && linkMore.value != null) ? String.Join(",", linkMore.value!) : "",
                    prodCatalog = (prodCatalog != null && prodCatalog.value != null) ? String.Join(",", prodCatalog.value!) : "",
                    membershipTerms = (membershipTerms != null && membershipTerms.value != null) ? String.Join(",", membershipTerms.value!) : "",
                },
                IsProduction = _env.IsProduction()
            };
            string view;
            if (new List<string> { "article" }.Contains(key.ToLower()) && long.TryParse(option, out id))
            {
                option = key;
            }
            model.option = key;
            ViewBag.option = model.option.ToLower();
            ViewBag.membershipTerms = model.storeSet.membershipTerms;
            Console.WriteLine($"hasMembershipTerms：{(membershipTerms != null && membershipTerms.value != null)}");
            GetFrontContenOutputDto? PageData =  null;
            if (string.IsNullOrEmpty(option)) option = "";
            if (!UseLegacyPathHandling(website, key, option))
            {
                model.PageData = new GetFrontContenOutputDto { SiteName = L.get("PathError") };
                Response.StatusCode = 404;
                view = "../Error/NotFound";
            }
            else if (!string.IsNullOrEmpty(key))
            {
                PageData = await webMenuApplication.GetFrontConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                switch (option.ToLower())
                {
                    case "article":
                        remoteInputDto.FK_WebmenuId = PageData.Id;
                        model.MenuBread = await webMenuApplication.GetMenuBread(PageData.Id);
                        model.PageData = await articleAppService.GetFrontConten(new ArticleGetFrontContenInputDto { siteId = defaultData.Id, articleId = id });
                        remoteInputDto.FK_ArticleId = model.PageData.Id;
                        model.ParentData = PageData;
                        model.PageData.PageView = "Article";
                        model.PageData.LayoutType = defaultData.Layout_Type;
                        model.PageData.holdPage = HoldPageNameEnum.Article;
                        ViewBag.option = option.ToLower();
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
                        ViewBag.linkMore = model.storeSet.linkMore;
                        if (id != 0)
                        {
                            remoteInputDto.FK_WebmenuId = PageData.Id;
                            model.MenuBread = await webMenuApplication.GetMenuBread(PageData.Id);
                            model.PageData = await productAppService.GetFrontConten(new ProdGetFrontContenInputDto { siteId = defaultData.Id, prodId = id });
                            if (model.PageData.Id == 0)
                            {
                                Response.StatusCode = 404;
                                view = "../Error/NotFound";
                            }
                            else
                            {
                                remoteInputDto.FK_ProdId = model.PageData.Id;
                                model.ParentData = PageData;
                                model.PageData.PageView = "Product";
                                model.PageData.LayoutType = defaultData.Layout_Type;
                                model.PageData.holdPage = HoldPageNameEnum.Article;
                                ViewBag.option = option.ToLower();
                                if (key == "product")
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
                        remoteInputDto.FK_WebmenuId = PageData.Id;
                        model.MenuBread = await webMenuApplication.GetMenuBread(PageData.Id);
                        model.PageData = await technicalCertificateAppService.GetFrontConten(new TechCertGetFrontContenInputDto { siteId = defaultData.Id, TechCertId = id });
                        remoteInputDto.FK_TechCertId = model.PageData.Id;
                        model.ParentData = PageData;
                        model.PageData.PageView = "Techcert";
                        model.PageData.LayoutType = defaultData.Layout_Type;
                        model.PageData.holdPage = HoldPageNameEnum.TechCert;
                        if (key.ToLower() == "techvert")
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
                    case "privacy":
                        model.PageData = await websiteApplication.GetPrivacyConten(new GetFrontContenInputDto { key = key, siteId = defaultData.Id });
                        remoteInputDto.FK_WebmenuId = model.PageData.Id;
                        view = "Index";
                        break;
                    default:
                        ViewBag.option = key.ToLower();
                        if (key.ToLower() == "search")
                        {
                            model.PageData = PageData;
                            model.PageData.PageView = "Search";
                            model.PageData.CurrentUrl = "/Search";
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
                            ViewData["prodCatalog"] = model.storeSet.prodCatalog;
                            ViewData["storeMemo"] = model.storeSet.storeMemo;
                            model.PageData = PageData;
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            view = "ShoppingCar";
                        }
                        else if (key.ToLower() == "member")
                        {
                            model.PageData = PageData;
                            model.PageData.CurrentUrl = "/Member";
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            view = "Member";
                        }
                        else if (key.ToLower() == "demosearch")
                        {
                            model.PageData = PageData;
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
                            model.PageData = PageData;
                            remoteInputDto.FK_WebmenuId = model.PageData.Id;
                            view = key;
                        }
                        else
                        {
                            model.PageData = PageData;
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
                    if (rootSiteId != defaultData.Id && model.PageData != null)
                    {
                        model.PageData.Html = stringHandler.HtmlEncode(model.PageData.Html);
                        model.PageData.Html = Regex.Replace(model.PageData.Html, $"src=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"src=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                        model.PageData.Html = Regex.Replace(model.PageData.Html, $"href=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"href=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                        model.PageData.Html = Regex.Replace(model.PageData.Html, $"data-pdf-url=&quot;/upload/(?!{defaultData.ParntOrgNames})", $"data-pdf-url=&quot;/upload/{defaultData.OrgName}/", RegexOptions.IgnoreCase);
                        model.PageData.Css = (model.PageData.Css ?? "").Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                    }
                    if (rootSiteId != defaultData.Id && model.ParentData != null)
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
            ViewBag.RootId = await webMenuApplication.GetRootId(key);
            ViewBag.isLogin = false;
            ViewBag.SiteId = siteId;
            try
            {
                var tokenItem = await tokenAppService.CreateToken();
                if (tokenItem != null)
                {
                    ViewBag.isLogin = tokenItem.IsLogin;
                    if (ViewBag.BonusEnabled && ViewBag.isLogin) {
                        var tokenUUID = await tokenAppService.GetUUID();
                        var UUID = tokenAppService.GetUUID(tokenUUID);
                        var bonus = (await bonusManagementAppService.GetQueryFrontUsersTotalAvaliableBonus(new List<Guid> { UUID })).FirstOrDefault();
                        ViewBag.UserBonus = bonus != null ? bonus.TotalAvaliableBonus : 0;
                        
                        ViewBag.MinOrderForRedemption = bonusSetting.MinOrderForRedemption; // 消費滿額開始紅利折抵
                        ViewBag.MaxRedemptionPercent = bonusSetting.MaxRedemptionPercent; // 折抵比例(%)
                        ViewBag.MinOrderForEarnPoints = bonusSetting.MinOrderForEarnPoints; // 消費買額可獲得紅利
                        ViewBag.RewardRatePercent = bonusSetting.RewardRatePercent; // 回饋比例
                    }
                    if (PageData != null)
                    {
                        var userInfo = await accountAppService.GetFrontUserData();
                        var perm = await permissionsAppService.GetPagePermission(new GetPagePermissionInputDto
                        {
                            isFront = true,
                            PageId = PageData!.Id,
                            Type = PermissionDetailsTypeEnum.選單會員
                        });
                        if (perm.Success && perm.Object != null) {
                            var permission = ((PagePermissionOutputDto)perm.Object).Roles.FindAll(e => e.IsChecked);
                            var isDenied = false;
                            if (permission.Any()) {
                                if (!userInfo.Success)
                                {
                                    isDenied = true;
                                }
                                else {
                                    var roles = permission.Find(e => e.Id == userInfo.data.FK_RoleId);
                                    if (roles != null && !roles.IsChecked)
                                    {
                                        isDenied = true;
                                    }
                                }

                                if (isDenied)
                                {
                                    Response.StatusCode = 401;
                                    view = "../Error/Denied";
                                }
                            }
                        }
                    }
                }
                else throw new Exception();
            }
            catch(Exception e)
            {
                ViewBag.isLogin = false;
                ViewBag.LoginEnable = false;
            }

            var remote = await RemoteAppService.insertRemote(remoteInputDto);
            if(remote!=null && remote.Success) ViewBag.PageKey = remote.Message;

			ViewBag.Css = HttpUtility.HtmlEncode((model.PageData!.Css)??"");
            if (model.ParentData != null)
				ViewBag.Css += HttpUtility.HtmlEncode(model.ParentData.Css);

			if (!string.IsNullOrEmpty(defaultData.Css))
				ViewBag.Css += HttpUtility.HtmlEncode(defaultData.Css);


			ViewData["SideName"] = model.PageData!.SiteName;
            ViewData["PageName"] = model.PageData.Title;
            ViewData["OrgName"] = model.orgName;
            ViewData["Layout"] = model.layout;
            ViewBag.PageTagNameName = key == "home" ? model.PageData.SiteName : $"{model.PageData.Title} - 【{model.PageData.SiteName}】";
            ViewBag.PageTagNameName = HttpUtility.HtmlAttributeEncode(ViewBag.PageTagNameName.Trim());
            ViewData["Description"] = model.PageData.Description;
            ViewBag.GA4 = model.storeSet.GA4;
            ViewBag.GTM = model.storeSet.GTM;
            ViewBag.GoogleAds = model.storeSet.GoogleAds;
            if (shareImage!=null && shareImage.Any()) {
                ViewBag.ImageUrl = new Uri(new Uri(model.root), shareImage[0].Link).AbsoluteUri;
            }
            else ViewBag.ImageUrl = string.IsNullOrEmpty(model.PageData.ImageUrl) ? "" : new Uri(new Uri(model.root), model.PageData.ImageUrl).AbsoluteUri;
            ViewBag.NoCopy = _env.IsProduction() && NoCopyItem != null && NoCopyItem.value != null && NoCopyItem.value.Count > 0 && NoCopyItem.value[0] == "1" ? "no-right-click" : "";
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
            ViewBag.SearchWord = JsonConvert.SerializeObject(search);
            ViewBag.Nonce = nonce;
            ViewData["nonce"] = nonce;
            ViewBag.storeBuyState = model.storeSet.storeBuyState ?? "noPay";
            ViewBag.IsProduction = model.IsProduction;
            ViewBag.ShowSwitchPage = true;
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
                case 401:
                    ViewData["VisibleHeader"] = true;
                    ViewData["VisibleFooter"] = true;
                    return View(view);
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
