using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Dto;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;
using EtheriT.Coker.Application.Token;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class PChomePayAppService : IPChomePayAppService
    {
        private readonly HttpClient ThirdPartyClient_PCHome;
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IOrderAppService orderAppService;
        private readonly ITokenAppService tokenAppService;
        public PChomePayAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IOrderAppService orderAppService,
            ITokenAppService tokenAppService
        )
        {
            ThirdPartyClient_PCHome = httpClientFactory.CreateClient("ThirdPartyClient_PCHome");
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.orderAppService = orderAppService;
            this.tokenAppService = tokenAppService;
        }
        public async Task<ResponseMessageDto> PChomePayGetToken()
        {
            ResponseMessageDto response = new ResponseMessageDto();

            return response;
        }
        public async Task<ResponseMessageDto> PChomePayRequest(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata != null)
                {
                    PChomePayPaymentDto PaymentBody = await PChomeGetPaymentBody(ohdata);

                    if (PaymentBody != null)
                    {
                        var RequestUri = "/v1/payment";
                        var PaymentBodyStr = JsonConvert.SerializeObject(PaymentBody);
                        response = await PChomePayHeaders();

                        if (response.Success)
                        {
                            response = new ResponseMessageDto();
                            var RequestContent = new StringContent(PaymentBodyStr, Encoding.UTF8, "application/json");
                            var PostResponse = await ThirdPartyClient_PCHome.PostAsync(RequestUri, RequestContent);
                            PostResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                            var pchomePayResponse = JsonConvert.DeserializeObject<PChomePayPaymentResponseDto>(jsonResponse);

                            if (pchomePayResponse != null)
                            {
                                if (pchomePayResponse.code == null)
                                {
                                    response.Success = true;
                                    response.Message = pchomePayResponse.payment_url;
                                    ohdata.TransactionId = pchomePayResponse.order_id;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    response.Error = $"{pchomePayResponse.error_type}: {pchomePayResponse.code}";
                                    response.Message = pchomePayResponse.message;
                                }
                            }
                            else throw new Exception("PChomePayRequest錯誤");
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
        public async Task<IActionResult> PChomePayReturn(string ohid)
        {
            var orderid = long.Parse(ohid);
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            try
            {
                if (orderid > 0)
                {
                    PChomePayStateDto statte = await PChomePayCheckPaymentStatus(orderid);
                    if (statte == null) throw new Exception("查無訂單狀態");
                }
                else throw new Exception("查無訂單資料");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"PChomePay=>PChomePayReturn回傳資料：{ex.Message}");
            }
            return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar?{ohid}");
        }
        public async Task<string> PChomePayNotify(PChomePayNotifyDto dto)
        {
            if (dto.notify_type == "refund_success")
            {
                Console.WriteLine($"-------------訊息查看-------------");
                Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：{dto.notify_message}");
            }
            else
            {
                JObject jsonMessage = JObject.Parse(dto.notify_message);

                if (jsonMessage.ContainsKey("order_id"))
                {
                    long orderId = jsonMessage["order_id"] != null && !string.IsNullOrEmpty(jsonMessage["order_id"].ToString()) ? long.Parse(jsonMessage["order_id"].ToString()) : 0;
                    var ohdata = await db.Order_Headers.Where(e => e.Id == orderId).FirstOrDefaultAsync();
                    if (ohdata != null)
                    {
                        switch (dto.notify_type)
                        {
                            case "order_audit":
                                Console.WriteLine($"-------------訊息查看-------------");
                                Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：{dto.notify_message}");
                                break;
                            case "order_confirm":
                                if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                {
                                    ohdata.State = OrderStatusEnum.已付款;
                                    db.SaveChanges();
                                }
                                break;
                            case "order_expired":
                                if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                {
                                    ohdata.State = OrderStatusEnum.已取消;
                                    db.SaveChanges();
                                }
                                break;
                            case "order_failed":
                                if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                {
                                    ohdata.State = OrderStatusEnum.付款失敗;
                                    db.SaveChanges();
                                }
                                break;
                        }
                    }
                    {
                        Console.WriteLine($"-------------訊息查看-------------");
                        Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：order不存在");
                    }
                }
                else
                {
                    Console.WriteLine($"-------------訊息查看-------------");
                    Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：order_id不存在");
                }
            }
            return "success";
        }
        public async Task<PChomePayStateDto> PChomePayCheckPaymentStatus(long ohid)
        {
            PChomePayStateDto PChomePayState = new PChomePayStateDto();
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                if (ohid > 0)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                    if (ohdata != null)
                    {
                        var orderidstr = ("000000000" + ohid).Substring(ohid.ToString().Length);
                        var RequestUri = $"/v1/payment/{orderidstr}";
                        response = await PChomePayHeaders();

                        if (response.Success)
                        {
                            var GetResponse = await ThirdPartyClient_PCHome.GetAsync(RequestUri);
                            GetResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                            PChomePayState = JsonConvert.DeserializeObject<PChomePayStateDto>(jsonResponse);
                            switch (PChomePayState.status)
                            {
                                case "W":
                                    if (ohdata.State == OrderStatusEnum.待確認)
                                    {
                                        ohdata.State = OrderStatusEnum.待付款;
                                        db.SaveChanges();
                                    }
                                    break;
                                case "S":
                                    if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.已付款;
                                        db.SaveChanges();
                                    }
                                    break;
                                default:
                                    if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.付款失敗;
                                        db.SaveChanges();
                                    }
                                    break;
                            }
                            PChomePayState.order_state = (int)ohdata.State;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                PChomePayState.status_code = "RequestErrors";
                PChomePayState.status = $"Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                PChomePayState.status_code = "OtherErrors";
                PChomePayState.status = $"Request failed: {ex.Message}";
            }
            return PChomePayState;
        }
        public async Task<ResponseMessageDto> PChomePayRefund(long ohid, int? refund)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata != null)
                {
                    var ohidstr = $"000000000{ohid}".Substring(ohid.ToString().Length);
                    var RefundBody = new { order_id = ohidstr, refund_id = $"{ohidstr}-Refund", trade_amount = refund == null ? ohdata.Subtotal + ohdata.Freight : refund };

                    if (RefundBody != null)
                    {
                        var RefundUri = $"/v1/refund";
                        var RefundBodyStr = JsonConvert.SerializeObject(RefundBody);
                        response = await PChomePayHeaders();

                        if (response.Success)
                        {
                            response = new ResponseMessageDto();
                            var RefundContent = new StringContent(RefundBodyStr, Encoding.UTF8, "application/json");
                            var PostResponse = await ThirdPartyClient_PCHome.PostAsync(RefundUri, RefundContent);
                            PostResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                            var pchomePayResponse = JsonConvert.DeserializeObject<PChomeRefundDto>(jsonResponse);

                            if (pchomePayResponse != null && pchomePayResponse.order_id != null)
                            {
                                response.Success = true;
                                response.Message = pchomePayResponse.ToString();
                                ohdata.refundTransactionId = pchomePayResponse.refund_id; ;
                                ohdata.refundTransactionDate = DateTime.Now;
                                db.SaveChanges();
                            }
                            else
                            {
                                response.Message = "退款發生未知錯誤";
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
        public async Task<ResponseMessageDto> PChomePayHeaders()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var token = await tokenAppService.CheckToken();
                if (token != null)
                {
                    var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
                    var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                         join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                         join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                         where tp.Title == "支付連"
                                                         where tpkv.FK_WebsiteId == WebsiteId
                                                         select new KeyValueDto() { Key = tpk.Title, Value = tpkv.Value }).ToListAsync();
                    var PchomePayAppId = "";
                    var PchomePaySecre = "";

                    if (thirdPartyKeypairValues.Any())
                    {
                        PchomePayAppId = thirdPartyKeypairValues.Find(e => e.Key == "PchomePayAppId").Value;
                        PchomePaySecre = thirdPartyKeypairValues.Find(e => e.Key == "PchomePaySecre").Value;
                    }
                    if (PchomePayAppId != "")
                    {
                        string credentials = $"{PchomePayAppId}:{PchomePaySecre}";
                        string encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

                        var RequestUri = $"/v1/token";

                        ThirdPartyClient_PCHome.DefaultRequestHeaders.Clear();

                        ThirdPartyClient_PCHome.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedCredentials}");
                        ThirdPartyClient_PCHome.DefaultRequestHeaders.Add("Cookie", $"RefreshToken={token.RefreshToken.ToString()}");

                        var PostResponse = await ThirdPartyClient_PCHome.PostAsync(RequestUri, null);
                        PostResponse.EnsureSuccessStatusCode();
                        var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                        var tokenPayResponse = JsonConvert.DeserializeObject<PChomePayTokenDto>(jsonResponse);

                        if (tokenPayResponse != null)
                        {
                            ThirdPartyClient_PCHome.DefaultRequestHeaders.Add("pcpay-token", tokenPayResponse.token);

                            response.Message = $"{tokenPayResponse.token}; {tokenPayResponse.expired_in}; {tokenPayResponse.expired_timestamp}";
                            response.Success = true;

                        }
                        else throw new Exception("取得PChomeToken發生錯誤");
                    }
                    else throw new Exception("查無PCHomePay所需參數");
                }
                else throw new Exception("查無Token資訊");
            }
            catch (HttpRequestException ex)
            {
                response.Message = $"Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Message = $"Other failed: {ex.Message}";
            }
            return response;
        }
        private async Task<PChomePayPaymentDto> PChomeGetPaymentBody(Order_Header ohdata)
        {
            PChomePayPaymentDto PaymentBody = new PChomePayPaymentDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

            try
            {
                var oddatas = await orderAppService.GetOrderDetails(ohdata.Id);

                if (oddatas.Any())
                {
                    var oid = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                    PaymentBody.order_id = oid;

                    var paytype = await db.PaymentTypes.Where(e => e.Id == ohdata.Payment).FirstOrDefaultAsync();

                    PaymentBody.pay_type = new List<string>();
                    if (paytype != null)
                    {
                        if (paytype.Code.StartsWith("PchomePayInstallment"))
                        {
                            PaymentBody.card_installment = $"{paytype.Code.Substring("PchomePayInstallment".Length)}";
                            PaymentBody.pay_type.Add("CARD");
                        }
                        else if (paytype.Code.EndsWith("CARD"))
                        {
                            PaymentBody.card_installment = "1";
                            PaymentBody.pay_type.Add(paytype.Code.Substring("PchomePay".Length).ToString());
                        }
                        else if (paytype.Code.StartsWith("PchomePay"))
                        {
                            PaymentBody.pay_type.Add(paytype.Code.Substring("PchomePay".Length).ToString());
                        }
                        else
                        {
                            PaymentBody.pay_type.Add(paytype.Code.Substring("Pchome".Length).ToString());
                        }
                    }
                    else throw new Exception("付款方式錯誤");

                    PaymentBody.amount = ohdata.Subtotal + ohdata.Freight;

                    var items = new List<PChomePayItemsDto>();
                    foreach (var oddata in oddatas)
                    {
                        items.Add(new PChomePayItemsDto()
                        {
                            name = oddata.Title,
                            url = $"{Website.DefaultUrl}/{Website.OrgName}/home/product/{oddata.PId}"
                        });
                    }
                    PaymentBody.items = items;

                    PaymentBody.return_url = $"{Website.DefaultUrl}/api/ThirdParty/PChomePayReturn?ohid={PaymentBody.order_id}";
                    PaymentBody.fail_return_url = $"{Website.DefaultUrl}/api/ThirdParty/PChomePayReturn?ohid={PaymentBody.order_id}";
                    PaymentBody.notify_url = $"{Website.DefaultUrl}/api/ThirdParty/PChomePayNotify";

                    PaymentBody.buyer_email = ohdata.OrdererEmail;
                    PaymentBody.atm_info = new PChomePayPaymentDto.PChomePayPaymentInfo() { expire_days = 5 };

                    PaymentBody.return_timer = "Y";
                    PaymentBody.member_key = ohdata.Fk_UserId?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"PChomePay=>PChomePayPaymentBody回傳資料：{ex.Message}");
            }
            return PaymentBody;
        }
        public async Task<ResponseMessageDto> PChomePayBalance()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var RequestUri = $"/v1/balance";
            try
            {
                response = await PChomePayHeaders();

                if (response.Success)
                {
                    response = new ResponseMessageDto();
                    var GetResponse = await ThirdPartyClient_PCHome.GetAsync(RequestUri);
                    GetResponse.EnsureSuccessStatusCode();
                    var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                    response.Success = true;
                    response.Message = jsonResponse.ToString();
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> PChomePayRefundState(string refundid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var RequestUri = $"/v1/refund/{refundid}";
            try
            {
                response = await PChomePayHeaders();

                if (response.Success)
                {
                    response = new ResponseMessageDto();
                    var GetResponse = await ThirdPartyClient_PCHome.GetAsync(RequestUri);
                    GetResponse.EnsureSuccessStatusCode();
                    var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                    var pchomePayResponse = JsonConvert.DeserializeObject<PChomePayRefundStateDto>(jsonResponse);
                    if (pchomePayResponse.refund_id != null)
                    {
                        response.Success = true;
                        switch (pchomePayResponse.status)
                        {
                            case "INIT":
                                response.Message = $"退款編號{pchomePayResponse.refund_id}已於{DateTime.ParseExact(pchomePayResponse.refund_date, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd")}建立，退款金額為{pchomePayResponse.amount.ToString("$#,##0")}";
                                break;
                            case "WAIT":
                                response.Message = $"退款編號{pchomePayResponse.refund_id}正在處理中";
                                break;
                            case "SUCC":
                                response.Message = $"退款編號{pchomePayResponse.refund_id}已於{DateTime.ParseExact(pchomePayResponse.actual_refund_date, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd")}退款成功，退款金額為{pchomePayResponse.amount.ToString("$#,##0")}，退款手續費{pchomePayResponse.transfer_fee.ToString("$#,##0")}";
                                break;
                            case "FAIL":
                                response.Message = $"退款編號{pchomePayResponse.refund_id}退款失敗";
                                break;
                        }
                    }
                    else
                    {
                        response.Message = "查詢退款發生錯誤";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
