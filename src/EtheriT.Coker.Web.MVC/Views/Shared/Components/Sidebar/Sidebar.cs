using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Sidebar : ViewComponent
    {
        public Sidebar() { 
        
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
