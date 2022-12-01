using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ProductManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("ProductList");
        }
    }
}
