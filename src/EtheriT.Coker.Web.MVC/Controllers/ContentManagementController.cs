using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ContentManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("MarqueeMessage");
        }
        public IActionResult Marquee()
        {
            return View("Marquee");
        }
        public IActionResult ContactUs(int id)
        {
            if (id != 0)
            {
                return View("ContactUsReply");
            }
            return View("ContactUs");
        }
    }
}
