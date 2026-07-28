using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Web.Public.Services;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class UserStatisticController : Controller
    {
        private readonly IRemoteAppService remoteAppService;
        private readonly RemoteTrackingTokenService remoteTrackingTokenService;

        public UserStatisticController(
            IRemoteAppService remoteAppService,
            RemoteTrackingTokenService remoteTrackingTokenService)
        {
            this.remoteAppService = remoteAppService;
            this.remoteTrackingTokenService = remoteTrackingTokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Collect(RemoteTrackingCollectDto dto)
        {
            if (!ModelState.IsValid
                || dto.EventId == Guid.Empty
                || !remoteTrackingTokenService.TryUnprotect(dto.Token, out var page))
            {
                return BadRequest();
            }

            await remoteAppService.CollectRemoteTracking(page, dto);
            return NoContent();
        }
    }
}
