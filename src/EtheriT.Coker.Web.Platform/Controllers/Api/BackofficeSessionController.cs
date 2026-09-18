using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/backoffice-session")]
public sealed class BackofficeSessionController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        Response.Headers.CacheControl = "no-store";
        return NoContent();
    }

    [HttpPost("activity")]
    public IActionResult ReportActivity()
    {
        if (!Request.Headers.TryGetValue("X-Requested-With", out var requestedWith) ||
            requestedWith != "XMLHttpRequest")
        {
            return BadRequest();
        }

        // Cookie validation has already checked and, when necessary, renewed the
        // persistent session before this action executes.
        return NoContent();
    }
}
