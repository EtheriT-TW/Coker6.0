using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ThemeController : Controller
    {
        public IActionResult Index()
        {
            return View("ThemeColor");
        }
    }
}
