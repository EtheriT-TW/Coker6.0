using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;

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
        private readonly IECPayLogisticsAppService ecPayLogisticsAppService;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ThirdPartyController(
            IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IECPayAppService ecPayAppService,
            IECPayLogisticsAppService ecPayLogisticsAppService,
            IConfiguration configuration,
            IHttpContextAccessor _httpContextAccessor)
        {
            this.thirdPartyAppService = thirdPartyAppService;
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
            this.ecPayAppService = ecPayAppService;
            this.ecPayLogisticsAppService = ecPayLogisticsAppService;
            this.configuration = configuration;
            this._httpContextAccessor = _httpContextAccessor;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PayRequest(long ohid, string paytype, bool? support)
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
                    return await ecPayAppService.ECPayGetTokenById(ohid, support ?? false);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ECPayGetPaymentInfo(long ohid)
        {
            return await ecPayAppService.ECPayGetPaymentInfo(ohid);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ECPayCreatePayment(ECPayPaymentInfoDto PaymentInfo)
        {
            return await ecPayAppService.ECPayCreatePayment(PaymentInfo);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ECPayGetToken(OrderHeaderAddDto dto)
        {
            return await ecPayAppService.ECPayGetToken(dto);
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
        public async Task<String> ECPayReturn([FromBody] ECPayReturnResponseDto ResultResponseData)
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
                            case "DeliveryNote":
                                response = await pchomePayAppService.PChomePayDeliveryNote(dto.OrderId);
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
        [HttpGet]
        public async Task<IActionResult> ECPayLogisticsGetMap(string SCIds, string LogisticsSubType, string IsCollection)
        {
            try
            {
                var baseUrl = configuration["ThirdParty:ECPayLogistics:LogisticsUrl"];
                var actionUrl = $"{baseUrl}/Express/map";

                ECPayLogisticsMapRequestDto RequestBody = await ecPayLogisticsAppService.ECPayLogisticsGetMapRequestBody(SCIds, LogisticsSubType, IsCollection);

                return Content(GenerateAutoPostForm(actionUrl, RequestBody), "text/html");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ECPayLogisticsGetMapResponse([FromForm] ECPayLogisticsMapResponseDto ResultResponseData)
        {
            var response = await ecPayLogisticsAppService.ECPayLogisticsGetMapResponse(ResultResponseData);
            var redirectUrl = response?.Message;

            if (string.IsNullOrWhiteSpace(redirectUrl)) return Content("1|OK");

            return LocalRedirect(redirectUrl);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> HandleThirdPartyLogistics(HandleThirdPartyLogisticsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var CheckSource = await thirdPartyAppService.CheckSource(dto.Token);
            if (CheckSource.Success)
            {
                var baseUrl = configuration["ThirdParty:ECPayLogistics:LogisticsUrl"];
                string actionUrl = "";

                switch (dto.Action)
                {
                    case "CreateLogistics":
                        actionUrl = $"{baseUrl}/Express/Create";
                        switch (dto.ExtraData)
                        {
                            case "CVS":
                                response = await ecPayLogisticsAppService.ECPayLogisticsExpressCVSCreate(dto.OrderId);
                                break;
                            case "HOME":

                                break;
                            default:
                                response.Message = $"物流方式【{dto.ExtraData}】不支援";
                                break;
                        }
                        break;
                    case "PrintOrderInfo":
                        ResponseMessageDto RequestBodyResponse = new ResponseMessageDto();
                        bool GetResponse = true;
                        switch (dto.ExtraData)
                        {
                            case "C2C711":
                                actionUrl = $"{baseUrl}/Express/PrintUniMartC2COrderInfo";
                                RequestBodyResponse = await ecPayLogisticsAppService.ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum.UniMart, dto.OrderId);
                                break;
                            case "C2CFAMI":
                                actionUrl = $"{baseUrl}/Express/PrintFAMIC2COrderInfo";
                                RequestBodyResponse = await ecPayLogisticsAppService.ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum.FAMI, dto.OrderId);
                                break;
                            case "C2CHILIFE":
                                actionUrl = $"{baseUrl}/Express/PrintHILIFEC2COrderInfo";
                                RequestBodyResponse = await ecPayLogisticsAppService.ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum.HILIFE, dto.OrderId);
                                break;
                            case "C2COKMART":
                                actionUrl = $"{baseUrl}/Express/PrintOKMARTC2COrderInfo";
                                RequestBodyResponse = await ecPayLogisticsAppService.ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum.OKMART, dto.OrderId);
                                break;
                            case "B2C":
                            case "HOME":
                                actionUrl = $"{baseUrl}/helper/printTradeDocument";
                                RequestBodyResponse = await ecPayLogisticsAppService.ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum.B2C, dto.OrderId);
                                break;
                            default:
                                GetResponse = false;
                                response.Message = $"物流方式【{dto.ExtraData}】不支援";
                                break;
                        }
                        if (GetResponse)
                        {
                            if (string.IsNullOrEmpty(RequestBodyResponse.Message)) throw new Exception("取得RequestBodyResponse發生錯誤");
                            if (!RequestBodyResponse.Success) throw new Exception(RequestBodyResponse.Message);
                            response.Message = $"{actionUrl}&&{RequestBodyResponse.Message}";
                            response.Success = true;
                        }
                        break;
                    default:
                        response.Message = $"查詢動作【{dto.Action}】不支援";
                        break;
                }
            }
            else response.Error = "Token 驗證錯誤";
            return response;
        }
        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ECPayLogisticsExpressCreateResponse()
        {
            var data = Request.Form.ToDictionary(
                x => x.Key,
                x => x.Value.ToString()
            );

            await ecPayLogisticsAppService.ECPayLogisticsExpressCreateResponse(data);

            return Content("1|OK");
        }
        private string GenerateAutoPostForm(string actionUrl, object RequestBody)
        {
            var props = RequestBody.GetType().GetProperties();

            var inputs = new StringBuilder();

            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(RequestBody)?.ToString() ?? "";

                inputs.AppendLine($@"<input type='hidden' name='{name}' value='{WebUtility.HtmlEncode(value)}' />");
            }

            var html = $@"<!DOCTYPE html>
                                            <html>
                                            <head>
                                                <meta charset='utf-8' />
                                                <title>Redirecting...</title>
                                            </head>
                                            <body>
                                                <form id='form' method='post' action='{actionUrl}'>
                                                    {inputs}
                                                </form>
                                                <script>
                                                    document.getElementById('form').submit();
                                                </script>
                                            </body>
                                            </html>";
            return html;
        }
    }
}
