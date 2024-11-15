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
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

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
                                db.SaveChanges();
                            }
                            else
                            {
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
            return response;
        }
        public async Task<IActionResult> LinePayConfirm(string transactionId, string orderId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            LinePayConfirmResponseDto linePayResponse = new LinePayConfirmResponseDto();
            string RequestBodyStr = "";
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            try
            {
                long.TryParse(orderId, out long ohid);
                if (ohid > 0)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                    if (ohdata != null)
                    {
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
                            Console.WriteLine($"LinePay回傳資料：({linePayResponse.ReturnCode}：{linePayResponse.ReturnCode})");

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar?{orderId}");
        }
        public async Task<IActionResult> LinePayCancel(string transactionId, string orderId)
        {
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            try
            {
                LinePayResponseDto linePayResponse = await LinePayCheckPaymentStatus(transactionId, orderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar?{orderId}");
        }
        public async Task<LinePayResponseDto> LinePayCheckPaymentStatus(string transactionId, string orderId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            LinePayResponseDto linePayResponse = new LinePayResponseDto();
            try
            {
                long.TryParse(orderId, out long ohid);
                if (ohid > 0)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && e.TransactionId == transactionId).FirstOrDefaultAsync();

                    if (ohdata != null)
                    {
                        var RequestUri = $"/v3/payments/requests/{transactionId}/check";
                        response = await LinePayDefaultRequestHeaders(RequestUri, "");

                        if (response.Success)
                        {
                            var GetResponse = await ThirdPartyClient_Line.GetAsync(RequestUri);
                            GetResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                            linePayResponse = JsonConvert.DeserializeObject<LinePayResponseDto>(jsonResponse);
                            switch (linePayResponse.ReturnCode)
                            {
                                case "0110":
                                    if (ohdata.State == OrderStatusEnum.待確認)
                                    {
                                        ohdata.State = OrderStatusEnum.待付款;
                                        db.SaveChanges();
                                    }
                                    break;
                                case "0121":
                                    if (ohdata.State == OrderStatusEnum.待確認)
                                    {
                                        response = await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已取消);
                                    }
                                    break;
                                case "0122":
                                    if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.付款失敗;
                                        db.SaveChanges();
                                    }
                                    break;
                                case "0123":
                                    if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.已付款;
                                        db.SaveChanges();
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                linePayResponse.ReturnCode = "OtherErrors";
                linePayResponse.ReturnMessage = ex.Message;
            }
            return linePayResponse;
        }
        private async Task<ResponseMessageDto> LinePayDefaultRequestHeaders(string RequestUri, string RequestBody)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
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
                    RequestBody.orderId = oid;

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
                            imageUrl = $"{Website.DefaultUrl}{od.ImagePath}",
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
