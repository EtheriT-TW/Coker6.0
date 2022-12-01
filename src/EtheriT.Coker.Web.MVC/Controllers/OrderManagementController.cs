using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class OrderManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("OrderList");
        }
    }
}
