using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class ProductManagementController : Controller
    {
        public IActionResult ProductList()
        {
            return View("ProductList");
        }
        public IActionResult TechnicalCertificate()
        {
            return View("TechnicalCertificate");
        }
    }
}
