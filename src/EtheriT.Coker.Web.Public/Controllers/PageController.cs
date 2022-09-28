using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class PageController : Controller
    {
        private readonly ILogger<PageController> _logger;

        public PageController(ILogger<PageController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string key, int id, string search)
        {
            string view = string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                view = "Product";
                if (id != 0) view = "ProductContent";
            }
            else {
                view = "index";
            }
            return View(view);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
