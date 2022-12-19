using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ContentManagementController : Controller
    {
        public IActionResult NewArticle()
        {
            return View("NewArticle");
        }
        public IActionResult Marquee()
        {
            return View("Marquee");
        }
        public IActionResult EnterAd(int id)
        {
            return View("EnterAd");
        }
        public IActionResult RightSideAd(int id)
        {
            return View("RightSideAd");
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
