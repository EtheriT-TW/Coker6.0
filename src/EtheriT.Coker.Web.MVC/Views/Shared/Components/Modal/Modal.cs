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
            }
            return View();
        }
    }
}