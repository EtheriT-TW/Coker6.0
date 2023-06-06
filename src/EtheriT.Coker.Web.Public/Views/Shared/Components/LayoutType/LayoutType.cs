
using EtheriT.Coker.Application;
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
            var website = HttpContext.GetRouteData().Values["website"];
            if (website == null)
            {
                website = HttpContext.GetRouteData().Values["key"];
            }
            if (website != null && !website.ToString().Equals("upload"))
            {
                var tempid = await websiteApplication.GetSiteId(siteId, website.ToString());
                if (tempid != 0)
                {
                    siteId = await websiteApplication.GetSiteId(siteId, website.ToString());
                }
            }
            var orgname = await websiteApplication.GetOrgName(siteId);
            orgname = (orgname == null || orgname == "") ? "Page" : orgname;
            var Layout_Type = await websiteApplication.GetLayoutType(siteId);
            var view = Layout_Type == 0 ? "Default" : $"Layout_{Layout_Type}";
            LayoutTypeViewModel layoutTypeViewModel = new LayoutTypeViewModel
            {
                SiteId = siteId,
                SiteName = Layout_Type == 0 ? "Default_Site" : $"~/css/Site/Layout_{Layout_Type}_Site.min.css",
                OrgName = orgname,
                LayoutType = Layout_Type,
            };

            return View(layoutTypeViewModel);
        }
    }
}
