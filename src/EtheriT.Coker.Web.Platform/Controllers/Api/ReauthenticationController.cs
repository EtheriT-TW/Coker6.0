using EtheriT.Coker.Web.Platform.Models;
using EtheriT.Coker.Web.Platform.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/reauthentication")]
public sealed class ReauthenticationController(
    PlatformReauthenticationService reauthenticationService) : ControllerBase
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting("backoffice-reauthentication")]
    [HttpPost]
    public async Task<ActionResult<ReauthenticateResponse>> Reauthenticate(
        ReauthenticateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("X-Requested-With", out var requestedWith) ||
            requestedWith != "XMLHttpRequest")
        {
            return BadRequest();
        }

        return Ok(await reauthenticationService.ReauthenticateAsync(
            request,
            cancellationToken));
    }
}
