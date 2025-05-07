using DevExpress.XtraRichEdit.Commands.Internal;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Specification;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ThirdPartyController : Controller
    {
        private readonly IThirdPartyAppService thirdPartyAppService;
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        private readonly IECPayAppService ecPayAppService;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ThirdPartyController(
            IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IECPayAppService ecPayAppService,
            IConfiguration configuration,
            IHttpContextAccessor _httpContextAccessor)
        {
            this.thirdPartyAppService = thirdPartyAppService;
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
                case "2":
                case "PCHomePay":
                    return await pchomePayAppService.PChomePayRequest(ohid);
                case "3":
                case "LinePay":
                    return await linePayAppService.LinePayRequest(ohid);
                case "4":
                case "ECPay":
                    return await ecPayAppService.ECPayGetToken(ohid);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ECPayCreatePayment(ECPayPaymentInfoDto PaymentInfo)
        {
            return await ecPayAppService.ECPayCreatePayment(PaymentInfo);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ECPayGetToken(long ohid)
        {
            return await ecPayAppService.ECPayGetToken(ohid);
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
            Console.WriteLine($"-------------進入PChomePayNotify-------------");
            var remoteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var allowedIps = configuration.GetSection("ThirdParty:PCHomePay:SourceIP").Get<List<string>>();

            Console.WriteLine($"-------------PChomePayNotify來源查看-------------");
            Console.WriteLine($"remoteIp：{remoteIp}");
            Console.WriteLine($"allowedIps：{allowedIps}");

            if (string.IsNullOrEmpty(remoteIp) || !allowedIps.Contains(remoteIp))
            {
                Console.WriteLine($"不允許");
                return "Forbidden: IP not allowed";
            }
            else Console.WriteLine($"允許");

            return await pchomePayAppService.PChomePayNotify(dto);
        }
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ECPayOrderResult([FromForm] string ResultData)
        {
            return await ecPayAppService.ECPayOrderResult(ResultData);
        }
        [HttpPost]
        public async Task<String> ECPayReturn(ECPayReturnResponseDto ResultResponseData)
        {
            return await ecPayAppService.ECPayReturn(ResultResponseData);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ECPayOrderState(long ohid)
        {
            return await ecPayAppService.ECPayOrderState(ohid);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> HandleThirdPartyPayment(HandleThirdPartyPaymentDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var CheckSource = await thirdPartyAppService.CheckSource(dto.Token);
            if (CheckSource.Success)
            {
                switch (dto.ThirdParties)
                {
                    case "LINEPay":
                        switch (dto.Action)
                        {
                            case "Refund":
                                response = await linePayAppService.LinePayRefund(dto.OrderId, null);
                                break;
                            case "CheckRefund":
                                response = await linePayAppService.LinePayRefundState(dto.OrderId);
                                break;
                            case "CheckStatus":
                                response = await linePayAppService.LinePayCheckPaymentStatus(dto.OrderId);
                                break;
                            default:
                                response.Message = $"查詢動作【{dto.Action}】不支援";
                                break;
                        }
                        break;
                    case "支付連":
                        switch (dto.Action)
                        {
                            case "Refund":
                                response = await pchomePayAppService.PChomePayRefund(dto.OrderId, null);
                                break;
                            case "CheckRefund":
                                response = await pchomePayAppService.PChomePayRefundState(dto.OrderId);
                                break;
                            case "CheckStatus":
                                response = await pchomePayAppService.PChomePayCheckPaymentStatus(dto.OrderId);
                                break;
                            default:
                                response.Message = $"查詢動作【{dto.Action}】不支援";
                                break;
                        }
                        break;
                    case "綠界支付":
                        switch (dto.Action)
                        {
                            case "Refund":
                                response = await ecPayAppService.ECPayRefund(dto.OrderId);
                                break;
                            case "CheckRefund":
                                //response = await pchomePayAppService.PChomePayRefundState(dto.OrderId);
                                break;
                            case "CheckStatus":
                                response = await ecPayAppService.ECPayOrderState(dto.OrderId);
                                break;
                            default:
                                response.Message = $"查詢動作【{dto.Action}】不支援";
                                break;
                        }
                        break;
                    default:
                        response.Message = $"付款方式【{dto.ThirdParties}】不存在";
                        break;
                }
            }
            else response.Error = "Token 驗證錯誤";
            return response;
        }
    }
}
