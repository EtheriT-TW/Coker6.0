using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/platform-context")]
public sealed class PlatformContextController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            UserName = User.Identity?.Name ?? "-",
            MvcUrl = configuration["SystemLinks:MvcUrl"]?.TrimEnd('/') ?? "/"
        });
    }
}
