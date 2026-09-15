using EtheriT.Coker.Authentication.Backoffice;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/navigation-preference")]
public sealed class NavigationPreferenceController(
    BackofficeNavigationPreferenceCookie preferenceCookie) : ControllerBase
{
    [HttpPost]
    public IActionResult Save(NavigationPreferenceRequest request)
    {
        preferenceCookie.Write(
            HttpContext,
            User.Identity?.Name ?? string.Empty,
            BackofficeSystem.Platform,
            request.Path);

        return NoContent();
    }
}

public sealed record NavigationPreferenceRequest(string? Path);
