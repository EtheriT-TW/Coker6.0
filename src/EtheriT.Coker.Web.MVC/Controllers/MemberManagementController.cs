using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class MemberManagementController : Controller
    {
        public IActionResult MemberSet(int id)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            if (id != 0)
            {
                return View("MemberDetails");
            }
            return View("MemberData");
        }

        public IActionResult MemberList()
        {
            return View("MemberList");
        }

        public IActionResult UserType()
        {
            return View("UserType");
        }
        public IActionResult MemberType()
        {
            return View("MemberType");
        }

        public IActionResult SelfData() {
			return View("SelfData");
		}
        public IActionResult ManagerList(int id)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest);
            return View("ManagerList");
        }
    }
}
