using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class AdvertisementManagementController : Controller
    {
        public IActionResult CustomAd() => View();
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
