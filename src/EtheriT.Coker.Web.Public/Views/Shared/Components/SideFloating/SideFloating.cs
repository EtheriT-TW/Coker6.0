using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.HtmlContent;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.SideFloating
{
    public class SideFloating : ViewComponent
    {

        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IConfiguration Configuration;
        private readonly IWebsiteApplication websiteApplication;
        public SideFloating(
            IHtmlContentAppService htmlContentAppService,
            IConfiguration Configuration,
            IWebsiteApplication websiteApplication
            )
        {
            this.htmlContentAppService = htmlContentAppService;
            this.Configuration = Configuration;
            this.websiteApplication = websiteApplication;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var website = HttpContext.GetRouteData().Values["website"];
            if (website == null)
            {
                website = HttpContext.GetRouteData().Values["key"];
            }
            var website_str = website == null ? "" : website.ToString();
            var defaultData = await websiteApplication.GetDefaultData(siteId, website_str);
            var rightSideAd = JsonConvert.DeserializeObject<List<HtmlContentDisplayDto>>(JsonConvert.SerializeObject((await htmlContentAppService.GetDisplay(defaultData.Id, 12, 4)).Value));
            SideFloatingViewModel model = new SideFloatingViewModel
            {
                rightSideAd = rightSideAd
            };
            return View(model);
        }
    }
}