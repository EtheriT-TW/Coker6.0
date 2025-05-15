using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class TemplateController : Controller
    {
        public IActionResult HeaderSettings()
        {
            return View();
        }
        public IActionResult FooterSettings()
        {
            return View();
        }
    }
}
