using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Order;
using static EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto.LinePayRequestBodyDto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class LinePayAppService : ILinePayAppService
    {
        private readonly HttpClient ThirdPartyClient_Line;
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IOrderAppService orderAppService;
        public LinePayAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IOrderAppService orderAppService
        )
        {
            ThirdPartyClient_Line = httpClientFactory.CreateClient("ThirdPartyClient_Line");
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.orderAppService = orderAppService;
        }
        public async Task<ResponseMessageDto> LinePayRequest(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
            try
            {
                if (ohdata != null)
                {
                    LinePayRequestBodyDto RequestBody = await LinePayGetRequestBody(ohdata);

                    if (RequestBody != null)
                    {
                        var RequestUri = "/v3/payments/request";
                        var RequestBodyStr = JsonConvert.SerializeObject(RequestBody);
                        response = await LinePayDefaultRequestHeaders(RequestUri, RequestBodyStr);

                        if (response.Success)
                        {
                            var RequestContent = new StringContent(RequestBodyStr, Encoding.UTF8, "application/json");
                            var PostResponse = await ThirdPartyClient_Line.PostAsync(RequestUri, RequestContent);
                            PostResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                            var linePayResponse = JsonConvert.DeserializeObject<LinePayRequestResponseDto>(jsonResponse);

                            if (linePayResponse.ReturnCode == "0000")
                            {
                                response.Success = true;
                                response.Message = linePayResponse.Info.PaymentUrl.Web;
                                ohdata.TransactionId = linePayResponse.Info.TransactionId;
                                if (ohdata.RepayTimes != null) ohdata.RepayDate = DateTime.Now;
                                ohdata.State = OrderStatusEnum.待付款;
                                db.SaveChanges();
                            }
                            else
                            {
                                ohdata.State = OrderStatusEnum.付款失敗;
                                db.SaveChanges();
                                response.Error = linePayResponse.ReturnCode;
                                response.Message = linePayResponse.ReturnMessage;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Message = $"Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                response.Message = $"Other Error: {ex.Message}";
            }
            if (!response.Success && ohdata != null)
            {
                var temp_response = await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.付款失敗);
                if (!temp_response.Success) response.Message += $"&{temp_response.Message}";
            }
            return response;
        }
        public async Task<IActionResult> LinePayConfirm(string transactionId, string orderId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            LinePayConfirmResponseDto linePayResponse = new LinePayConfirmResponseDto();
            string RequestBodyStr = "";
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var ohdata = await db.Order_Headers.Where(e => e.TransactionId == transactionId).FirstOrDefaultAsync();
            var ohidstr = "";
            try
            {
                if (ohdata != null)
                {
                    ohidstr = $"000000000{ohdata.Id}".Substring(ohdata.Id.ToString().Length);
                    var RequestUri = $"/v3/payments/{transactionId}/confirm";
                    LinePayConfirmRequestDto RequestBody = new LinePayConfirmRequestDto()
                    {
                        amount = (ohdata.Subtotal + ohdata.Freight).ToString(),
                        currency = "TWD",
                    };
                    RequestBodyStr = JsonConvert.SerializeObject(RequestBody);
                    response = await LinePayDefaultRequestHeaders(RequestUri, RequestBodyStr);

                    if (response.Success)
                    {
                        var RequestContent = new StringContent(RequestBodyStr, Encoding.UTF8, "application/json");
                        var PostResponse = await ThirdPartyClient_Line.PostAsync(RequestUri, RequestContent);
                        PostResponse.EnsureSuccessStatusCode();
                        var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                        linePayResponse = JsonConvert.DeserializeObject<LinePayConfirmResponseDto>(jsonResponse);
                        Console.WriteLine($"-------------錯誤訊息查看-------------");
                        Console.WriteLine($"LinePay=>LinePayConfirm回傳資料：({linePayResponse.ReturnCode}：{linePayResponse.ReturnCode})");

                        if (linePayResponse.ReturnCode == "0000")
                        {
                            ohdata.State = OrderStatusEnum.已付款;
                            db.SaveChanges();
                        }
                        else
                        {
                            ohdata.State = OrderStatusEnum.付款失敗;
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"LinePay=>LinePayConfirm回傳資料：Request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"LinePay=>LinePayConfirm回傳資料：Other Error: {ex.Message}");
            }
            return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar?{ohidstr}");
        }
        public async Task<ResponseMessageDto> LinePayConfirm(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            LinePayConfirmResponseDto linePayResponse = new LinePayConfirmResponseDto();
            string RequestBodyStr = "";
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata != null)
                {
                    var RequestUri = $"/v3/payments/{ohdata.TransactionId}/confirm";
                    LinePayConfirmRequestDto RequestBody = new LinePayConfirmRequestDto()
                    {
                        amount = (ohdata.Subtotal + ohdata.Freight).ToString(),
                        currency = "TWD",
                    };
                    RequestBodyStr = JsonConvert.SerializeObject(RequestBody);
                    response = await LinePayDefaultRequestHeaders(RequestUri, RequestBodyStr);

                    if (response.Success)
                    {
                        response = new ResponseMessageDto();
                        var RequestContent = new StringContent(RequestBodyStr, Encoding.UTF8, "application/json");
                        var PostResponse = await ThirdPartyClient_Line.PostAsync(RequestUri, RequestContent);
                        PostResponse.EnsureSuccessStatusCode();
                        var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                        linePayResponse = JsonConvert.DeserializeObject<LinePayConfirmResponseDto>(jsonResponse);

                        if (linePayResponse.ReturnCode == "0000")
                        {
                            ohdata.State = OrderStatusEnum.已付款;
                            db.SaveChanges();
                            response.Success = true;
                        }
                        else
                        {
                            ohdata.State = OrderStatusEnum.付款失敗;
                            db.SaveChanges();
                            response.Error = linePayResponse.ReturnCode;
                            response.Message = linePayResponse.ReturnMessage;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                response.Error = "Http Request Error";
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Error = "Other Error";
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<IActionResult> LinePayCancel(string transactionId, string orderId)
        {
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var ohdata = await db.Order_Headers.Where(e => e.TransactionId == transactionId).FirstOrDefaultAsync();
            var ohidstr = "";
            try
            {
                if (ohdata != null)
                {
                    ohidstr = $"000000000{ohdata.Id}".Substring(ohdata.Id.ToString().Length);
                    ResponseMessageDto linePayResponse = await LinePayCheckPaymentStatus(int.Parse(orderId));
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"LinePay=>LinePayCancel回傳資料：Request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"LinePay=>LinePayCancel回傳資料：Other Error: {ex.Message}");
            }
            return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar?{ohidstr}");
        }
        public async Task<ResponseMessageDto> LinePayVoid(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata != null && ohdata.State == OrderStatusEnum.待付款)
                {
                    LinePayRequestBodyDto RequestBody = await LinePayGetRequestBody(ohdata);

                    if (RequestBody != null)
                    {
                        var RequestUri = $"/v3/payments/authorizations/{ohdata.TransactionId}/void";
                        var RequestBodyStr = JsonConvert.SerializeObject(RequestBody);
                        response = await LinePayDefaultRequestHeaders(RequestUri, RequestBodyStr);

                        if (response.Success)
                        {
                            response = new ResponseMessageDto();
                            var RequestContent = new StringContent(RequestBodyStr, Encoding.UTF8, "application/json");
                            var PostResponse = await ThirdPartyClient_Line.PostAsync(RequestUri, RequestContent);
                            PostResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                            var linePayResponse = JsonConvert.DeserializeObject<LinePayResponseDto>(jsonResponse);

                            if (linePayResponse.ReturnCode == "0000")
                            {
                                response.Success = true;
                                ohdata.State = OrderStatusEnum.已取消;
                                db.SaveChanges();
                            }
                            response.Error = linePayResponse.ReturnCode;
                            response.Message = linePayResponse.ReturnMessage;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Message = $"Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                response.Message = $"Other Error: {ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> LinePayCheckPaymentStatus(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            LinePayResponseDto linePayResponse = new LinePayResponseDto();
            try
            {
                if (ohid > 0)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                    if (ohdata != null)
                    {
                        var RequestUri = $"/v3/payments/requests/{ohdata.TransactionId}/check";
                        response = await LinePayDefaultRequestHeaders(RequestUri, "");

                        if (response.Success)
                        {
                            response = new ResponseMessageDto();
                            var GetResponse = await ThirdPartyClient_Line.GetAsync(RequestUri);
                            GetResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                            linePayResponse = JsonConvert.DeserializeObject<LinePayResponseDto>(jsonResponse);
                            response.Success = true;
                            response.Error = linePayResponse.ReturnCode;
                            switch (linePayResponse.ReturnCode)
                            {
                                case "0000":
                                case "0110":
                                    ohdata.State = OrderStatusEnum.待付款;
                                    break;
                                case "0121":
                                    response = await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已取消);
                                    break;
                                case "0123":
                                    ohdata.State = OrderStatusEnum.已付款;
                                    break;
                                default:
                                    ohdata.State = OrderStatusEnum.付款失敗;
                                    break;
                            }
                            db.SaveChanges();
                            response.Message = $"{(int)ohdata.State},{linePayResponse.ReturnMessage}";
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Error = "Request failed";
                response.Error = "ex.Message";
            }
            catch (Exception ex)
            {
                response.Error = "Other Error";
                response.Error = "ex.Message";
            }
            return response;
        }
        public async Task<ResponseMessageDto> LinePayRefund(long ohid, int? refund)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata != null)
                {
                    var RequestBody = new { refundAmount = refund };

                    if (RequestBody != null)
                    {
                        var RequestUri = $"/v3/payments/{ohdata.TransactionId}/refund";
                        var RequestBodyStr = JsonConvert.SerializeObject(RequestBody);
                        response = await LinePayDefaultRequestHeaders(RequestUri, RequestBodyStr);

                        if (response.Success)
                        {
                            response = new ResponseMessageDto();
                            var RequestContent = new StringContent(RequestBodyStr, Encoding.UTF8, "application/json");
                            var PostResponse = await ThirdPartyClient_Line.PostAsync(RequestUri, RequestContent);
                            PostResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                            var linePayResponse = JsonConvert.DeserializeObject<LinePayRefundResponseDto>(jsonResponse);

                            if (linePayResponse.ReturnCode == "0000")
                            {
                                response.Success = true;
                                response.Message = $"Message: {linePayResponse.ReturnMessage}; RefundId: {linePayResponse.info.refundTransactionId}; Date: {linePayResponse.info.refundTransactionDate}";
                                ohdata.refundTransactionId = linePayResponse.info.refundTransactionId;
                                ohdata.refundTransactionDate = linePayResponse.info.refundTransactionDate != null ? DateTime.Parse(linePayResponse.info.refundTransactionDate).ToLocalTime() : null;
                                ohdata.State = OrderStatusEnum.已取消;
                                db.SaveChanges();
                            }
                            else
                            {
                                response.Message = linePayResponse.ReturnMessage;
                            }
                            response.Error = linePayResponse.ReturnCode;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Message = $"Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                response.Message = $"Other Error: {ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> LinePayRefundState(string transactionId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var RequestUri = $"/v3/payments";
                string queryString = $"transactionId={transactionId}";
                response = await LinePayDefaultRequestHeaders(RequestUri, queryString);
                RequestUri += $"?{queryString}";

                if (response.Success)
                {
                    response = new ResponseMessageDto();
                    var GetResponse = await ThirdPartyClient_Line.GetAsync(RequestUri);
                    GetResponse.EnsureSuccessStatusCode();
                    var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                    response.Message = jsonResponse.ToString();
                    var linePayResponse = JsonConvert.DeserializeObject<LinePayRefundCheckResponseDto>(jsonResponse);

                    if (linePayResponse.ReturnCode == "0000")
                    {
                        if (linePayResponse.info[0].refundList != null)
                        {
                            response.Success = true;
                            response.Message = $"退款編號{linePayResponse.info[0].refundList[0].refundTransactionId}已於{linePayResponse.info[0].refundList[0].refundTransactionDate.ToString("yyyy/MM/dd")}退款，退款金額為{(linePayResponse.info[0].refundList[0].refundAmount * -1).ToString("$#,##0")}";
                        }
                        else
                        {
                            response.Message = "該筆訂單不存在退款資訊";
                        }
                    }
                    else
                    {
                        response.Message = linePayResponse.ReturnMessage;
                    }
                    response.Error = linePayResponse.ReturnCode;
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Message = $"Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                response.Message = $"Other Error: {ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> LinePayPayCancelOrder(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                ResponseMessageDto temp_response = await LinePayCheckPaymentStatus(ohid);
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                if (ohdata != null)
                {
                    if (ohdata.TransactionId != null)
                    {
                        switch (ohdata.State)
                        {
                            case OrderStatusEnum.待付款:
                                response = await LinePayVoid(ohdata.Id);
                                if (response.Success) response.Message = "訂單已取消並送出退款申請。";
                                break;
                            case OrderStatusEnum.已付款:
                                response = await LinePayRefund(ohdata.Id, null);
                                if (response.Success) response.Message = "訂單已取消並送出退款申請。";
                                break;
                        }
                    }
                    else
                    {
                        response = await orderAppService.OrderStateChange(ohid, (int)OrderStatusEnum.已取消);
                        if (response.Success) response.Message = "訂單已取消。";
                    }
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                response.Error = "Other Error";
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ResponseMessageDto> LinePayDefaultRequestHeaders(string RequestUri, string RequestBody)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
                var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                     join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                     join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                     where tp.Title == "LINE Pay"
                                                     where tpkv.FK_WebsiteId == WebsiteId
                                                     select new KeyValueDto() { Key = tpk.Title, Value = tpkv.Value }).ToListAsync();
                var ChannelId = "";
                var ChannelSecretKey = "";

                if (thirdPartyKeypairValues.Any())
                {
                    ChannelId = thirdPartyKeypairValues.Find(e => e.Key == "Channel ID").Value;
                    ChannelSecretKey = thirdPartyKeypairValues.Find(e => e.Key == "Channel Secret Key").Value;
                }
                if (ChannelId != "" && ChannelSecretKey != "")
                {
                    String Nonce = Guid.NewGuid().ToString("N");

                    ThirdPartyClient_Line.DefaultRequestHeaders.Clear();
                    ThirdPartyClient_Line.DefaultRequestHeaders.Add("X-LINE-ChannelId", ChannelId);
                    ThirdPartyClient_Line.DefaultRequestHeaders.Add("X-LINE-Authorization-Nonce", Nonce);

                    String signature = encrypt(ChannelSecretKey, ChannelSecretKey + RequestUri + RequestBody + Nonce);
                    ThirdPartyClient_Line.DefaultRequestHeaders.Add("X-LINE-Authorization", signature);

                    response.Success = true;
                }
                else throw new Exception("查無LinePay所需參數");
            }
            catch (Exception e)
            {

            }
            return response;
        }
        private async Task<LinePayRequestBodyDto> LinePayGetRequestBody(Order_Header ohdata)
        {
            LinePayRequestBodyDto RequestBody = new LinePayRequestBodyDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

            try
            {
                var oddatas = await orderAppService.GetOrderDetails(ohdata.Id);

                if (oddatas.Any())
                {
                    RequestBody.currency = "TWD";
                    
                    RequestBody.amount = (ohdata.Subtotal + ohdata.Freight).ToString();
                    var oid = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                    if (ohdata.TransactionId == null) RequestBody.orderId = $"{DateTime.Now.ToString("yyyyMMdd")}{oid}";
                    else
                    {
                        if (ohdata.RepayTimes == null) ohdata.RepayTimes = 1;
                        else ohdata.RepayTimes += 1;
                        db.SaveChanges();
                        RequestBody.orderId = $"{DateTime.Now.ToString("yyyyMMdd")}{oid}-{ohdata.RepayTimes}";
                    }

                    var Packages = new List<LinePayPackageDto>();
                    Packages.Add(new LinePayPackageDto()
                    {
                        id = oid,
                        amount = (ohdata.Subtotal + ohdata.Freight).ToString(),
                        userFee = 0.ToString(),
                        name = $"訂單編號：{oid}",
                    });

                    var Products = new List<LinePayProductsDto>();
                    foreach (var od in oddatas)
                    {
                        Products.Add(new LinePayProductsDto()
                        {
                            id = od.PId.ToString(),
                            name = od.Title,
                            imageUrl = $"{Website.DefaultUrl}{od.ImagePath}".Replace($"/{Website.OrgName}/", "/"),
                            quantity = od.Quantity.ToString(),
                            price = od.Price.ToString(),
                        });
                    }

                    Packages[0].products = Products;
                    Packages[0].products.Add(new LinePayProductsDto()
                    {
                        id = "",
                        name = "運費",
                        imageUrl = "",
                        quantity = 1.ToString(),
                        price = ohdata.Freight.ToString(),
                    });
                    RequestBody.packages = Packages;

                    RequestBody.redirectUrls = new LinePayRedirectUrlsDto();
                    RequestBody.redirectUrls.confirmUrl = $"{Website.DefaultUrl}/api/ThirdParty/LinePayConfirm";
                    RequestBody.redirectUrls.cancelUrl = $"{Website.DefaultUrl}/api/ThirdParty/LinePayCancel";

                    RequestBody.options = new LinePayOptionDto();

                    RequestBody.options.payment = new LinePayOptionDto.LinePayPaymentDto();
                    RequestBody.options.payment.capture = true;

                    RequestBody.options.display = new LinePayOptionDto.LinePayDisplayDto();
                    RequestBody.options.display.locale = "zh_TW";
                    RequestBody.options.display.checkConfirmUrlBrowser = false;
                }
            }
            catch (Exception ex)
            {

            }
            return RequestBody;
        }
        private string encrypt(string keys, string data)
        {
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keys));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }
    }
}
