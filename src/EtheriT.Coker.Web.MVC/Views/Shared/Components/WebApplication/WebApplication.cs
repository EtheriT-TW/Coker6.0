using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.WebApplication
{
    public class WebApplication : ViewComponent
    {
        public IViewComponentResult Invoke() => View();
    }
}
