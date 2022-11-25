using DevExtreme.AspNet.Mvc;
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
        public async Task<ResponseMessageDto> Add(MarqueeAddDto dto)
        {
            return await marqueeAppService.Add(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Update(MarqueeUpdateDto dto)
        {
            return await marqueeAppService.Update(dto);
        }
        [HttpGet]
        public async Task<MarqueeGetDto> Get(int id)
        {
            return await marqueeAppService.Get(id);
        }

        [HttpGet]
        public async Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions)
        {
            return await marqueeAppService.GetAll(loadOptions);
        }

        [HttpGet]
        public async Task<ResponseMessageDto> Delete(int id)
        {
            return await marqueeAppService.Delete(id);
        }
    }
}
