using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/session")]
public sealed class SessionController(
    CokerDbContext db,
    IConfiguration configuration,
    ILogger<SessionController> logger) : ControllerBase
{
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var sessionValue = User.FindFirstValue(ClaimTypes.Sid);
        if (Guid.TryParse(sessionValue, out var sessionId))
        {
            try
            {
                var token = await db.Tokens.FirstOrDefaultAsync(
                    item => item.id == sessionId,
                    HttpContext.RequestAborted);
                if (token != null)
                {
                    if (token.UserID.HasValue)
                    {
                        db.Account_Logs.Add(new Account_Log
                        {
                            UUID = token.UUID,
                            WebsiteId = token.websiteId,
                            Status = (int)AccountStatusEnum.登出,
                            CreatorUserId = token.UserID.Value,
                            LastLoginTime = DateTime.Now,
                            CreationTime = DateTime.Now
                        });
                    }

                    db.Tokens.Remove(token);
                    await db.SaveChangesAsync(HttpContext.RequestAborted);
                }
            }
            catch (Exception exception)
            {
                // Local sign-out still proceeds; an orphaned token expires through normal session lifetime rules.
                logger.LogError(exception, "Platform logout could not revoke session {SessionId}.", sessionId);
            }
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        DeleteLegacyCookie("BackstageToken");
        DeleteLegacyCookie("BackstageRefreshToken");

        return NoContent();
    }

    private void DeleteLegacyCookie(string name)
    {
        var options = new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.Lax
        };
        var cookieDomain = configuration["BackofficeAuthentication:CookieDomain"];
        if (!string.IsNullOrWhiteSpace(cookieDomain))
        {
            options.Domain = cookieDomain;
        }

        Response.Cookies.Delete(name, options);
        // Older MVC versions created these as host-only cookies.
        if (!string.IsNullOrWhiteSpace(cookieDomain))
        {
            Response.Cookies.Delete(name, new CookieOptions { Path = "/", Secure = true });
        }
    }
}
