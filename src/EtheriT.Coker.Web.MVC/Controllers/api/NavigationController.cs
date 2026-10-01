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
        if (!await loginUserData.CanAccessMvc() && await loginUserData.CanAccessPlatform())
        {
            var configuredPlatformUrl = configuration["SystemLinks:PlatformUrl"];
            if (!Uri.TryCreate(configuredPlatformUrl, UriKind.Absolute, out var platformUri) ||
                (platformUri.Scheme != Uri.UriSchemeHttp && platformUri.Scheme != Uri.UriSchemeHttps))
                return Problem("尚未設定有效的客戶管理平台網址。");

            return Ok(new { Url = $"{platformUri.AbsoluteUri.TrimEnd('/')}/" });
        }

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

        if (!await loginUserData.CanAccessPlatform())
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
