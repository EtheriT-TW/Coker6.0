using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Models.Header;
using EtheriT.Coker.Web.MVC.Models.Website;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.WebApplication
{
    public class WebApplication: ViewComponent
    {
        private readonly IWebsiteApplication websiteApplication;
        public WebApplication(IWebsiteApplication websiteApplication) { 
            this.websiteApplication = websiteApplication;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            WebsiteInvokeModel result = new WebsiteInvokeModel { 
                webs= await websiteApplication.GetAll()
            };
            return View(result);
        }
    }
}
