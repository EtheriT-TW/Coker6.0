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
using Microsoft.AspNetCore.Http;
using AutoMapper.Configuration.Conventions;
using MailKit.Search;
using System.Text.Json;
using System;

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
            var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
            try
            {
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
                                    if (ohdata.RepayTimes != null) ohdata.RepayDate = DateTime.Now;
                                    ohdata.State = OrderStatusEnum.待付款;
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
                        else throw new Exception("PChomePayHeader錯誤");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Message = $"Request Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                response.Message = $"Request Other Error: {ex.Message}";
            }
            if (!response.Success && ohdata != null)
            {
                var temp_response = await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.付款失敗);
                if (!temp_response.Success) response.Message += $"&{temp_response.Message}";
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
                    ResponseMessageDto response = await PChomePayCheckPaymentStatus(orderid);
                    if (!response.Success) throw new Exception("查無訂單狀態");
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
            try
            {
                JObject jsonMessage = JObject.Parse(dto.notify_message);

                Console.WriteLine($"-------------訊息查看-------------");
                Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：{dto}");
                Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：{jsonMessage}");

                if (dto.notify_type == "refund_success")
                {
                    return "success";
                }
                else
                {
                    if (jsonMessage.ContainsKey("order_id"))
                    {
                        string orderId = jsonMessage["order_id"] != null && !string.IsNullOrEmpty(jsonMessage["order_id"].ToString()) ? jsonMessage["order_id"].ToString() : "0";
                        var ohdata = await db.Order_Headers.Where(e => e.TransactionId == orderId).FirstOrDefaultAsync();
                        if (ohdata != null)
                        {
                            switch (jsonMessage["status"]?.ToString())
                            {
                                case "S":
                                    if (ohdata.State != OrderStatusEnum.已付款)
                                    {
                                        ohdata.State = OrderStatusEnum.已付款;
                                        DateTime paydate = jsonMessage["pay_date"] == null ? DateTime.Now : DateTime.ParseExact(jsonMessage["pay_date"].ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                        var send_mail = await orderAppService.PaySuccessMailSend(ohdata.Id, paydate);
                                        db.SaveChanges();
                                    }
                                    break;
                                case "W":
                                    if (ohdata.State != OrderStatusEnum.已付款 && ohdata.State != OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.待付款;
                                        db.SaveChanges();
                                    }
                                    break;
                                case "F":
                                    if (ohdata.State != OrderStatusEnum.付款失敗)
                                    {
                                        ohdata.State = OrderStatusEnum.付款失敗;
                                        db.SaveChanges();
                                    }
                                    break;
                            }
                            return "success";
                        }
                        else
                        {
                            Console.WriteLine($"-------------訊息查看-------------");
                            Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：order不存在");
                            return "fail";
                        }
                    }
                    else
                    {
                        Console.WriteLine($"-------------訊息查看-------------");
                        Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：order_id不存在");
                        return "fail";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------訊息查看-------------");
                Console.WriteLine($"PChomePay=>PChomePayNotify回傳資料：{ex.Message}");
                return "fail";
            }
        }
        public async Task<ResponseMessageDto> PChomePayCheckPaymentStatus(long ohid)
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
                        var RequestUri = $"/v1/payment/{ohdata.TransactionId}";
                        response = await PChomePayHeaders();

                        if (response.Success)
                        {
                            response = new ResponseMessageDto();
                            var GetResponse = await ThirdPartyClient_PCHome.GetAsync(RequestUri);

                            if (!GetResponse.IsSuccessStatusCode)
                            {
                                string responseBody = await GetResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"❌ 請求失敗: {GetResponse.StatusCode}");
                                Console.WriteLine($"🔹 Headers: {string.Join("\n", GetResponse.Headers)}");
                                Console.WriteLine($"🔹 Response Body: {responseBody}");

                                throw new Exception($"API 錯誤: {GetResponse.StatusCode}\n{responseBody}");
                            }
                            else GetResponse.EnsureSuccessStatusCode();

                            GetResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await GetResponse.Content.ReadAsStringAsync();
                            PChomePayState = JsonConvert.DeserializeObject<PChomePayStateDto>(jsonResponse);

                            var message = "";
                            var return_status = "fail";
                            switch (PChomePayState.status)
                            {
                                case "W":
                                    if (ohdata.State == OrderStatusEnum.待確認)
                                    {
                                        ohdata.State = OrderStatusEnum.待付款;
                                    }
                                    message = "交易處理中";
                                    return_status = "success";
                                    break;
                                case "S":
                                    if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.已付款;
                                        DateTime paydate = PChomePayState.pay_date == null ? DateTime.Now : DateTime.ParseExact(PChomePayState.pay_date.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                        var send_mail = await orderAppService.PaySuccessMailSend(ohdata.Id, paydate);
                                    }
                                    message = "交易已完成";
                                    return_status = "success";
                                    break;
                                default:
                                    if (ohdata.State == OrderStatusEnum.待確認 || ohdata.State == OrderStatusEnum.待付款)
                                    {
                                        ohdata.State = OrderStatusEnum.付款失敗;
                                    }
                                    message = "交易失敗：";
                                    break;
                            }
                            db.SaveChanges();
                            if (PChomePayState.status != null && return_status == "fail")
                            {
                                switch (PChomePayState.status_code)
                                {
                                    case "FE":
                                        message += "訂單逾時";
                                        break;
                                    case "FT":
                                        message += "連線失敗";
                                        break;
                                    case "FF":
                                    case "FA":
                                        message += "信用卡授權失敗";
                                        break;
                                    case "FF-1":
                                        message += "信用卡授權失敗-請與發卡銀行聯絡";
                                        break;
                                    case "FF-2":
                                        message += "信用卡授權失敗-拒絕交易";
                                        break;
                                    case "FF-3":
                                        message += "信用卡授權失敗-異常卡片";
                                        break;
                                    case "FF-4":
                                        message += "信用卡授權失敗-卡片過期";
                                        break;
                                    case "FF-5":
                                        message += "信用卡授權失敗-交易日期錯誤";
                                        break;
                                    case "FF-6":
                                        message += "信用卡授權失敗-交易逾時";
                                        break;
                                    case "FX":
                                        message += "ATM虛擬帳號失效";
                                        break;
                                    case "FP":
                                        message += "審單拒絕";
                                        break;
                                    case "FC":
                                        message += "合作方審單拒絕";
                                        break;
                                    case "FEL":
                                        message += "銀行支付超過限額";
                                        break;
                                    case "FEC":
                                        message += "銀行支付超過交易次數";
                                        break;
                                    case "FEB":
                                        message += "銀行支付帳戶存款不足";
                                        break;
                                    case "FEA":
                                        message += "銀行支付帳戶異常";
                                        break;
                                    case "FES":
                                        message += "銀行支付接收單位業務停止或關閉";
                                        break;
                                    case "FET":
                                        message += "銀行支付交易逾時";
                                        break;
                                    case "FB":
                                        message += "支付連餘額不足";
                                        break;
                                    case "WB":
                                        message += "尚未選擇銀行";
                                        break;
                                    case "WP":
                                        message += "ATM 待繳款";
                                        break;
                                    case "WAP":
                                        message += "審單中";
                                        break;
                                    case "WAC":
                                        message += "合作方自行審單中";
                                        break;
                                    case "WO":
                                        message += "等待 OTP 驗證";
                                        break;
                                    default:
                                        message += "未列出的原因";
                                        break;
                                }
                            }
                            response.Success = true;
                            response.Error = PChomePayState.status_code;
                            response.Message = $"{(int)ohdata.State},{message}";
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Http 請求錯誤
                response.Error = "Request Errors";
                response.Message = $"{0},{ex.Message}";
            }
            catch (Exception ex)
            {
                response.Error = "Other Errors";
                response.Message = $"{0},{ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> PChomePayRefund(long ohid, int? refund)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata != null)
                {
                    var ohidstr = ohdata.TransactionId;
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
                            if (!PostResponse.IsSuccessStatusCode)
                            {
                                string responseBody = await PostResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"❌ 請求失敗: {PostResponse.StatusCode}");
                                Console.WriteLine($"🔹 Headers: {string.Join("\n", PostResponse.Headers)}");
                                Console.WriteLine($"🔹 Response Body: {responseBody}");

                                // 可以拋出自訂例外，包含回應內容
                                throw new Exception($"API 錯誤: {PostResponse.StatusCode}\n{responseBody}");
                            }
                            else PostResponse.EnsureSuccessStatusCode();
                            var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                            var pchomePayResponse = JsonConvert.DeserializeObject<PChomeRefundDto>(jsonResponse);

                            if (pchomePayResponse != null && pchomePayResponse.order_id != null)
                            {
                                response.Success = true;
                                response.Message = pchomePayResponse.ToString();
                                ohdata.refundTransactionId = pchomePayResponse.refund_id; ;
                                ohdata.refundTransactionDate = DateTime.Now;
                                var temp_response = await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已取消);
                                if (!temp_response.Success)
                                {
                                    response.Success = false;
                                    response.Message = $"【{temp_response.Message}】{response.Message}";
                                }
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
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"-------------Refund Request failed-------------");

                response.Message = $"Refund Request failed: {ex.Message}";
                Console.WriteLine("(PChomePay Refund)HTTP Error: " + ex.Message);
                Console.WriteLine("(PChomePay Refund)Status Code: " + ex.StatusCode);
                Console.WriteLine("Response Body: " + ex.Data);
            }
            catch (Exception ex)
            {
                // 其他未知錯誤
                response.Message = $"Refund Other Error: {ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> PChomePayRefundState(string transactionId)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var ohdata = await db.Order_Headers.Where(e => e.TransactionId == transactionId).FirstOrDefaultAsync();
            try
            {
                if (ohdata != null)
                {
                    var RequestUri = $"/v1/refund/{ohdata.refundTransactionId}";
                    response = await PChomePayHeaders();

                    if (response.Success)
                    {
                        response = new ResponseMessageDto();
                        var GetResponse = await ThirdPartyClient_PCHome.GetAsync(RequestUri);

                        if (!GetResponse.IsSuccessStatusCode)
                        {
                            string responseBody = await GetResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"❌ 請求失敗: {GetResponse.StatusCode}");
                            Console.WriteLine($"🔹 Headers: {string.Join("\n", GetResponse.Headers)}");
                            Console.WriteLine($"🔹 Response Body: {responseBody}");

                            throw new Exception($"API 錯誤: {GetResponse.StatusCode}\n{responseBody}");
                        }
                        else GetResponse.EnsureSuccessStatusCode();

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
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> PChomePayCancelOrder(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                if (ohdata != null)
                {

                    ResponseMessageDto statusresponse = await PChomePayCheckPaymentStatus(ohid);

                    if (statusresponse.Success == true)
                    {
                        if (ohdata.TransactionId != null)
                        {
                            switch (ohdata.State)
                            {
                                case OrderStatusEnum.已付款:
                                case OrderStatusEnum.已出貨:
                                case OrderStatusEnum.已完成:
                                    response = await PChomePayRefund(ohdata.Id, null);
                                    if (response.Success) response.Message = "訂單已取消並送出退款申請。";
                                    break;
                                default:
                                    response = await orderAppService.OrderStateChange(ohid, (int)OrderStatusEnum.已取消);
                                    if (response.Success) response.Message = "訂單已取消。";
                                    break;
                            }
                        }
                        else
                        {
                            response = await orderAppService.OrderStateChange(ohid, (int)OrderStatusEnum.已取消);
                            if (response.Success) response.Message = "訂單已取消。";
                        }
                    }
                    else throw new Exception($"查詢訂單狀態發生錯誤：{statusresponse.Message}");
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (HttpRequestException ex)
            {
                response.Message = $"CancelOrder Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Message = $"CancelOrder Other failed: {ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> PChomePayHeaders()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var token = await tokenAppService.CheckToken(null);
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
                response.Message = $"Headers Request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Message = $"Headers Other failed: {ex.Message}";
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
                    if (ohdata.TransactionId == null) PaymentBody.order_id = $"{DateTime.Now.ToString("yyyyMMdd")}{oid}";
                    else
                    {
                        if (ohdata.RepayTimes == null) ohdata.RepayTimes = 1;
                        else ohdata.RepayTimes += 1;
                        db.SaveChanges();
                        PaymentBody.order_id = $"{DateTime.Now.ToString("yyyyMMdd")}{oid}-{ohdata.RepayTimes}";
                    }

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
                    var items_length = 0;
                    foreach (var oddata in oddatas)
                    {
                        var temp_item = new PChomePayItemsDto()
                        {
                            name = oddata.Title,
                            url = $"{Website.DefaultUrl}/{Website.OrgName}/home/product/{oddata.PId}"
                        };

                        int totalLength = temp_item.GetType()
                                                        .GetProperties()
                                                        .Where(prop => prop.PropertyType == typeof(string))
                                                        .Select(prop => prop.GetValue(temp_item) as string)
                                                        .Where(value => value != null)
                                                        .Sum(value => value.Length);

                        if (items_length + totalLength > 8000)
                        {
                            if (items.Count() > 0)
                            {
                                items.Last().name = "剩餘商品...";
                                items.Last().url = "";
                            }
                            else
                            {
                                temp_item.name = "商品...";
                                temp_item.url = "";
                                items.Add(temp_item);
                            }
                            break;
                        }
                        else
                        {
                            items_length += totalLength;
                            items.Add(temp_item);
                        }
                    }
                    PaymentBody.items = items;

                    PaymentBody.return_url = $"{Website.DefaultUrl}/api/ThirdParty/PChomePayReturn?ohid={oid}";
                    PaymentBody.fail_return_url = $"{Website.DefaultUrl}/api/ThirdParty/PChomePayReturn?ohid={oid}";
                    PaymentBody.notify_url = $"{Website.DefaultUrl}/api/ThirdParty/PChomePayNotify";

                    PaymentBody.buyer_email = ohdata.OrdererEmail;

                    var tpkv_Value = await (from tpkv in db.ThirdPartyKeypairValues
                                            join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                            join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                            where tp.Title == "支付連"
                                            where tpkv.FK_WebsiteId == WebsiteId
                                            where tpk.Code == "expire_days"
                                            select tpkv.Value).FirstOrDefaultAsync();
                    var expire_days = tpkv_Value == null ? 5 : int.Parse(tpkv_Value) < 1 ? 1 : int.Parse(tpkv_Value) > 5 ? 5 : int.Parse(tpkv_Value);
                    PaymentBody.atm_info = new PChomePayPaymentDto.PChomePayPaymentInfo() { expire_days = expire_days };

                    PaymentBody.return_timer = "Y";
                    PaymentBody.member_key = ohdata.Fk_UserId?.ToString() ?? "";
                    Console.WriteLine($"-------------訊息查看-------------");
                    Console.WriteLine($"PChomePay=>PChomePayPaymentBody：{PaymentBody}");
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
    }
}
