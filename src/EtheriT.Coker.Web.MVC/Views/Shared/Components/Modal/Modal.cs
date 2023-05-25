using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Modal
{
    public class Modal : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string dispaly)
        {
            if (dispaly == "TagListModal")
            {
                return View("TagListModal");
            }else if(dispaly == "TechCertListModal")
            {
                return View("TechCertListModal");
            }
            return View();
        }
    }
}