using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Modal
{
    public class Modal : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string dispaly)
        {
            if (dispaly == "ShoppingCarModal")
            {
                return View("ShoppingCarModal");
            }
            return View();
        }
    }
}
