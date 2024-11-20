using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.ThirdParty;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

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
        public async Task<ResponseMessageDto> PayRequest(long ohid, string paytype)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (paytype)
            {
                case "LinePay":
                    return await linePayAppService.LinePayRequest(ohid);
                case "PCHomePay":
                    return await pchomePayAppService.PChomePayRequest(ohid);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
        //[HttpGet]
        //public async Task<ResponseMessageDto> LinePayRequest(long ohid)
        //{
        //    return await linePayAppService.LinePayRequest(ohid);
        //}
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
        public async Task<PChomePayStateDto> PChomePayCheckPaymentStatus(long ohid)
        {
            return await pchomePayAppService.PChomePayCheckPaymentStatus(ohid);
        }
        //[HttpGet]
        //public async Task<ResponseMessageDto> PChomePayReturn()
        //{
        //    return await pchomePayAppService.PChomePayReturn();
        //}
        //[HttpGet]
        //public async Task<ResponseMessageDto> PChomePayFailReturn()
        //{
        //    return await pchomePayAppService.PChomePayFailReturn();
        //}
        //[HttpPost]
        //public async Task<string> PChomePayNotify(PChomePayNotifyDto dto)
        //{
        //    return await pchomePayAppService.PChomePayNotify(dto);
        //}
    }
}
