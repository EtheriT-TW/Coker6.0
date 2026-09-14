using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
