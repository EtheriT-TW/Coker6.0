using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        public async Task<List<MarqueeDisplayDto>> GetAll(long webid, string placement)
        {
			var marquee = JsonConvert.DeserializeObject<List<MarqueeDisplayDto>>(JsonConvert.SerializeObject((await marqueeAppService.GetAll(webid, "Top")).Value));
            return marquee??new List<MarqueeDisplayDto>();

        }
    }
}
