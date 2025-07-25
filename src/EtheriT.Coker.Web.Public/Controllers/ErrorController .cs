using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
            PrepareErrorViewBag();
            ViewData["PageTagNameName"] = "頁面不存在";
            return View(viewName);
        }

        [Route("Error")]
        public IActionResult HandleError()
        {
            PrepareErrorViewBag();
            return View("Error");
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
            ViewData["VisibleHeader"] = "false";
            ViewData["VisibleFooter"] = "false";
        }
    }
}
