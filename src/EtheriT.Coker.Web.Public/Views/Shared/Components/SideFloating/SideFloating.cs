using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Common;
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
        private readonly StringHandler stringHandler;
        public SideFloating(
            IConfiguration Configuration,
            IWebsiteApplication websiteApplication,
            IAdvertiseAppService advertiseAppService,
            StringHandler stringHandler
        )
        {
            this.Configuration = Configuration;
            this.websiteApplication = websiteApplication;
            this.advertiseAppService = advertiseAppService;
            this.stringHandler = stringHandler;
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
            var rightSideAds = JsonConvert.DeserializeObject<List<AdvertiseDisplayDto>>(JsonConvert.SerializeObject((await advertiseAppService.GetDisplay(defaultData.Id, 2, 4)).Value)) ?? new List<AdvertiseDisplayDto>();
            if (defaultData.Id != siteId)
            {
                foreach (var rightSideAd in rightSideAds)
                {
                    var file = rightSideAd.FileLink?.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(file?.Link))
                    {
                        file.Link = file.Link.Replace("upload", $"upload/{defaultData.OrgName}");
                    }

                    rightSideAd.Html = stringHandler.ResolveUploadPath(rightSideAd.Html ?? "", defaultData.OrgName);
                    rightSideAd.Css = stringHandler.ResolveUploadPath(rightSideAd.Css ?? "", defaultData.OrgName);
                }
            }

            var nonce = HttpContext.Items["CSPNonce"] as string ?? "";
            SideFloatingViewModel model = new SideFloatingViewModel
            {
                rightSideAd = rightSideAds,
                WebsiteId = siteId,
                Nonce = nonce,
            };
            return View(model);
        }
    }
}