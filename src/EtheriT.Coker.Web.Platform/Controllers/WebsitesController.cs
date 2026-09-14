using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Controllers;

public class WebsitesController : Controller
{
    public IActionResult Index() => View();
}
