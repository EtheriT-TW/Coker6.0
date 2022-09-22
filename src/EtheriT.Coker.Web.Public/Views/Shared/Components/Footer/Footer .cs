using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Footer
{
    public class Footer : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
