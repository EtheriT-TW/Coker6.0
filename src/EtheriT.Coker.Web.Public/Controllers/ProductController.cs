using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EtheriT.Coker.Web.Public.Controllers
{
	public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        /* 有傳入值
        public IActionResult Index(int id)
        {
            return View("_ProdDetail");
        }
        */

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
