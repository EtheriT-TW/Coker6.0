using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ThirdPartyController : Controller
    {
        private readonly IThirdPartyAppService thirdPartyAppService;
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        private readonly IECPayLogisticsAppService ecPayLogisticsAppService;
        public ThirdPartyController(
            IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IECPayLogisticsAppService ecPayLogisticsAppService)
        {
            this.thirdPartyAppService = thirdPartyAppService;
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
            this.ecPayLogisticsAppService = ecPayLogisticsAppService;
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
        //[HttpGet]
        //public async Task<ResponseMessageDto> PayRefund(string payment, long ohid, int? refund)
        //{
        //    ResponseMessageDto response = new ResponseMessageDto();
        //    switch (payment)
        //    {
        //        case "LINEPay":
        //            return await linePayAppService.LinePayRefund(ohid, refund);
        //        case "支付連":
        //            return await pchomePayAppService.PChomePayRefund(ohid, refund);
        //    }
        //    response.Success = false;
        //    response.Message = "支付方式不存在";
        //    return response;
        //}
        //[HttpGet]
        //public async Task<ResponseMessageDto> CheckPaymentStatus(long ohid, int thirdparty)
        //{
        //    ResponseMessageDto response = new ResponseMessageDto();
        //    switch (thirdparty)
        //    {
        //        case 2:
        //            return await pchomePayAppService.PChomePayCheckPaymentStatus(ohid);
        //        case 3:
        //            return await linePayAppService.LinePayCheckPaymentStatus(ohid);
        //    }
        //    response.Success = false;
        //    response.Message = "支付方式不存在";
        //    return response;
        //}
        [HttpGet]
        public async Task<IActionResult> PChomePayReturn(string ohid)
        {
            return await pchomePayAppService.PChomePayReturn(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PChomePayBalance()
        {
            return await pchomePayAppService.PChomePayBalance();
        }
        //[HttpGet]
        //public async Task<ResponseMessageDto> CheckRefund(string payment, string transactionId)
        //{
        //    ResponseMessageDto response = new ResponseMessageDto();
        //    switch (payment)
        //    {
        //        case "LINEPay":
        //            return await linePayAppService.LinePayRefundState(transactionId);
        //        case "支付連":
        //            return await pchomePayAppService.PChomePayRefundState(transactionId);
        //    }
        //    response.Success = false;
        //    response.Message = "支付方式不存在";
        //    return response;
        //}
        [HttpPost]
        public async Task<ResponseMessageDto> HandleThirdPartyPayment(HandleThirdPartyPaymentDto dto)
        {
            return await thirdPartyAppService.HandleThirdPartyPayment(dto);
        }
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> ECPayLogisticsExpressCreate(HandleThirdPartyLogisticsDto dto)
        {
            try
            {
                var response = await thirdPartyAppService.HandleThirdPartyLogistics(dto);
                if (response.Success && !string.IsNullOrEmpty(response.Message)) return Content(response.Message, "text/html");
                else throw new Exception(response.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<ResponseMessageDto> HandleThirdPartyLogistics(HandleThirdPartyLogisticsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                switch (dto.Action)
                {
                    case "CreateLogistics":
                        response = await thirdPartyAppService.HandleThirdPartyLogistics(dto);
                        break;
                    default:
                        throw new Exception($"查詢動作【{dto.Action}】不支援");
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<IActionResult> ECPayLogisticsPrintShippingLabel([FromForm] HandleThirdPartyLogisticsDto dto)
        {
            try
            {
                ResponseMessageDto response = new ResponseMessageDto();
                string[] message = [];
                string actionUrl = "";

                switch (dto.Action)
                {
                    case "PrintOrderInfo":
                        response = await thirdPartyAppService.HandleThirdPartyLogistics(dto);

                        if (string.IsNullOrEmpty(response.Message)) throw new Exception("RequestBody取得錯誤");
                        if (!response.Success) throw new Exception(response.Message);

                        message = response.Message.Split("&&");
                        if (message.Length != 2) throw new Exception("資料錯誤");

                        actionUrl = message[0];
                        switch (dto.ExtraData)
                        {
                            case "C2C711":
                                ECPayLogisticsPrintShippingLabelC2C711RequestDto PrintShippingLabelC2C711RequestBody = JsonConvert.DeserializeObject<ECPayLogisticsPrintShippingLabelC2C711RequestDto>(message[1]);
                                if (PrintShippingLabelC2C711RequestBody == null) throw new Exception("RequestBody為空");
                                return Content(GenerateAutoPostForm(actionUrl, PrintShippingLabelC2C711RequestBody), "text/html");
                            case "C2CFAMI":
                            case "C2CHILIFE":
                            case "C2COKMART":
                                ECPayLogisticsPrintShippingLabelC2CRequestDto PrintShippingLabelC2CRequestBody = JsonConvert.DeserializeObject<ECPayLogisticsPrintShippingLabelC2CRequestDto>(message[1]);
                                if (PrintShippingLabelC2CRequestBody == null) throw new Exception("RequestBody為空");
                                return Content(GenerateAutoPostForm(actionUrl, PrintShippingLabelC2CRequestBody), "text/html");
                            case "B2C":
                            case "HOME":
                                ECPayLogisticsPrintShippingLabelB2CRequestDto PrintShippingLabelB2CRequestBody = JsonConvert.DeserializeObject<ECPayLogisticsPrintShippingLabelB2CRequestDto>(message[1]);
                                if (PrintShippingLabelB2CRequestBody == null) throw new Exception("RequestBody為空");
                                return Content(GenerateAutoPostForm(actionUrl, PrintShippingLabelB2CRequestBody), "text/html");
                            default:
                                throw new Exception($"查詢內容【{dto.ExtraData}】不支援");
                        }
                    default:
                        throw new Exception($"查詢動作【{dto.Action}】不支援");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
