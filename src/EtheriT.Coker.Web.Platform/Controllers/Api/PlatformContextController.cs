using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.Web.Platform.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/platform-context")]
public sealed class PlatformContextController(
    IConfiguration configuration,
    IOptions<BackofficeSessionOptions> sessionOptions,
    IAntiforgery antiforgery,
    PlatformReauthenticationTicketService reauthenticationTicketService,
    PlatformAuthorizationService platformAuthorizationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var antiforgeryTokens = antiforgery.GetAndStoreTokens(HttpContext);
        var roleCodes = await platformAuthorizationService.GetRoleCodesAsync(
            User.Identity?.Name,
            HttpContext.RequestAborted);
        var isAdministrator = roleCodes.Contains(EtheriT.Coker.Core.Models.PlatformRoleCodes.Administrator);

        return Ok(new
        {
            UserName = User.Identity?.Name ?? "-",
            MvcUrl = configuration["SystemLinks:MvcUrl"]?.TrimEnd('/') ?? "/",
            SessionActivityIntervalSeconds = Math.Max(
                60,
                (int)sessionOptions.Value.ActivityPingInterval.TotalSeconds),
            AntiforgeryToken = antiforgeryTokens.RequestToken ?? string.Empty,
            ReauthenticationTicket = reauthenticationTicketService.Create(
                User.Identity?.Name ?? string.Empty),
            CanManagePlatformData = isAdministrator ||
                roleCodes.Contains(EtheriT.Coker.Core.Models.PlatformRoleCodes.DataManager),
            CanControlServers = isAdministrator ||
                roleCodes.Contains(EtheriT.Coker.Core.Models.PlatformRoleCodes.ServerOperator),
            CanManagePlatformRoles = isAdministrator
        });
    }
}
