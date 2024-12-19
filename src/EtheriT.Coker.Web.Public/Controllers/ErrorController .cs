using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            var viewName = statusCode switch
            {
                404 => "NotFound",
                _ => "Error"
            };
            ViewData["Locale"] = "zh-tw";
            return View(viewName);
        }

        [Route("Error")]
        public IActionResult HandleError()
        {
            ViewData["Locale"] = "zh-tw";
            return View("Error");
        }
    }
}
