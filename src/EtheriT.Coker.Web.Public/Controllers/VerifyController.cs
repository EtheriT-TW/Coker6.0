using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EtheriT.Coker.Web.Public.Controllers
{
    public class VerifyController : Controller
    {
        private readonly IConfiguration Configuration;
        public VerifyController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IActionResult Index(string option,string? key)
        {
            string verifyString = Configuration.GetValue<string>($"Verify:{option}:{key}");
            if(string.IsNullOrEmpty(key)) verifyString = Configuration.GetValue<string>($"Verify:{option}");
            if (!string.IsNullOrEmpty(verifyString)) return Content(verifyString, "text/plain");
            else {
                Response.StatusCode = 404;
                return Content("Verify Error", "text/plain"); 
            }

        }
    }
}
