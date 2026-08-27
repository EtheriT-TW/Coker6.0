using EtheriT.Coker.Application;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public sealed class RobotsController : Controller
    {
        private readonly LoginUserData loginUserData;
        private readonly ILogger<RobotsController> logger;

        public RobotsController(
            LoginUserData loginUserData,
            ILogger<RobotsController> logger)
        {
            this.loginUserData = loginUserData;
            this.logger = logger;
        }

        [HttpGet]
        [Produces("text/plain")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index()
        {
            var websiteUrl = await loginUserData.GetFrontWebsiteUrl();

            if (!Uri.TryCreate(websiteUrl, UriKind.Absolute, out var websiteUri) ||
                (websiteUri.Scheme != Uri.UriSchemeHttp &&
                 websiteUri.Scheme != Uri.UriSchemeHttps))
            {
                logger.LogError(
                    "無法產生 robots.txt：網站 DefaultUrl 設定不正確。DefaultUrl={DefaultUrl}",
                    websiteUrl);

                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var sitemapUrl = new Uri(websiteUri, "/Sitemap").AbsoluteUri;
            var content = $"User-agent: *\nAllow: /\n\nSitemap: {sitemapUrl}\n";

            return Content(content, "text/plain", Encoding.UTF8);
        }
    }
}
