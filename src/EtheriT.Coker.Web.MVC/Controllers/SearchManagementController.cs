using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class SearchManagementController : Controller
    {
        private readonly ILogger<SearchManagementController> _logger;
        public SearchManagementController(ILogger<SearchManagementController> logger)
        {
            _logger = logger;
        }
        public ActionResult CustSearch() {
            return View();
        }
    }
}
