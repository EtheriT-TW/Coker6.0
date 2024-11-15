using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyController : Controller
    {
        private readonly ILinePayAppService linePayAppService;
        public ThirdPartyController(
            ILinePayAppService linePayAppService)
        {
            this.linePayAppService = linePayAppService;
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
        public async Task<LinePayResponseDto> LinePayCheckPaymentStatus(string transactionId, string orderId)
        {
            return await linePayAppService.LinePayCheckPaymentStatus(transactionId, orderId);
        }
    }
}
