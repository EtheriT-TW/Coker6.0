using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class FileManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
