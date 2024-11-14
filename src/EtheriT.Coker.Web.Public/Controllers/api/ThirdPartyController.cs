using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyController : Controller
    {
        //private readonly IThirdPartyAppService thirdPartyAppService;
        private readonly ILinePayAppService linePayAppService;
        public ThirdPartyController(
            //IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService)
        {
            //this.thirdPartyAppService = thirdPartyAppService;
            this.linePayAppService = linePayAppService;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayRequest(long ohid)
        {
            return await linePayAppService.LinePayRequest(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayConfirm(long ohid)
        {
            return await linePayAppService.LinePayConfirm(ohid);
        }
    }
}
