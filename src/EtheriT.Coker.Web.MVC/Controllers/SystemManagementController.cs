using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class SystemManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("WebData");
        }
        public IActionResult SEO()
        {
            return View("SEO");
        }
    }
}
