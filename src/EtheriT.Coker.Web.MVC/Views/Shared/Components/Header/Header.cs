using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Header
{
    public class Header : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
