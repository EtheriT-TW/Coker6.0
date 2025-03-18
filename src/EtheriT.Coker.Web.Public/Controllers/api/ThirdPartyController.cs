using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Specification;
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
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ThirdPartyController(
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IECPayAppService ecPayAppService,
            IConfiguration configuration,
            IHttpContextAccessor _httpContextAccessor)
        {
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
            this.ecPayAppService = ecPayAppService;
            this.configuration = configuration;
            this._httpContextAccessor = _httpContextAccessor;
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
        public async Task<ResponseMessageDto> ECPayGetToken(long ohid)
        {
            return await ecPayAppService.ECPayGetToken(ohid);
        }
    }
}
