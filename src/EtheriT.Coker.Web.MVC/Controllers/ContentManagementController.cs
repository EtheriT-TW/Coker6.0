using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ContentManagementController : Controller
    {
        private readonly NavigationProvider navigation;
        public ContentManagementController(NavigationProvider navigation) {
            this.navigation = navigation;
        }
        public IActionResult Tag()
        {
            return View("Tag");
        }
        public async Task<IActionResult> Directory()
        {
            var site = await navigation.getMenus();
            await navigation.SetPower(site);
            await navigation.setUserJob(site);
            return View("Directory");
        }
        public IActionResult Article()
        {
            return View("Article");
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
