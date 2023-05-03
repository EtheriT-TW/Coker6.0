
using EtheriT.Coker.Application;
using EtheriT.Coker.Web.Public.Views.Shared.Components.Header;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.LayoutType
{
    public class LayoutType : ViewComponent
    {
        private readonly IWebsiteApplication websiteApplication;
        private readonly IConfiguration Configuration;
        public LayoutType(
            IWebsiteApplication websiteApplication,
            IConfiguration Configuration
            )
        {
            this.websiteApplication = websiteApplication;
            this.Configuration = Configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var Layout_Type = await websiteApplication.GetLayoutType(siteId);
            var orgname = await websiteApplication.GetOrgName(siteId);
            LayoutTypeViewModel layoutTypeViewModel = new LayoutTypeViewModel
            {
                SiteName = Layout_Type == 0 ? "Default_Site" : $"~/css/Site/Layout_{Layout_Type}_Site.min.css",
                OrgName = orgname,
                LayoutType = Layout_Type,
            };

            return View(layoutTypeViewModel);
        }
    }
}
