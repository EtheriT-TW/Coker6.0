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
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
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
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Templates;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Web.Public.Models;
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

                                if (!string.IsNullOrEmpty(model.PageData.Html) && string.IsNullOrEmpty(model.PageData.Description))
                                {
                                    string htmlString = stringHandler.HtmlDecode(model.PageData.Html);
                                    model.PageData.Description = Regex.Replace(htmlString, @"<(.|\n)*?>", "");
                                }
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
            ViewBag.PageTagNameName = isHomePage
                ? model.PageData.SiteName
                : $"{model.PageData.Title} - 【{model.PageData.SiteName}】";
            ViewBag.PageTagNameName = HttpUtility.HtmlAttributeEncode(ViewBag.PageTagNameName.Trim());
            ViewData["Description"] = model.PageData.Description;
            ViewBag.GA4 = model.storeSet.GA4;
            ViewBag.GTM = model.storeSet.GTM;
            ViewBag.GoogleAds = model.storeSet.GoogleAds;
            if (shareImage!=null && shareImage.Any()) {
                ViewBag.ImageUrl = new Uri(new Uri(model.root), shareImage[0].Link).AbsoluteUri;
            }
            else ViewBag.ImageUrl = string.IsNullOrEmpty(model.PageData.ImageUrl) ? "" : new Uri(new Uri(model.root), model.PageData.ImageUrl).AbsoluteUri;
            if (isProductPage && productSeoData?.PublicPrice != null)
            {
                var rootUri = new Uri(model.root.EndsWith("/", StringComparison.Ordinal) ? model.root : $"{model.root}/");
                var canonicalUrl = new Uri(
                    rootUri,
                    $"{model.orgName}/search/product/{model.PageData.Id}").AbsoluteUri;
                var productImageUrl = string.IsNullOrWhiteSpace(model.PageData.ImageUrl)
                    ? null
                    : new Uri(rootUri, model.PageData.ImageUrl.TrimStart('/')).AbsoluteUri;

                var productStructuredData = new Dictionary<string, object?>
                {
                    ["@context"] = "https://schema.org",
                    ["@type"] = "Product",
                    ["name"] = productSeoData.Title,
                    ["url"] = canonicalUrl,
                    ["description"] = model.PageData.Description,
                    ["image"] = productImageUrl == null ? null : new[] { productImageUrl },
                    ["sku"] = string.IsNullOrWhiteSpace(productSeoData.ItemNo) ? null : productSeoData.ItemNo,
                    ["offers"] = new Dictionary<string, object?>
                    {
                        ["@type"] = "Offer",
                        ["url"] = canonicalUrl,
                        ["priceCurrency"] = priceCurrency.Code,
                        ["price"] = productSeoData.PublicPrice.Value.ToString("0.################", CultureInfo.InvariantCulture),
                        ["availability"] = productSeoData.IsAvailable
                            ? "https://schema.org/InStock"
                            : "https://schema.org/OutOfStock"
                    }
                };

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
            ViewBag.NoCopy = _env.IsProduction() && NoCopyItem != null && NoCopyItem.value != null && NoCopyItem.value.Count > 0 && NoCopyItem.value[0] == "1" ? "no-right-click" : "";
            ViewData["google.translate"] = model.storeSet.GoogleTranslate;
            ViewData["CurrentUrl"] = model.PageData.CurrentUrl;
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
