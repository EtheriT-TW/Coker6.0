using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Marquee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MarqueeController : Controller
    {
        private readonly IMarqueeAppService marqueeAppService;
        public MarqueeController(
            IMarqueeAppService marqueeAppService
            )
        {
            this.marqueeAppService = marqueeAppService;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> Add(MarqueeDto dto)
        {
            return await marqueeAppService.Add(dto);
        }

        [HttpGet]
        public async Task<MarqueeDto> Get(int id)
        {
            return await marqueeAppService.Get(id);
        }
    }
}
