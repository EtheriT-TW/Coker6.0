using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class AuditLogsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
