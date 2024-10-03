using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.SideFloating
{
    public class SideFloating : ViewComponent
    {
        private readonly IConfiguration Configuration;
        private readonly IWebsiteApplication websiteApplication;
        private readonly IAdvertiseAppService advertiseAppService;
        public SideFloating(
            IConfiguration Configuration,
            IWebsiteApplication websiteApplication,
            IAdvertiseAppService advertiseAppService
            )
        {
            this.Configuration = Configuration;
            this.websiteApplication = websiteApplication;
            this.advertiseAppService = advertiseAppService;
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
            var rightSideAds = JsonConvert.DeserializeObject<List<AdvertiseDisplayDto>>(JsonConvert.SerializeObject((await advertiseAppService.GetDisplay(defaultData.Id, 2, 4)).Value));
            if (defaultData.Id != siteId) foreach (var rightSideAd in rightSideAds) if (rightSideAd.FileLink[0].Link != null) rightSideAd.FileLink[0].Link = rightSideAd.FileLink[0].Link.Replace("upload", $"upload/{defaultData.OrgName}");
            SideFloatingViewModel model = new SideFloatingViewModel
            {
                rightSideAd = rightSideAds
            };
            return View(model);
        }
    }
}