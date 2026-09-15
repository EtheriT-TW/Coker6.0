using EtheriT.Coker.Authentication.Backoffice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/platform-context")]
public sealed class PlatformContextController(
    IConfiguration configuration,
    IOptions<BackofficeSessionOptions> sessionOptions) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            UserName = User.Identity?.Name ?? "-",
            MvcUrl = configuration["SystemLinks:MvcUrl"]?.TrimEnd('/') ?? "/",
            SessionActivityIntervalSeconds = Math.Max(
                60,
                (int)sessionOptions.Value.ActivityPingInterval.TotalSeconds)
        });
    }
}
