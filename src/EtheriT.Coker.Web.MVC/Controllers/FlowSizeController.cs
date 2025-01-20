using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
	public class FlowSizeController : Controller
	{
		private readonly ILogger<SearchManagementController> _logger;
		public FlowSizeController(ILogger<SearchManagementController> logger)
		{
			_logger = logger;
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
