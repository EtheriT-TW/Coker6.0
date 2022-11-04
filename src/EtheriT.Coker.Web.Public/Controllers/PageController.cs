using EtheriT.Coker.Web.Public.Models;
using Microsoft.AspNetCore.Mvc;
using SimpleCaptcha;
using System.Diagnostics;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class PageController : Controller
    {
        private readonly ILogger<PageController> _logger;
        private readonly ICaptcha _captcha;

        public PageController(ILogger<PageController> logger, ICaptcha captcha)
        {
            _logger = logger;
            _captcha = captcha;
        }

        public IActionResult Index(string key, int id, string search)
        {
            PageViewModel model = new PageViewModel
            {
                id = id,
                search = search ?? "".Trim()
            };
            string view = string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                if (key == "Search" || key == "ShoppingCar" || key == "Favorites" || key == "Contact" || key == "Catalog")
                {
                    view = key;
                }
                else if (key == "OrderManagement")
                {
                    view = key;
                }
                else
                {
                    view = "Product";
                    if (id != 0) view = "ProductContent";
                }
            }
            else
            {
                view = "index";
            }
            return View(view, model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Captcha(string id)
        {
            var info = _captcha.Generate(id);
            var stream = new MemoryStream(info.CaptchaByteData);
            return File(stream, "image/png");
        }

        public IActionResult Validate(string id, string code)
        {
            var result = _captcha.Validate(id, code);
            return Json(new { success = result });
        }
    }
}
