using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.Front
{
    [Route("front/[controller]/[action]")]
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View("~/Views/Front/Auth/Login.cshtml");
        }
    }
}
