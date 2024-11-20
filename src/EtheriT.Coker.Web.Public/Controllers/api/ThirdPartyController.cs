using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.ThirdParty;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyController : Controller
    {
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        public ThirdPartyController(
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService)
        {
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayRequest(long ohid)
        {
            return await linePayAppService.LinePayRequest(ohid);
        }
        [HttpGet]
        public async Task<IActionResult> LinePayConfirm(string transactionId, string orderId)
        {
            return await linePayAppService.LinePayConfirm(transactionId, orderId);
        }
        [HttpGet]
        public async Task<IActionResult> LinePayCancel(string transactionId, string orderId)
        {
            return await linePayAppService.LinePayCancel(transactionId, orderId);
        }
        [HttpGet]
        public async Task<LinePayResponseDto> LinePayCheckPaymentStatus(long ohid)
        {
            return await linePayAppService.LinePayCheckPaymentStatus(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PChomePayHeaders()
        {
            return await pchomePayAppService.PChomePayHeaders();
        }
    }
}
