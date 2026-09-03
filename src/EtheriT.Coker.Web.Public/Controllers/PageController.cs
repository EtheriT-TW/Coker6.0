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
using EtheriT.Coker.Application.Shared.Currency;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
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
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Templates;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Web.Public.Models;
using EtheriT.Coker.Web.Public.Helpers;
using EtheriT.Coker.Web.Public.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
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
        private readonly IDirectoryAppService directoryAppService;
        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IProductAppService productAppService;
        private readonly ICustSearchAppService custSearchAppService;
        private readonly IStoreSetAppService storeSetAppService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ITechnicalCertificateAppService technicalCertificateAppService;
        private readonly ITokenAppService tokenAppService;
        private readonly IAdvertiseAppService advertiseAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ITemplatesApplicationService templatesApplicationService;
        private readonly IFrontAccountAppService accountAppService;
        private readonly IPermissionsAppService permissionsAppService;
        private readonly IBonusManagementAppService bonusManagementAppService;
        private readonly StringHandler stringHandler;
        private readonly LoginUserData loginUserData;
        private readonly RemoteTrackingTokenService remoteTrackingTokenService;
        private readonly IHtmlProcessor htmlProcessor;
        private readonly IWebHostEnvironment _env;

        public PageController(
            ILogger<PageController> logger,
            IFreightAppService freightAppService,
            IThirdPartyAppService thirdpartyAppService,
            IWebMenuApplication webMenuApplication,
            IConfiguration configuration,
            IWebsiteApplication websiteApplication,
            IArticleAppService articleAppService,
            IDirectoryAppService directoryAppService,
            IHtmlContentAppService htmlContentAppService,
            IProductAppService productAppService,
            IStoreSetAppService storeSetAppService,
            ICustSearchAppService custSearchAppService,
            IHttpContextAccessor httpContextAccessor,
            ITechnicalCertificateAppService technicalCertificateAppService,
            IAdvertiseAppService advertiseAppService,
            ITokenAppService tokenAppService,
            IBonusManagementAppService bonusManagementAppService,
            IFileUploadAppService fileUploadAppService,
            ITemplatesApplicationService templatesApplicationService,
            IFrontAccountAppService accountAppService,
            IPermissionsAppService permissionsAppService,
            StringHandler stringHandler,
            LoginUserData loginUserData,
            RemoteTrackingTokenService remoteTrackingTokenService,
            IHtmlProcessor htmlProcessor,
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
            this.directoryAppService = directoryAppService;
            this.htmlContentAppService = htmlContentAppService;
            this.productAppService = productAppService;
            this.stringHandler = stringHandler;
            this.storeSetAppService = storeSetAppService;
            this.custSearchAppService = custSearchAppService;
            this.httpContextAccessor = httpContextAccessor;
            this.technicalCertificateAppService = technicalCertificateAppService;
            this.tokenAppService = tokenAppService;
            this.advertiseAppService = advertiseAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.templatesApplicationService = templatesApplicationService;
            this.accountAppService = accountAppService;
            this.permissionsAppService = permissionsAppService;
            this.bonusManagementAppService = bonusManagementAppService;
            this.loginUserData = loginUserData;
            this.remoteTrackingTokenService = remoteTrackingTokenService;
            this.htmlProcessor = htmlProcessor;
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
            var productPageLayout = StoreSet.storeSetDetails?.Find(e => e.key == "ProductPageLayout");
            var priceOrder = StoreSet.storeSetDetails?.Find(e => e.key == "priceOrder");
            var priceCurrencySetting = StoreSet.storeSetDetails?.Find(e => e.key == "priceCurrency");
            var priceCurrency = CurrencyCatalog.Resolve(priceCurrencySetting?.value?.FirstOrDefault());
            var HasInvoice = string.Join(",", StoreSet.storeSetDetails?.Find(e => e.key == "HasInvoice")?.value ?? Enumerable.Empty<string>()) != "DisabledInvoice";
            var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
            List<string> Carrier = StoreSet.storeSetDetails?.Find(e => e.key == "ExtraInviiceCarrier")?.value ?? new List<string>();

            var shareImage = await fileUploadAppService.getImgFiles(new FileGetImgInputDto { Sid = siteId, Type = 13 });
            var template = await templatesApplicationService.GetDefaultTemplatesAsync(defaultData.Id);
            var globalSettings = await templatesApplicationService.GetGlobalSettingsForDisplayAsync(defaultData.Id);
            ViewBag.BackstageUrl = Configuration["BACKSTAGE_URL"] ?? Configuration.GetValue<string>("WebConfig:BackstageUrl");
            ViewBag.OAuthError = TempData["OAuthError"];
            ViewBag.OAuthSuccess = TempData["OAuthSuccess"];
            var orderPriceLowToHigh = priceOrder != null &&
                priceOrder.value != null &&
                priceOrder.value.Any() &&
                priceOrder.value.Contains("LtoH");
            ViewBag.priceOrder = orderPriceLowToHigh;
            ViewBag.PriceCurrencyCode = priceCurrency.Code;
            ViewBag.PriceCurrencySymbol = priceCurrency.Symbol;
            ViewBag.PriceCurrencyDecimalDigits = priceCurrency.DecimalDigits;
            ViewBag.MemberRegister = !MemberRegister;
            ViewBag.PrivacyPolicy = privacyPolicy != null && privacyPolicy.value != null && privacyPolicy.value.Any() ? string.Join(",", privacyPolicy.value) : "";
            ViewBag.HasInvoice = HasInvoice;
            ViewBag.Carrier = Carrier;
            ViewBag.BonusEnabled = bonusSetting.BonusEnabled;
            var headerStyleView = defaultData.View;
            var configuredHeader = template?.templateSections.FirstOrDefault(e => e.sectionType == SectionTypeEnum.表頭);
            if (template != null && configuredHeader != null && !string.IsNullOrWhiteSpace(configuredHeader.ContentConfig))
            {
                headerStyleView = template.HeadType switch
                {
                    HeadTypeEnum.logo在左選單在右 => "Layout_8",
                    HeadTypeEnum.logo與Banner重疊 => "Layout_8",
                    _ => "Layout_7"
                };
            }
            ViewBag.HeaderLayoutClass = headerStyleView == "Layout_1" ? "layout-1-header" : "";
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
                GlobalSettings = globalSettings,
                IsProduction = _env.IsProduction()
            };
            string view;
            var htmlSanitizeSourceType = HtmlSanitizeSourceType.選單;
            if (new List<string> { "article" }.Contains(key.ToLower()) && long.TryParse(option, out id))
            {
                option = key;
            }
            model.option = key;
            ViewBag.option = model.option.ToLower();
            ViewBag.RouterName = ViewBag.option;
            ViewBag.membershipTerms = model.storeSet.membershipTerms;
            Console.WriteLine($"hasMembershipTerms：{(membershipTerms != null && membershipTerms.value != null)}");
            GetFrontContenOutputDto? PageData =  null;
            ProductSeoDataDto? productSeoData = null;
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
                        htmlSanitizeSourceType = HtmlSanitizeSourceType.文章;
                        remoteInputDto.FK_WebmenuId = PageData.Id;
                        model.MenuBread = await webMenuApplication.GetMenuBread(PageData.Id);
                        model.PageData = await articleAppService.GetFrontConten(new ArticleGetFrontContenInputDto
                        {
                            siteId = defaultData.Id,
                            articleId = id
                        });

                        if (model.PageData.Id == 0 || string.IsNullOrEmpty(model.PageData.Html))
                        {
                            Response.StatusCode = 404;
                            view = "../Error/NotFound";
                            break;
                        }

                        if (await IsFrontRoleDeniedAsync(model.PageData.Id, PermissionDetailsTypeEnum.文章會員))
                        {
                            Response.StatusCode = 401;
                            view = "../Error/Denied";
                            break;
                        }

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
                        else view = "Index";
                        break;
                    case "product":
                        htmlSanitizeSourceType = HtmlSanitizeSourceType.商品;
                        ViewBag.IsProductDetail = true;
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
                                ViewBag.RouterName = "Product";
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

                                productSeoData = await productAppService.GetSeoData(
                                    new ProdGetFrontContenInputDto
                                    {
                                        siteId = defaultData.Id,
                                        prodId = id
                                    },
                                    orderPriceLowToHigh);
                                var layoutKey = productPageLayout?.value?.FirstOrDefault();
                                if(layoutKey != null)
                                {
                                    view = $"ProductContent/{layoutKey}";
                                }
                                else
                                {
                                    view = "ProductContent/Layout_1";
                                }
                            }
                        }
                        else view = "../Error/NotFound";
                        break;
                    case "techcert":
                        htmlSanitizeSourceType = HtmlSanitizeSourceType.頁面;
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
                        else view = "Index";
                        break;
                    case "privacy":
                        htmlSanitizeSourceType = HtmlSanitizeSourceType.頁面;
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
                                SearchId = SearchTargetIds.Normalize(id),
                                SearchText = search ?? "",
                                Class = await custSearchAppService.GetSearchList(defaultData.Id)
                            };
                            if (model.SearchPalameter.SearchId == SearchTargetIds.Default)
                            {
                                model.SearchPalameter.SearchId = model.SearchPalameter.Class.Exists(e => e.Id == SearchTargetIds.Product)
                                    ? SearchTargetIds.Product
                                    : SearchTargetIds.Article;
                            }
                            view = "CustSearch";

                            ViewBag.RouterName = "Search";
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
                            ViewBag.RouterName = "Member";
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
                            else view = "Index";
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
                    model.SafeHtml = stringHandler.HtmlDecode(model.PageData?.Html ?? "");
                    model.SafeCss = model.PageData?.Css ?? "";
                    model.ParentSafeHtml = stringHandler.HtmlDecode(model.ParentData?.Html ?? "");
                    model.ParentSafeCss = model.ParentData?.Css ?? "";
                    model.HtmlSanitizeWebsiteId = defaultData.Id;
                    model.HtmlSanitizeSourceType = htmlSanitizeSourceType;
                    model.HtmlSanitizeSourceId = model.PageData?.Id ?? 0;
                    model.ParentHtmlSanitizeSourceId = model.ParentData?.Id ?? 0;
                    model.RewriteUploadPaths = rootSiteId != defaultData.Id;
                    model.UploadOrgName = defaultData.OrgName ?? "";
                    model.UploadParentOrgNames = defaultData.ParntOrgNames ?? "";
                }
            }
            else
            {
                view = "index";
            }
            ViewBag.HasFullBanner = Regex.IsMatch(
                $"{model.ParentSafeHtml} {model.SafeHtml}",
                "class\\s*=\\s*[\"'][^\"']*\\bfull-banner\\b[^\"']*[\"']",
                RegexOptions.IgnoreCase);
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
                        ViewBag.MaximumDiscount = bonusSetting.MaximumDiscount; // 單筆訂單紅利折抵上限，空值表示無限制
                        ViewBag.MinOrderForEarnPoints = bonusSetting.MinOrderForEarnPoints; // 消費買額可獲得紅利
                        ViewBag.RewardRatePercent = bonusSetting.RewardRatePercent; // 回饋比例
                        ViewBag.RewardCalculationType = (int)bonusSetting.RewardCalculationType; // 回饋計算方式
                        ViewBag.RewardFixedPoints = bonusSetting.RewardFixedPoints; // 固定回饋點數
                        ViewBag.RewardFixedPointsCumulative = bonusSetting.RewardFixedPointsCumulative; // 固定點數是否累計
                    }
                    if (ViewBag.isLogin)
                    {
                        ViewBag.UserLevel = await accountAppService.GetFrontUserLevelName();
                    }
                    if (PageData != null && view.IndexOf("Error/") < 0)
                    {
                        if (await IsFrontRoleDeniedAsync(PageData.Id, PermissionDetailsTypeEnum.選單會員))
                        {
                            Response.StatusCode = 401;
                            view = "../Error/Denied";
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

            ViewBag.RemoteTrackingToken = remoteTrackingTokenService.Protect(remoteInputDto);

			var pageCss = model.PageData!.Css ?? "";
            var parentCss = model.ParentData?.Css ?? "";
            if (model.RewriteUploadPaths)
            {
                pageCss = pageCss.Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
                parentCss = parentCss.Replace("background-image:url('/upload/", $"background-image:url('/upload/{defaultData.OrgName}/");
            }
            var isProductContentView = view.StartsWith("ProductContent/", StringComparison.OrdinalIgnoreCase);
            ViewBag.ProductContentCss = isProductContentView
                ? HttpUtility.HtmlEncode(pageCss)
                : string.Empty;
			ViewBag.Css = isProductContentView
                ? string.Empty
                : HttpUtility.HtmlEncode(pageCss);
            if (model.ParentData != null)
				ViewBag.Css += HttpUtility.HtmlEncode(parentCss);

			if (!string.IsNullOrEmpty(defaultData.Css))
				ViewBag.Css += HttpUtility.HtmlEncode(defaultData.Css);


			ViewData["SideName"] = model.PageData!.SiteName;
            ViewData["PageName"] = model.PageData.Title;
            ViewData["OrgName"] = model.orgName;
            ViewData["Layout"] = model.layout;
            var isProductPage = string.Equals(
                model.PageData.PageView,
                "Product",
                StringComparison.OrdinalIgnoreCase);
            var isHomePage = !isProductPage && string.Equals(
                key,
                "home",
                StringComparison.OrdinalIgnoreCase);
            var rendersInheritedHtml = string.Equals(
                    view,
                    "Index",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    view,
                    "ShoppingCar",
                    StringComparison.OrdinalIgnoreCase);
            var renderedContentHtml = rendersInheritedHtml
                ? MainHeadingHtml.PrepareInheritedContent(
                    htmlProcessor,
                    model.SafeHtml,
                    model.ParentSafeHtml)
                : string.Empty;
            var contentMainHeadingCount = MainHeadingHtml.CountMainHeadings(
                htmlProcessor,
                renderedContentHtml);
            var viewHasOwnMainHeading = isProductPage ||
                view.Contains("Error/", StringComparison.OrdinalIgnoreCase);

            if (contentMainHeadingCount > 1)
            {
                _logger.LogWarning(
                    "Page {PageId} ({PageView}) contains {HeadingCount} H1 elements after inherited content composition.",
                    model.PageData.Id,
                    model.PageData.PageView,
                    contentMainHeadingCount);
            }

            ViewData["UseSiteTitleAsMainHeading"] = !viewHasOwnMainHeading &&
                contentMainHeadingCount == 0;
            ViewData["MainHeading"] = isHomePage
                ? model.PageData.SiteName
                : model.PageData.Title;
            var canonicalPageUrl = BuildCanonicalPageUrl(model);
            ViewBag.PageTagNameName = isHomePage
                ? model.PageData.SiteName
                : $"{model.PageData.Title} - 【{model.PageData.SiteName}】";
            ViewBag.PageTagNameName = HttpUtility.HtmlAttributeEncode(ViewBag.PageTagNameName.Trim());
            var seoDescription = await SeoMetaDescription.BuildAsync(
                htmlProcessor,
                model.PageData.Description,
                model.SafeHtml,
                defaultData.Description,
                model.PageData.Title,
                model.locale,
                directoryIds => directoryAppService.GetSeoData(
                    directoryIds,
                    siteId));
            ViewData["Description"] = seoDescription;
            ViewBag.GA4 = model.storeSet.GA4;
            ViewBag.GTM = model.storeSet.GTM;
            ViewBag.GoogleAds = model.storeSet.GoogleAds;
            if (shareImage!=null && shareImage.Any()) {
                ViewBag.ImageUrl = new Uri(new Uri(model.root), shareImage[0].Link).AbsoluteUri;
            }
            else ViewBag.ImageUrl = string.IsNullOrEmpty(model.PageData.ImageUrl) ? "" : new Uri(new Uri(model.root), model.PageData.ImageUrl).AbsoluteUri;
            if (isHomePage)
            {
                var rootUri = new Uri(model.root.EndsWith("/", StringComparison.Ordinal) ? model.root : $"{model.root}/");
                var websiteData = (await websiteApplication.GetAllData(siteId))
                    .FirstOrDefault(e => e.Id == siteId);
                var organizationLogoUrl = ResolveStructuredDataImage(rootUri, websiteData?.Logo);
                var websiteStructuredData = BuildWebsiteStructuredData(
                    model.PageData.SiteName,
                    canonicalPageUrl,
                    seoDescription,
                    model.locale,
                    organizationLogoUrl);
                RemoveNullStructuredDataValues(websiteStructuredData);

                ViewBag.WebsiteStructuredDataWebsiteId = siteId;
                ViewBag.WebsiteStructuredDataJson = JsonConvert.SerializeObject(
                    websiteStructuredData,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        StringEscapeHandling = StringEscapeHandling.EscapeHtml
                    });
            }
            if (isProductPage && productSeoData != null)
            {
                var rootUri = new Uri(model.root.EndsWith("/", StringComparison.Ordinal) ? model.root : $"{model.root}/");
                var productImageUrl = string.IsNullOrWhiteSpace(model.PageData.ImageUrl)
                    ? null
                    : new Uri(rootUri, model.PageData.ImageUrl.TrimStart('/')).AbsoluteUri;

                var productStructuredData = BuildProductStructuredData(
                    productSeoData,
                    canonicalPageUrl,
                    rootUri,
                    productImageUrl,
                    seoDescription,
                    priceCurrency.Code);
                RemoveNullStructuredDataValues(productStructuredData);

                ViewBag.ProductStructuredDataProductId = productSeoData.Id;
                ViewBag.ProductStructuredDataJson = JsonConvert.SerializeObject(
                    productStructuredData,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        StringEscapeHandling = StringEscapeHandling.EscapeHtml
                    });
            }
            if (!isHomePage)
            {
                var breadcrumbRootUri = new Uri(
                    model.root.EndsWith("/", StringComparison.Ordinal)
                        ? model.root
                        : $"{model.root}/");
                var breadcrumbStructuredData = BuildBreadcrumbStructuredData(
                    model.MenuBread,
                    model.PageData.Title,
                    canonicalPageUrl,
                    breadcrumbRootUri,
                    new Uri(breadcrumbRootUri, $"{model.orgName}/home").AbsoluteUri,
                    model.PageData.PageView is "Article" or "Techcert");
                if (breadcrumbStructuredData != null)
                {
                    RemoveNullStructuredDataValues(breadcrumbStructuredData);
                    ViewBag.BreadcrumbStructuredDataPageType = model.PageData.PageView;
                    ViewBag.BreadcrumbStructuredDataPageId = model.PageData.Id;
                    ViewBag.BreadcrumbStructuredDataJson = JsonConvert.SerializeObject(
                        breadcrumbStructuredData,
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            StringEscapeHandling = StringEscapeHandling.EscapeHtml
                        });
                }
            }
            ViewBag.NoCopy = _env.IsProduction() && NoCopyItem != null && NoCopyItem.value != null && NoCopyItem.value.Count > 0 && NoCopyItem.value[0] == "1" ? "no-right-click" : "";
            ViewData["google.translate"] = model.storeSet.GoogleTranslate;
            ViewData["CurrentUrl"] = model.PageData.CurrentUrl;
            ViewData["CanonicalUrl"] = canonicalPageUrl;
            ViewData["OpenGraphUrl"] = isProductPage
                ? $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}"
                : model.PageData.CurrentUrl;
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

        private static Dictionary<string, object?> BuildProductStructuredData(
            ProductSeoDataDto product,
            string canonicalUrl,
            Uri rootUri,
            string? productImageUrl,
            string? description,
            string priceCurrency)
        {
            var hasCompleteVariants = product.Variants.Count > 1 &&
                product.Variants.All(e => e.Options.Count > 0);
            if (!hasCompleteVariants)
            {
                return new Dictionary<string, object?>
                {
                    ["@context"] = "https://schema.org",
                    ["@type"] = "Product",
                    ["@id"] = $"{canonicalUrl}#product",
                    ["name"] = product.Title,
                    ["url"] = canonicalUrl,
                    ["description"] = description,
                    ["image"] = productImageUrl == null ? null : new[] { productImageUrl },
                    ["sku"] = NormalizeStructuredDataSku(product.ItemNo),
                    ["offers"] = product.PublicPrice.HasValue
                        ? BuildProductOffer(
                            canonicalUrl,
                            product.PublicPrice.Value,
                            product.IsAvailable,
                            priceCurrency)
                        : null
                };
            }

            var variantSemanticValues = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            var hasVariant = new List<Dictionary<string, object?>>();
            foreach (var variant in product.Variants)
            {
                var variantUrl = $"{canonicalUrl}?psid={variant.StockId.ToString(CultureInfo.InvariantCulture)}";
                var optionValues = variant.Options
                    .Select(e => e.Value?.Trim())
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var optionDescription = string.Join(
                    "、",
                    variant.Options
                        .Where(e => !string.IsNullOrWhiteSpace(e.Value))
                        .Select(e => string.IsNullOrWhiteSpace(e.TypeName)
                            ? e.Value.Trim()
                            : $"{e.TypeName.Trim()}：{e.Value.Trim()}"));
                var variantImageUrl = ResolveStructuredDataImage(rootUri, variant.ImageUrl) ?? productImageUrl;
                var variantData = new Dictionary<string, object?>
                {
                    ["@type"] = "Product",
                    ["@id"] = $"{variantUrl}#product",
                    ["name"] = optionValues.Count == 0
                        ? product.Title
                        : $"{product.Title}－{string.Join("／", optionValues)}",
                    ["url"] = variantUrl,
                    ["description"] = string.IsNullOrWhiteSpace(optionDescription)
                        ? description
                        : string.IsNullOrWhiteSpace(description)
                            ? optionDescription
                            : $"{description}；{optionDescription}",
                    ["image"] = variantImageUrl == null ? null : new[] { variantImageUrl },
                    ["sku"] = NormalizeStructuredDataSku(variant.SubItemNo),
                    ["offers"] = BuildProductOffer(
                        variantUrl,
                        variant.PublicPrice,
                        variant.IsAvailable,
                        priceCurrency)
                };

                foreach (var option in variant.Options)
                {
                    var semanticProperty = ApplyVariantSemanticProperty(variantData, option);
                    if (semanticProperty.HasValue)
                    {
                        if (!variantSemanticValues.TryGetValue(
                            semanticProperty.Value.PropertyUrl,
                            out var values))
                        {
                            values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            variantSemanticValues[semanticProperty.Value.PropertyUrl] = values;
                        }
                        values.Add(semanticProperty.Value.Value);
                    }
                }

                hasVariant.Add(variantData);
            }

            var variesBy = variantSemanticValues
                .Where(e => e.Value.Count > 1)
                .Select(e => e.Key)
                .ToArray();

            return new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "ProductGroup",
                ["@id"] = $"{canonicalUrl}#product-group",
                ["name"] = product.Title,
                ["url"] = canonicalUrl,
                ["description"] = description,
                ["image"] = productImageUrl == null ? null : new[] { productImageUrl },
                ["productGroupID"] = $"product-{product.Id.ToString(CultureInfo.InvariantCulture)}",
                ["variesBy"] = variesBy.Length == 0 ? null : variesBy,
                ["hasVariant"] = hasVariant
            };
        }

        private static Dictionary<string, object?> BuildWebsiteStructuredData(
            string? siteName,
            string canonicalUrl,
            string? description,
            string? locale,
            string? organizationLogoUrl)
        {
            var organizationId = $"{canonicalUrl}#organization";
            var websiteId = $"{canonicalUrl}#website";
            var normalizedLocale = string.Equals(locale, "zh-tw", StringComparison.OrdinalIgnoreCase)
                ? "zh-TW"
                : locale?.Trim();

            return new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@graph"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["@type"] = "Organization",
                        ["@id"] = organizationId,
                        ["name"] = siteName,
                        ["url"] = canonicalUrl,
                        ["description"] = description,
                        ["logo"] = organizationLogoUrl
                    },
                    new Dictionary<string, object?>
                    {
                        ["@type"] = "WebSite",
                        ["@id"] = websiteId,
                        ["name"] = siteName,
                        ["url"] = canonicalUrl,
                        ["description"] = description,
                        ["inLanguage"] = normalizedLocale,
                        ["publisher"] = new Dictionary<string, object?>
                        {
                            ["@id"] = organizationId
                        }
                    }
                }
            };
        }

        private static string? NormalizeStructuredDataSku(string? sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return null;
            }

            return Regex.Replace(sku.Trim(), @"\s+", "-");
        }

        private string BuildCanonicalPageUrl(PageViewModel model)
        {
            var rootUri = new Uri(
                model.root.EndsWith("/", StringComparison.Ordinal)
                    ? model.root
                    : $"{model.root}/");
            var pageView = model.PageData?.PageView ?? string.Empty;
            var relativeUrl = pageView switch
            {
                "Article" => $"{model.orgName}/search/article/{model.PageData!.Id}",
                "Product" => $"{model.orgName}/search/product/{model.PageData!.Id}",
                "Techcert" => $"{model.orgName}/search/techcert/{model.PageData!.Id}",
                _ => $"{model.orgName}/{model.PageData?.CurrentUrl?.TrimStart('/') ?? string.Empty}"
            };
            var canonicalUrl = new Uri(rootUri, relativeUrl).AbsoluteUri;

            if (!HasSingleDirectoryCatalog(model.SafeHtml) ||
                !int.TryParse(
                    Request.Query["Page"].ToString(),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var pageNumber) ||
                pageNumber <= 1)
            {
                return canonicalUrl;
            }

            var canonicalUri = new UriBuilder(canonicalUrl)
            {
                Query = $"Page={pageNumber.ToString(CultureInfo.InvariantCulture)}"
            };
            return canonicalUri.Uri.AbsoluteUri;
        }

        private static bool HasSingleDirectoryCatalog(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return false;
            }

            return Regex.Matches(
                html,
                @"\bcatalog_frame\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count == 1;
        }

        private static Dictionary<string, object?>? BuildBreadcrumbStructuredData(
            List<GetMenuBreadDto>? menuBread,
            string? currentTitle,
            string canonicalUrl,
            Uri rootUri,
            string homeUrl,
            bool appendCurrentItem)
        {
            if (string.IsNullOrWhiteSpace(currentTitle))
            {
                return null;
            }

            var sourceItems = (menuBread ?? new List<GetMenuBreadDto>())
                .Where(e => !string.IsNullOrWhiteSpace(e.Title))
                .ToList();
            var lastSourceItem = sourceItems.LastOrDefault();
            var lastIsCurrent = !appendCurrentItem && lastSourceItem != null &&
                string.Equals(
                    lastSourceItem.Title.Trim(),
                    currentTitle.Trim(),
                    StringComparison.OrdinalIgnoreCase);
            var listItems = new List<Dictionary<string, object?>>();
            var homeSourceItem = sourceItems.FirstOrDefault(e =>
                AreSameBreadcrumbUrls(ResolveBreadcrumbUrl(rootUri, e.Link), homeUrl));
            listItems.Add(new Dictionary<string, object?>
            {
                ["@type"] = "ListItem",
                ["position"] = 1,
                ["name"] = string.IsNullOrWhiteSpace(homeSourceItem?.Title)
                    ? "Home"
                    : homeSourceItem.Title.Trim(),
                ["item"] = homeUrl
            });

            foreach (var sourceItem in sourceItems)
            {
                var isCurrent = ReferenceEquals(sourceItem, lastSourceItem) && lastIsCurrent;
                var itemUrl = isCurrent
                    ? canonicalUrl
                    : ResolveBreadcrumbUrl(rootUri, sourceItem.Link);

                if (AreSameBreadcrumbUrls(itemUrl, homeUrl))
                {
                    continue;
                }

                // Google 要求非最後一階具有可導覽 URL；純分類標題沒有連結時保守略過。
                if (!isCurrent && itemUrl == null)
                {
                    continue;
                }

                listItems.Add(new Dictionary<string, object?>
                {
                    ["@type"] = "ListItem",
                    ["position"] = listItems.Count + 1,
                    ["name"] = sourceItem.Title.Trim(),
                    ["item"] = itemUrl
                });
            }

            if (!lastIsCurrent)
            {
                listItems.Add(new Dictionary<string, object?>
                {
                    ["@type"] = "ListItem",
                    ["position"] = listItems.Count + 1,
                    ["name"] = currentTitle.Trim(),
                    ["item"] = canonicalUrl
                });
            }

            if (listItems.Count < 2)
            {
                return null;
            }

            return new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "BreadcrumbList",
                ["@id"] = $"{canonicalUrl}#breadcrumb",
                ["itemListElement"] = listItems
            };
        }

        private static string? ResolveBreadcrumbUrl(Uri rootUri, string? link)
        {
            if (string.IsNullOrWhiteSpace(link) ||
                link.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                link.StartsWith("#", StringComparison.Ordinal))
            {
                return null;
            }

            if (link.StartsWith("/", StringComparison.Ordinal))
            {
                return new Uri(rootUri, link.Trim()).AbsoluteUri;
            }

            if (Uri.TryCreate(link, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.Scheme is "http" or "https" &&
                       string.Equals(absoluteUri.Host, rootUri.Host, StringComparison.OrdinalIgnoreCase)
                    ? absoluteUri.AbsoluteUri
                    : null;
            }

            return new Uri(rootUri, link.Trim()).AbsoluteUri;
        }

        private static bool AreSameBreadcrumbUrls(string? left, string? right)
            => !string.IsNullOrWhiteSpace(left) &&
               !string.IsNullOrWhiteSpace(right) &&
               string.Equals(
                   left.TrimEnd('/'),
                   right.TrimEnd('/'),
                   StringComparison.OrdinalIgnoreCase);

        private static void RemoveNullStructuredDataValues(object? value)
        {
            if (value is IDictionary<string, object?> dictionary)
            {
                var nullKeys = dictionary
                    .Where(e => e.Value == null)
                    .Select(e => e.Key)
                    .ToList();
                foreach (var key in nullKeys)
                {
                    dictionary.Remove(key);
                }

                foreach (var child in dictionary.Values)
                {
                    RemoveNullStructuredDataValues(child);
                }
                return;
            }

            if (value is System.Collections.IEnumerable items && value is not string)
            {
                foreach (var item in items)
                {
                    RemoveNullStructuredDataValues(item);
                }
            }
        }

        private static Dictionary<string, object?> BuildProductOffer(
            string url,
            decimal price,
            bool isAvailable,
            string priceCurrency)
            => new()
            {
                ["@type"] = "Offer",
                ["url"] = url,
                ["priceCurrency"] = priceCurrency,
                ["price"] = price.ToString("0.################", CultureInfo.InvariantCulture),
                ["availability"] = isAvailable
                    ? "https://schema.org/InStock"
                    : "https://schema.org/OutOfStock"
            };

        private static (string PropertyUrl, string Value)? ApplyVariantSemanticProperty(
            Dictionary<string, object?> variant,
            ProductSeoVariantOptionDto option)
        {
            var value = option.Value?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var propertyName = option.SeoVariantProperty switch
            {
                SeoVariantPropertyEnum.Color => "color",
                SeoVariantPropertyEnum.Size => "size",
                SeoVariantPropertyEnum.Material => "material",
                SeoVariantPropertyEnum.Pattern => "pattern",
                _ => null
            };
            if (propertyName != null)
            {
                // 同一變體若誤設兩個相同語意，保留第一個值，避免輸出互相矛盾的資料。
                if (!variant.ContainsKey(propertyName))
                {
                    variant[propertyName] = value;
                    return ($"https://schema.org/{propertyName}", value);
                }
                return null;
            }

            if (option.SeoVariantProperty == SeoVariantPropertyEnum.SuggestedGender &&
                TryNormalizeSuggestedGender(value, out var suggestedGender))
            {
                GetOrCreateAudience(variant)["suggestedGender"] = suggestedGender;
                return ("https://schema.org/suggestedGender", suggestedGender);
            }

            if (option.SeoVariantProperty == SeoVariantPropertyEnum.SuggestedAge &&
                TryBuildSuggestedAge(value, out var suggestedAge, out var normalizedAge))
            {
                GetOrCreateAudience(variant)["suggestedAge"] = suggestedAge;
                return ("https://schema.org/suggestedAge", normalizedAge);
            }

            return null;
        }

        private static Dictionary<string, object?> GetOrCreateAudience(
            Dictionary<string, object?> variant)
        {
            if (variant.TryGetValue("audience", out var existing) &&
                existing is Dictionary<string, object?> audience)
            {
                return audience;
            }

            audience = new Dictionary<string, object?>
            {
                ["@type"] = "PeopleAudience"
            };
            variant["audience"] = audience;
            return audience;
        }

        private static bool TryNormalizeSuggestedGender(string value, out string normalized)
        {
            var key = Regex.Replace(value.Trim().ToLowerInvariant(), @"[\s_\-－]+", string.Empty);
            normalized = key switch
            {
                "male" or "man" or "men" or "男" or "男性" or "男款" => "https://schema.org/Male",
                "female" or "woman" or "women" or "女" or "女性" or "女款" => "https://schema.org/Female",
                "unisex" or "中性" or "男女適用" or "男女通用" => "Unisex",
                _ => string.Empty
            };
            return normalized.Length > 0;
        }

        private static bool TryBuildSuggestedAge(
            string value,
            out Dictionary<string, object?> suggestedAge,
            out string normalizedAge)
        {
            var key = Regex.Replace(value.Trim().ToLowerInvariant(), @"[\s_\-－]+", string.Empty);
            decimal? minValue = null;
            decimal? maxValue = null;
            switch (key)
            {
                case "newborn":
                case "新生兒":
                    minValue = 0m;
                    maxValue = 0.25m;
                    break;
                case "infant":
                case "嬰兒":
                    minValue = 0.25m;
                    maxValue = 1m;
                    break;
                case "toddler":
                case "幼兒":
                    minValue = 1m;
                    maxValue = 5m;
                    break;
                case "kids":
                case "kid":
                case "children":
                case "兒童":
                case "孩童":
                    minValue = 5m;
                    maxValue = 13m;
                    break;
                case "adult":
                case "成人":
                    minValue = 13m;
                    break;
            }

            suggestedAge = new Dictionary<string, object?>();
            normalizedAge = string.Empty;
            if (!minValue.HasValue)
            {
                return false;
            }

            suggestedAge["@type"] = "QuantitativeValue";
            suggestedAge["minValue"] = minValue.Value;
            suggestedAge["maxValue"] = maxValue;
            suggestedAge["unitCode"] = "ANN";
            normalizedAge = $"{minValue.Value.ToString(CultureInfo.InvariantCulture)}-{maxValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty}";
            return true;
        }

        private static string? ResolveStructuredDataImage(Uri rootUri, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            return new Uri(rootUri, imageUrl.Trim()).AbsoluteUri;
        }

        private async Task<bool> IsFrontRoleDeniedAsync(long targetId, PermissionDetailsTypeEnum type)
        {
            var userInfo = await accountAppService.GetFrontUserData();

            var perm = await permissionsAppService.GetPagePermission(new GetPagePermissionInputDto
            {
                isFront = true,
                PageId = targetId,
                Type = type
            });

            if (!perm.Success || perm.Object == null)
            {
                return false;
            }

            var permissionOutput = (PagePermissionOutputDto)perm.Object;

            var allowedRoleIds = permissionOutput.Roles
                .Where(e => e.IsChecked)
                .Select(e => e.Id)
                .ToList();

            // 沒有設定任何角色限制，代表公開
            if (!allowedRoleIds.Any())
            {
                return false;
            }

            // 有設定限制，但會員未登入，拒絕
            if (!userInfo.Success || userInfo.data == null)
            {
                return true;
            }

            // 有設定限制，但目前會員角色不在允許清單，拒絕
            return !allowedRoleIds.Contains(userInfo.data.FK_RoleId);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
