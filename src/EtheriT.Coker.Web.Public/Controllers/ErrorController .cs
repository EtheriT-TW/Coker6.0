using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace EtheriT.Coker.Web.Public.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        [Route("{statusCode:int}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                PrepareErrorViewBag();
                return View("Error");
            }
            Response.StatusCode = statusCode >= 400 && statusCode <= 599
                ? statusCode
                : StatusCodes.Status500InternalServerError;
            var resourceError = CreateResourceError();
            if (resourceError != null) return resourceError;
            var viewName = statusCode switch
            {
                404 => "NotFound",
                401 or 403 => "Denied",
                _ => "Error"
            };
            PrepareErrorViewBag();
            ViewData["PageTagNameName"] = viewName == "NotFound" ? "頁面不存在" : "錯誤頁面";
            return View(viewName);
        }

        [Route("")]
        public IActionResult HandleError()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            var resourceError = CreateResourceError();
            if (resourceError != null) return resourceError;
            PrepareErrorViewBag();
            return View("Error");
        }
        private IActionResult? CreateResourceError()
        {
            // Re-execution changes Request.Path; classify the original request.
            var originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath
                ?? HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path
                ?? Request.Path.Value ?? "";
            var path = new PathString(originalPath);
            if (path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(new { status = Response.StatusCode })
                {
                    StatusCode = Response.StatusCode
                };
            }
            var destination = Request.Headers["Sec-Fetch-Dest"].ToString();
            if (destination is "image" or "script" or "style" or "font" or "audio" or "video"
                || path.StartsWithSegments("/upload", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/Shared", StringComparison.OrdinalIgnoreCase))
            {
                return new ContentResult
                {
                    StatusCode = Response.StatusCode,
                    ContentType = "text/plain; charset=utf-8",
                    Content = Response.StatusCode.ToString()
                };
            }
            return null;
        }
        private void PrepareErrorViewBag()
        {
            ViewBag.PageTagNameName = "錯誤頁面";
            ViewBag.ImageUrl = null;
            ViewBag.Nonce = "";
            ViewBag.PageKey = "error";
            ViewBag.SiteId = 0;
            ViewBag.BackstageUrl = "";
            ViewBag.OAuthSuccess = "";
            ViewBag.OAuthError = "";
            ViewBag.RootId = 0;
            ViewBag.priceOrder = false;
            ViewBag.LoginEnable = false;
            ViewBag.NoCopy = "";
            ViewBag.option = "none";
            ViewBag.Css = "";
            ViewBag.SearchWord = JsonConvert.SerializeObject("");
            var nonce = HttpContext.Items["CSPNonce"] as string;
            ViewBag.Nonce = nonce;

            ViewData["Locale"] = "zh-tw";
            ViewData["PageView"] = "Default";
            ViewData["Root"] = "/";
            ViewData["OrgName"] = "error";
            ViewData["Id"] = "0";
            ViewData["CurrentUrl"] = HttpContext.Request.Path;
            ViewData["Description"] = "目前頁面發生錯誤";
            ViewData["SideName"] = "網站名稱";
            ViewData["Layout"] = "ErrorLayout";
            ViewData["bodyClass"] = "";
            ViewData["VisibleHeader"] = "true";
            ViewData["VisibleFooter"] = "true";
        }
    }
}
