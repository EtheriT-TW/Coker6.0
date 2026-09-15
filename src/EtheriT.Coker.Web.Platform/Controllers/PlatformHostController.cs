using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Controllers;

public sealed class PlatformHostController(ViteManifestService viteManifest) : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        return View(viteManifest.GetEntryAssets());
    }
}
