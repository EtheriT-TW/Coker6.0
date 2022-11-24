using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MarqueeController : Controller
    {

        private readonly IMarqueeAppService marqueeAppService;
        public MarqueeController(
            IMarqueeAppService marqueeAppService
            )
        {
            this.marqueeAppService = marqueeAppService;
        }

        [HttpGet]
        public async Task<JsonResult> GetAll(long webid)
        {
            return await marqueeAppService.GetAll(webid);
        }

    }
}
