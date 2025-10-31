using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class AdvertisementManagementController : Controller
    {
        private readonly NavigationProvider navigation;
        public AdvertisementManagementController(NavigationProvider navigation) {
            this.navigation = navigation;
        }
        public IActionResult CustomAd(int id)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            return View("CustomAd");
        }
        public IActionResult EnterAd(int id)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            return View("EnterAd");
        }
        public IActionResult RightSideAd(int id)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            return View("RightSideAd");
        }
    }
}
