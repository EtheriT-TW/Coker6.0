using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class ContentManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("MarqueeMessage");
        }
        public IActionResult MarqueeMessage()
        {
            return View("MarqueeMessage");
        }
    }
}
