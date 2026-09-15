using EtheriT.Coker.Application;
using EtheriT.Coker.Authentication.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api;

[ApiController]
[Authorize]
[Route("api/navigation")]
public sealed class NavigationController(
    IConfiguration configuration,
    LoginUserData loginUserData,
    BackofficeNavigationPreferenceCookie preferenceCookie) : ControllerBase
{
    [HttpGet("post-login-destination")]
    public async Task<IActionResult> GetPostLoginDestination(string? returnUrl = null)
    {
        // A returnUrl produced by MVC session expiry has priority over the stored location.
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            BackofficeNavigationPreferenceCookie.TryNormalizePath(
                BackofficeSystem.Mvc,
                returnUrl,
                out var safeReturnUrl))
        {
            return Ok(new { Url = safeReturnUrl });
        }

        var preference = preferenceCookie.Read(HttpContext);
        var account = User.Identity?.Name;
        if (preference == null ||
            string.IsNullOrWhiteSpace(account) ||
            !string.Equals(preference.Account, account, StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new { Url = "/Welcome" });
        }

        if (preference.System == BackofficeSystem.Mvc)
        {
            return Ok(new { Url = preference.Path });
        }

        if (!await loginUserData.isSystemUser())
        {
            return Ok(new { Url = "/Welcome" });
        }

        var platformUrl = configuration["SystemLinks:PlatformUrl"];
        if (!Uri.TryCreate(platformUrl, UriKind.Absolute, out var platformBaseUri) ||
            (platformBaseUri.Scheme != Uri.UriSchemeHttps && platformBaseUri.Scheme != Uri.UriSchemeHttp))
        {
            return Ok(new { Url = "/Welcome" });
        }

        var baseUri = new Uri($"{platformBaseUri.AbsoluteUri.TrimEnd('/')}/");
        var destination = new Uri(baseUri, preference.Path.TrimStart('/'));
        return Ok(new { Url = destination.AbsoluteUri });
    }
}
