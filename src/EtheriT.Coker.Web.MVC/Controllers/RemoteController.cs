using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class RemoteController : Controller
	{
		private readonly ILogger<SearchManagementController> _logger;
		public RemoteController(ILogger<SearchManagementController> logger)
		{
			_logger = logger;
		}
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Page() {
			return View();
		}
	}
}
