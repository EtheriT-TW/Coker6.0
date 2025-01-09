using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyController : Controller
    {
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        private readonly IECPayAppService ecPayAppService;
        public ThirdPartyController(
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IECPayAppService ecPayAppService)
        {
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
            this.ecPayAppService = ecPayAppService;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PayRequest(long ohid, string paytype)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (paytype)
            {
                case "3":
                case "LinePay":
                    return await linePayAppService.LinePayRequest(ohid);
                case "2":
                case "PCHomePay":
                    return await pchomePayAppService.PChomePayRequest(ohid);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
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
        public async Task<IActionResult> PChomePayReturn(string ohid)
        {
            return await pchomePayAppService.PChomePayReturn(ohid);
        }
        [HttpPost]
        public async Task<string> PChomePayNotify(PChomePayNotifyDto dto)
        {
            return await pchomePayAppService.PChomePayNotify(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ECPayGetToken(long ohid)
        {
            return await ecPayAppService.ECPayGetToken(ohid);
        }
    }
}
