using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using System.Security.Cryptography;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ThirdPartyController
    {
        private readonly IThirdPartyAppService thirdPartyAppService;
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ThirdPartyController(
            IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.thirdPartyAppService = thirdPartyAppService;
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
            this.configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto)
        {
            return await thirdPartyAppService.SaveThirdParty(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayConfirm(long ohid)
        {
            return await linePayAppService.LinePayConfirm(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayVoid(long ohid)
        {
            return await linePayAppService.LinePayVoid(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PayRefund(string payment, long ohid, int? refund)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (payment)
            {
                case "LINEPay":
                    return await linePayAppService.LinePayRefund(ohid, refund);
                case "支付連":
                    return await pchomePayAppService.PChomePayRefund(ohid, refund);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> CheckPaymentStatus(long ohid, int thirdparty)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (thirdparty)
            {
                case 2:
                    return await pchomePayAppService.PChomePayCheckPaymentStatus(ohid);
                case 3:
                    return await linePayAppService.LinePayCheckPaymentStatus(ohid);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
        [HttpGet]
        public async Task<IActionResult> PChomePayReturn(string ohid)
        {
            return await pchomePayAppService.PChomePayReturn(ohid);
        }
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<string> PChomePayNotify([FromForm] PChomePayNotifyDto dto)
        {
            var remoteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var allowedIps = configuration.GetValue<List<string>>("WebConfig:SourceIP");

            Console.WriteLine($"-------------PChomePayNotify來源查看-------------");

            if (string.IsNullOrEmpty(remoteIp) || !allowedIps.Contains(remoteIp))
            {
                Console.WriteLine($"不允許的來源：{remoteIp}");
                return "Forbidden: IP not allowed";
            }
            else Console.WriteLine($"允許的來源：{remoteIp}");

            return await pchomePayAppService.PChomePayNotify(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PChomePayBalance()
        {
            return await pchomePayAppService.PChomePayBalance();
        }
        [HttpGet]
        public async Task<ResponseMessageDto> CheckRefund(string payment, string transactionId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (payment)
            {
                case "LINEPay":
                    return await linePayAppService.LinePayRefundState(transactionId);
                case "支付連":
                    return await pchomePayAppService.PChomePayRefundState(transactionId);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
    }
}
