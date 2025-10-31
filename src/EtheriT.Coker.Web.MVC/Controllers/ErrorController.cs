using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("{code:int}")]
        public IActionResult Code(int code)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return View("~/Views/Error/400.cshtml");
            }
            Response.StatusCode = code;

            var viewPath = code switch
            {
                400 => "~/Views/Error/400.cshtml",
                404 => "~/Views/Error/404.cshtml",
                500 => "~/Views/Error/500.cshtml",
                _ => "~/Views/Error/Index.cshtml"
            };

            return View(viewPath);
        }
        [Route("500")]
        public IActionResult E500()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("~/Views/Error/500.cshtml");
        }
    }
}
