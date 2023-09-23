using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class MemberManagementController : Controller
    {
        public IActionResult Index(int id)
        {
            if (id != 0)
            {
                return View("MemberDetails");
            }
            return View("MemberData");
        }
        public IActionResult SelfData() {
			return View("SelfData");
		}
    }
}
