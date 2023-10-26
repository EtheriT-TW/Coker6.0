using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class WelcomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
