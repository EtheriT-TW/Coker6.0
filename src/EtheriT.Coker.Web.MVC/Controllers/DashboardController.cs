using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
