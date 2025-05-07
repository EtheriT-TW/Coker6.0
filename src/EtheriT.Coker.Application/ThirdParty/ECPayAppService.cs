using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto.ECPayRequestDto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ECPayAppService : IECPayAppService
    {
        private readonly HttpClient ThirdPartyClient_ECPay;
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IOrderAppService orderAppService;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment _env;
        public ECPayAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IOrderAppService orderAppService,
            IMapper mapper,
            IWebHostEnvironment env
        )
        {
            ThirdPartyClient_ECPay = httpClientFactory.CreateClient("ThirdPartyClient_ECPay");
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.orderAppService = orderAppService;
            this.mapper = mapper;
            this._env = env;
        }
        // 查詢訂單明細
        public async Task<ResponseMessageDto> ECPayOrderState(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                if (ohdata != null)
                {
                    var ThirdPartyData = await ECPayGetThirdPartyData();
                    if (ThirdPartyData != null)
                    {
                        ECPayRequestDto RequestBody = new ECPayRequestDto();
                        var DateTimeNow = DateTime.Now;
                        var RequestUri = ThirdPartyClient_ECPay.BaseAddress?.ToString().Replace("ecpg", "ecpayment") + "/1.0.0/Cashier/QueryTrade";

                        if (ThirdPartyData.PlatformID != "") RequestBody.MerchantID = ThirdPartyData.PlatformID;
                        else RequestBody.MerchantID = ThirdPartyData.MerchantID;
                        RequestBody.RqHeader = new RqHeaderDto();
                        RequestBody.RqHeader.Timestamp = ((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds().ToString();

                        ECPayQueryTradeDataDto requestData = new ECPayQueryTradeDataDto();
                        requestData.PlatformID = ThirdPartyData.PlatformID;
                        requestData.MerchantID = ThirdPartyData.MerchantID;
                        requestData.MerchantTradeNo = ohdata.TransactionId ?? "";

                        RequestBody.Data = Encrypt(requestData, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                        var queryTradeResponse = await ECPaySendRequest("ECPayGetQueryTrade", RequestUri, RequestBody);

                        if (queryTradeResponse.Success)
                        {
                            var message = "交易處理中";
                            if (ohdata.State == OrderStatusEnum.待付款)
                            {
                                if (queryTradeResponse.OrderInfo.TradeStatus == "1")
                                {
                                    message = "交易已完成";
                                    await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已付款);
                                }
                                else
                                {
                                    DateTime tradeDate = DateTime.ParseExact(queryTradeResponse.OrderInfo.TradeDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    switch (queryTradeResponse.OrderInfo.PaymentType)
                                    {
                                        case "Credit":
                                        case "CreditInstallment":
                                        case "UnionPay":
                                        case "ApplePay":
                                            if (DateTimeNow > tradeDate.AddMinutes(30))
                                            {
                                                message = "交易失敗";
                                                await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.付款失敗);
                                            }
                                            break;
                                        case "ATM":
                                            if (DateTimeNow > tradeDate.AddDays(double.Parse(ThirdPartyData.ExpireDate)))
                                            {
                                                message = "交易失敗";
                                                await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.付款失敗);
                                            }
                                            break;
                                        case "CVS":
                                            if (DateTimeNow > tradeDate.AddDays(double.Parse(ThirdPartyData.StoreExpireDate_CVS)))
                                            {
                                                message = "交易失敗";
                                                await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.付款失敗);
                                            }
                                            break;
                                        case "Barcode":
                                            if (DateTimeNow > tradeDate.AddDays(double.Parse(ThirdPartyData.StoreExpireDate_Barcode)))
                                            {
                                                message = "交易失敗";
                                                await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.付款失敗);
                                            }
                                            break;
                                    }
                                }
                            }
                            response.Success = true;
                            response.Message = $"{(int)ohdata.State},{message}";
                        }
                        else throw new Exception(queryTradeResponse.Message);
                    }
                    else throw new Exception("取得綠界支付資料發生錯誤，請確認是否正確設置");
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayRefund(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync() ?? throw new Exception("查無訂單資訊");
                var payment = await db.PaymentTypes.Where(e => e.Id == ohdata.Payment).FirstOrDefaultAsync() ?? throw new Exception("查無訂單付款方式");
                var DateTimeNow = DateTime.Now;

                switch (ohdata.State)
                {
                    case OrderStatusEnum.待確認:
                        await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已取消);
                        await orderAppService.CancelOrderMailSend(ohdata.Id, DateTimeNow);
                        response.Success = true;
                        break;
                    case OrderStatusEnum.已取消:
                        throw new Exception("該筆訂單已取消");
                    default:
                        if (!payment.CanRefund) throw new Exception("訂單無法取消，請聯繫客服協助處理。");
                        var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("取得綠界支付資料發生錯誤，請確認是否正確設置");

                        var RequestUri = ThirdPartyClient_ECPay.BaseAddress?.ToString().Replace("ecpg", "ecpayment") + "/1.0.0/Credit/DoAction";
                        if (RequestUri.Contains("-stage")) throw new Exception("測試環境無法退刷");

                        var CreditDetailGetResponse = await ECPayCreditDetailGet(ohdata.Id);
                        if (!CreditDetailGetResponse.Success) throw new Exception(CreditDetailGetResponse.Message);

                        ECPayRequestDto RequestBody = new ECPayRequestDto();

                        if (ThirdPartyData.PlatformID != "") RequestBody.MerchantID = ThirdPartyData.PlatformID;
                        else RequestBody.MerchantID = ThirdPartyData.MerchantID;
                        RequestBody.RqHeader = new RqHeaderDto();
                        RequestBody.RqHeader.Timestamp = ((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds().ToString();

                        ECPayRefundDataDto refundRequestData = new ECPayRefundDataDto();
                        refundRequestData.PlatformID = ThirdPartyData.PlatformID;
                        refundRequestData.MerchantID = ThirdPartyData.MerchantID;
                        refundRequestData.MerchantTradeNo = ohdata.TransactionId ?? "";
                        refundRequestData.TradeNo = CreditDetailGetResponse.TradeNo;
                        switch (CreditDetailGetResponse.Status)
                        {
                            case "Canceled":
                            case "Operation canceled":
                                await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已取消);
                                await orderAppService.CancelOrderMailSend(ohdata.Id, DateTimeNow);
                                response.Success = true;
                                break;
                            case "Unauthorized":
                                DateTime tradeDate = DateTime.ParseExact(CreditDetailGetResponse.AuthTime, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                                if (DateTimeNow > tradeDate.AddMinutes(30))
                                {
                                    await orderAppService.OrderStateChange(ohdata.Id, (int)OrderStatusEnum.已取消);
                                    await orderAppService.CancelOrderMailSend(ohdata.Id, DateTimeNow);
                                    response.Success = true;
                                }
                                break;
                            case "Authorized":
                                refundRequestData.Action = "N";
                                break;
                            case "To be captured":
                                refundRequestData.Action = "E";
                                break;
                            case "Captured":
                                refundRequestData.Action = "R";
                                break;
                        }
                        refundRequestData.TotalAmount = CreditDetailGetResponse.Amount;

                        RequestBody.Data = Encrypt(refundRequestData, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                        var refundResponse = await ECPaySendRequest("ECPayGetQueryTrade", RequestUri, RequestBody);
                        if (!refundResponse.Success) throw new Exception(refundResponse.Message);

                        if (refundRequestData.Action == "E")
                        {
                            refundRequestData.Action = "N";
                            RequestBody.Data = Encrypt(refundRequestData, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                            refundResponse = await ECPaySendRequest("ECPayGetQueryTrade", RequestUri, RequestBody);

                            if (!refundResponse.Success) throw new Exception(refundResponse.Message);
                        }

                        ohdata.State = OrderStatusEnum.已取消;
                        ohdata.refundTransactionId = refundRequestData.TradeNo;
                        ohdata.refundTransactionDate = DateTimeNow;
                        db.SaveChanges();
                        await orderAppService.CancelOrderMailSend(ohdata.Id, DateTimeNow);
                        response.Success = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ECPayCreditDetailReturnDto> ECPayCreditDetailGet(long ohid)
        {
            ECPayCreditDetailReturnDto response = new ECPayCreditDetailReturnDto();
            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync() ?? throw new Exception("查無訂單資訊");

                var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("取得綠界支付資料發生錯誤，請確認是否正確設置");

                ECPayRequestDto RequestBody = new ECPayRequestDto();
                var DateTimeNow = DateTime.Now;
                var RequestUri = ThirdPartyClient_ECPay.BaseAddress?.ToString().Replace("ecpg", "ecpayment") + "/1.0.0/Cashier/QueryTrade";

                if (ThirdPartyData.PlatformID != "") RequestBody.MerchantID = ThirdPartyData.PlatformID;
                else RequestBody.MerchantID = ThirdPartyData.MerchantID;
                RequestBody.RqHeader = new RqHeaderDto();
                RequestBody.RqHeader.Timestamp = ((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds().ToString();

                ECPayQueryTradeDataDto requestData = new ECPayQueryTradeDataDto();
                requestData.PlatformID = ThirdPartyData.PlatformID;
                requestData.MerchantID = ThirdPartyData.MerchantID;
                requestData.MerchantTradeNo = ohdata.TransactionId ?? "";

                RequestBody.Data = Encrypt(requestData, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                var queryTradeResponse = await ECPaySendRequest("ECPayGetQueryTrade", RequestUri, RequestBody);
                if (!queryTradeResponse.Success) throw new Exception(queryTradeResponse.Message);
                RequestUri = "/1.0.0/CreditDetail/QueryTrade";

                ECPayCreditDetailDataDto creditDetailRequestData = new ECPayCreditDetailDataDto();
                creditDetailRequestData = mapper.Map<ECPayCreditDetailDataDto>(requestData);
                creditDetailRequestData.TradeNo = queryTradeResponse.OrderInfo.TradeNo;

                RequestBody.Data = Encrypt(creditDetailRequestData, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                var content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                var PostResponse = await ThirdPartyClient_ECPay.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                var creditDetailResponse = JsonConvert.DeserializeObject<ECPayResponseDto>(jsonResponse) ?? throw new Exception("無法取得ECPayCreditDetail");

                if (!creditDetailResponse.TransMsg.ToLower().Contains("success")) throw new Exception($"取得ECPayCreditDetail發生錯誤，{JsonConvert.SerializeObject(creditDetailResponse, Formatting.Indented)}");

                var data = Decrypt(creditDetailResponse.Data, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                var creditDetailResponseData = JsonConvert.DeserializeObject<ECPayCreditDetailResponseDataDto>(data as string);
                switch (creditDetailResponseData.RtnMsg)
                {
                    case "error_Stop":
                        throw new Exception($"取得ECPayCreditDetail發生錯誤，查無商家或商家己到期，{JsonConvert.SerializeObject(creditDetailResponseData, Formatting.Indented)}");
                    case "error_nopay":
                        throw new Exception($"取得ECPayCreditDetail發生錯誤，查無該筆交易授權單號，{JsonConvert.SerializeObject(creditDetailResponseData, Formatting.Indented)}");
                    case "error":
                        throw new Exception($"取得ECPayCreditDetail發生錯誤或資料檢核失敗，{JsonConvert.SerializeObject(creditDetailResponseData, Formatting.Indented)}");
                    default:
                        response.TradeNo = queryTradeResponse.OrderInfo.TradeNo;
                        response.Status = creditDetailResponseData.RtnValue.Status;
                        response.Amount = creditDetailResponseData.RtnValue.Amount;
                        response.AuthTime = creditDetailResponseData.RtnValue.AuthTime;
                        response.Success = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<IActionResult> ECPayOrderResult(string ResultData)
        {
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("商家未確實設置綠界支付資料");

                ECPayResponseDto ResultResponseData = JsonConvert.DeserializeObject<ECPayResponseDto>(ResultData);

                if (ResultResponseData == null) throw new Exception("無法取得ECPayOrderResult");

                if (!ResultResponseData.TransMsg.ToLower().Contains("success")) throw new Exception($"取得ECPayOrderResult發生錯誤，{JsonConvert.SerializeObject(ResultResponseData, Formatting.Indented)}");

                var data = Decrypt(ResultResponseData.Data, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                var ResponseData = JsonConvert.DeserializeObject<ECPayResponseDataDto>(data as string);
                if (ResponseData.RtnCode != 1) throw new Exception($"取得ECPayOrderResult發生錯誤，{JsonConvert.SerializeObject(ResponseData, Formatting.Indented)}");

                var ohdata = await db.Order_Headers.Where(e => e.TransactionId == ResponseData.OrderInfo.MerchantTradeNo).FirstOrDefaultAsync();
                if (ResponseData.OrderInfo.PaymentType == "CreditInstallment") ohdata.Payment = await db.PaymentTypes.Where(e => e.Code.StartsWith("ECPay") && e.Code.Contains($"CreditInstallment_{ResponseData.CardInfo.Stage}")).Select(e => e.Id).FirstOrDefaultAsync();
                else ohdata.Payment = await db.PaymentTypes.Where(e => e.Code.StartsWith("ECPay") && e.Code.Contains(ResponseData.OrderInfo.PaymentType)).Select(e => e.Id).FirstOrDefaultAsync();
                db.SaveChanges();

                return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar?{("000000000" + ohdata.Id).Substring(ohdata.Id.ToString().Length)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPay=>ECPayOrderResult回傳資料：{ex.Message}");
            }
            return new LocalRedirectResult($"/{Website.OrgName}/ShoppingCar");
        }
        public async Task<String> ECPayReturn(ECPayResponseDto ResultResponseData)
        {
            Console.WriteLine($"-------------ECPayReturn訊息查看-------------");
            Console.WriteLine($"ECPayReturn回傳資料：{ResultResponseData}");
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("商家未確實設置綠界支付資料");
                if (!ResultResponseData.TransMsg.ToLower().Contains("success")) throw new Exception($"取得ECPayReturn發生錯誤，{JsonConvert.SerializeObject(ResultResponseData, Formatting.Indented)}");
                var data = Decrypt(ResultResponseData.Data, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                var ResponseData = JsonConvert.DeserializeObject<ECPayResponseDataDto>(data as string);
                if (ResponseData.RtnCode != 1) throw new Exception($"取得ECPayReturn發生錯誤，{JsonConvert.SerializeObject(ResponseData, Formatting.Indented)}");

                var ohdata = await db.Order_Headers.Where(e => e.TransactionId == ResponseData.OrderInfo.MerchantTradeNo).FirstOrDefaultAsync();
                if (ResponseData.OrderInfo.TradeStatus == "1") ohdata.CompletedDate = DateTime.ParseExact(ResponseData.OrderInfo.TradeDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                if (ohdata.State == OrderStatusEnum.待付款 && ResponseData.OrderInfo.TradeStatus == "1") ohdata.State = OrderStatusEnum.已付款;
                db.SaveChanges();
                DateTime paydate = DateTime.ParseExact(ResponseData.OrderInfo.TradeDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                var send_mail = await orderAppService.PaySuccessMailSend(ohdata.Id, paydate);
                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayReturn", JsonConvert.SerializeObject(ResponseData));
                return "OK";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPay=>ECPayReturn回傳資料：{ex.Message}");
                return "Fail";
            }
        }
        public async Task<ResponseMessageDto> ECPayCreatePayment(ECPayPaymentInfoDto PaymentInfo)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();

                if (ThirdPartyData != null)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.TransactionId == PaymentInfo.MerchantTradeNo).FirstOrDefaultAsync();

                    if (ohdata != null)
                    {
                        var RequestUri = "/Merchant/CreatePayment";

                        ECPayRequestDto RequestBody = new ECPayRequestDto();

                        if (ThirdPartyData.PlatformID != "") RequestBody.MerchantID = ThirdPartyData.PlatformID;
                        else RequestBody.MerchantID = ThirdPartyData.MerchantID;

                        var DateTimeNow = DateTime.Now;
                        RequestBody.RqHeader = new RqHeaderDto();
                        RequestBody.RqHeader.Timestamp = ((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds().ToString();

                        ECPayCreatePaymentDataDto PaymentData = new ECPayCreatePaymentDataDto();
                        PaymentData.PlatformID = ThirdPartyData.PlatformID;
                        PaymentData.MerchantID = ThirdPartyData.MerchantID;
                        PaymentData.PayToken = PaymentInfo.PayToken;
                        PaymentData.MerchantTradeNo = PaymentInfo.MerchantTradeNo;

                        RequestBody.Data = Encrypt(PaymentData, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                        var createPaymentResponse = await ECPaySendRequest("ECPayCreatePayment", RequestUri, RequestBody);

                        if (createPaymentResponse.Success)
                        {
                            response.Message = JsonConvert.SerializeObject(createPaymentResponse);
                            response.Success = true;
                            ohdata.Payment = await db.PaymentTypes.Where(e => e.Code.StartsWith("ECPay") && e.Code.Contains(createPaymentResponse.OrderInfo.PaymentType)).Select(e => e.Id).FirstOrDefaultAsync();
                            if (ohdata.Payment == 0) ohdata.Payment = 16;
                            ohdata.State = OrderStatusEnum.待付款;
                            db.SaveChanges();
                        }
                        else throw new Exception(createPaymentResponse.Message);
                    }
                    else throw new Exception("查無訂單資訊");
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Request failed: {ex.Message}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayGetToken(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();

                if (ThirdPartyData != null)
                {
                    var RequestUri = "/Merchant/GetTokenbyTrade";
                    var RequestBody = await ECPayRequestBody(ThirdPartyData, ohid);

                    if (RequestBody != null)
                    {
                        var tokenResponse = await ECPaySendRequest("ECPayGetToken", RequestUri, RequestBody);

                        if (tokenResponse.Success)
                        {
                            response.Success = true;
                            response.Message = tokenResponse.Token;
                        }
                        else throw new Exception(tokenResponse.Message);
                    }
                    else throw new Exception("查詢訂單資訊發生錯誤");
                }
                else throw new Exception("商家未確實設置綠界支付資料");
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
        private async Task<ECPayResponseDataDto> ECPaySendRequest(string RequestName, string RequestUri, ECPayRequestDto RequestBody)
        {
            ECPayResponseDataDto response = new ECPayResponseDataDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();
                if (ThirdPartyData != null)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                    var PostResponse = await ThirdPartyClient_ECPay.PostAsync(RequestUri, content);
                    PostResponse.EnsureSuccessStatusCode();
                    var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                    var Response = JsonConvert.DeserializeObject<ECPayResponseDto>(jsonResponse);
                    if (Response != null)
                    {
                        if (Response.TransMsg.ToLower().Contains("success"))
                        {
                            var data = Decrypt(Response.Data, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                            var ResponseData = JsonConvert.DeserializeObject<ECPayResponseDataDto>(data as string);
                            if (ResponseData.RtnCode == 1)
                            {
                                response = ResponseData;
                                response.Success = true;
                            }
                            else throw new Exception($"取得{RequestName}發生錯誤，{JsonConvert.SerializeObject(ResponseData, Formatting.Indented)}");
                        }
                        else throw new Exception($"取得{RequestName}發生錯誤，{JsonConvert.SerializeObject(Response, Formatting.Indented)}");
                    }
                    else throw new Exception($"無法取得{RequestName}");
                }
                else throw new Exception("商家未確實設置綠界支付資料");
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
        private async Task<ECPayRequestDto> ECPayRequestBody(ECPayThirdPartyDataDto ThirdPartyData, long ohid)
        {
            ECPayRequestDto RequestBody = new ECPayRequestDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var DateTimeNow = DateTime.Now;

            try
            {
                if (ThirdPartyData.MerchantID != "" && ThirdPartyData.HashKey != "" && ThirdPartyData.HashIV != "")
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                    var oddatas = await orderAppService.GetOrderDetails(ohid);

                    if (ThirdPartyData.PlatformID != "") RequestBody.MerchantID = ThirdPartyData.PlatformID;
                    else RequestBody.MerchantID = ThirdPartyData.MerchantID;

                    RequestBody.RqHeader = new RqHeaderDto();
                    RequestBody.RqHeader.Timestamp = ((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds().ToString();

                    if (ohdata != null & oddatas.Any())
                    {
                        var user = await db.FrontUsers.Where(e => e.UUID == ohdata.FK_UUID).FirstOrDefaultAsync();

                        ECPayGetTokenDataDto PaymentBody = new ECPayGetTokenDataDto();
                        PaymentBody.MerchantID = ThirdPartyData.MerchantID;
                        PaymentBody.PlatformID = ThirdPartyData.PlatformID;
                        PaymentBody.RememberCard = user == null ? 0 : 1;
                        PaymentBody.PaymentUIType = 2;

                        ECPayCardInfoDto CardInfo = new ECPayCardInfoDto();
                        ECPayUnionPayInfoDto UnionPayInfo = new ECPayUnionPayInfoDto();
                        ECPayATMInfoDto ATMInfo = new ECPayATMInfoDto();
                        ECPayCVSInfoDto CVSInfo = new ECPayCVSInfoDto();
                        ECPayBarcodeInfoDto BarcodeInfo = new ECPayBarcodeInfoDto();

                        var paytypes = await (from ptv in db.PaymentTypesValues
                                              join pt in db.PaymentTypes on ptv.FK_PaymentTypesId equals pt.Id
                                              where ptv.Used && ptv.FK_WebsiteId == WebsiteId
                                              where pt.FK_ThirdPartyId == 4
                                              select pt).ToListAsync();

                        if (paytypes.Any())
                        {

                            if (_env.IsProduction())
                            {
                                CardInfo.OrderResultURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayOrderResult";
                                UnionPayInfo.OrderResultURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayOrderResult";
                            }
                            else
                            {
                                CardInfo.OrderResultURL = "https://lcb.develop.coker.ezsale.tw/api/ThirdParty/ECPayOrderResult";
                                UnionPayInfo.OrderResultURL = "https://lcb.develop.coker.ezsale.tw/api/ThirdParty/ECPayOrderResult";
                            }

                            foreach (var paytype in paytypes)
                            {
                                var temp_paytype = paytype.Code.Substring("ECPay".Length);
                                var code = temp_paytype.Split('_');
                                var value = code.Length > 1 ? code[1] : null;

                                if (PaymentBody.ChoosePaymentList != "") PaymentBody.ChoosePaymentList += ",";

                                switch (code[0].ToString())
                                {
                                    case "CreditCard":
                                        PaymentBody.ChoosePaymentList += "1";
                                        break;
                                    case "CreditInstallment":
                                        PaymentBody.ChoosePaymentList += "2";
                                        if (CardInfo.CreditInstallment != "") CardInfo.CreditInstallment += ",";
                                        CardInfo.CreditInstallment += value;
                                        break;
                                    case "ATM":
                                        PaymentBody.ChoosePaymentList += "3";
                                        ATMInfo.ExpireDate = int.Parse(ThirdPartyData.ExpireDate);
                                        break;
                                    case "CVS":
                                        PaymentBody.ChoosePaymentList += "4";
                                        CVSInfo.StoreExpireDate = int.Parse(ThirdPartyData.StoreExpireDate_CVS);
                                        CVSInfo.CVSCode = "CVS";
                                        CVSInfo.Desc_1 = $"{Website.Title}-商品購買交易";
                                        break;
                                    case "Barcode":
                                        PaymentBody.ChoosePaymentList += "5";
                                        BarcodeInfo.StoreExpireDate = int.Parse(ThirdPartyData.StoreExpireDate_Barcode);
                                        break;
                                    case "UnionPay":
                                        PaymentBody.ChoosePaymentList += "6";
                                        break;
                                    case "ApplePay":
                                        PaymentBody.ChoosePaymentList += "7";
                                        break;
                                }
                            }

                            ECPayOrderInfoDto OrderInfo = new ECPayOrderInfoDto();
                            OrderInfo.MerchantTradeDate = DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");
                            var oid = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                            if (ohdata.TransactionId == null) OrderInfo.MerchantTradeNo = $"{DateTimeNow.ToString("yyyyMMdd")}{oid}";
                            else
                            {
                                if (ohdata.RepayTimes == null) ohdata.RepayTimes = 1;
                                else ohdata.RepayTimes += 1;
                                db.SaveChanges();
                                OrderInfo.MerchantTradeNo = $"{DateTimeNow.ToString("yyyyMMdd")}{oid}R{ohdata.RepayTimes}";
                                ohdata.RepayDate = DateTimeNow;
                            }

                            ohdata.TransactionId = OrderInfo.MerchantTradeNo;
                            db.SaveChanges();

                            OrderInfo.TotalAmount = ohdata.Subtotal + ohdata.Freight;

                            if (_env.IsProduction()) OrderInfo.ReturnURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayReturn";
                            else OrderInfo.ReturnURL = "https://lcb.develop.coker.ezsale.tw/api/ThirdParty/ECPayReturn";

                            OrderInfo.TradeDesc = $"{Website.Title}-商品購買交易";

                            var itemlist = "";
                            foreach (var oddata in oddatas)
                            {
                                var title = oddata.Title.Length > 200 ? $"{oddata.Title.Substring(0, 197)}..." : oddata.Title;
                                if (string.IsNullOrEmpty(itemlist)) itemlist = title;
                                else
                                {
                                    title = $"#{title}";
                                    if (itemlist.Length + title.Length > 194)
                                    {
                                        itemlist += "#其他...";
                                        break;
                                    }
                                    itemlist += title;
                                }
                            }
                            OrderInfo.ItemName = itemlist;

                            PaymentBody.OrderInfo = OrderInfo;
                            PaymentBody.CardInfo = CardInfo;
                            PaymentBody.UnionPayInfo = UnionPayInfo;
                            PaymentBody.ATMInfo = ATMInfo;
                            PaymentBody.CVSInfo = CVSInfo;
                            PaymentBody.BarcodeInfo = BarcodeInfo;

                            ECPayConsumerInfoDto ConsumerInfo = new ECPayConsumerInfoDto();
                            ConsumerInfo.MerchantMemberID = user?.Id.ToString();
                            ConsumerInfo.Email = ohdata.OrdererEmail;
                            ConsumerInfo.Phone = ohdata.OrdererCellPhone;
                            ConsumerInfo.Name = ohdata.Orderer;
                            ConsumerInfo.CountryCode = "158";
                            ConsumerInfo.Address = ohdata.OrdererAddress.Replace(" ", "");

                            PaymentBody.ConsumerInfo = ConsumerInfo;

                            RequestBody.Data = Encrypt(PaymentBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                        }
                        else throw new Exception("付款資訊錯誤");
                    }
                    else throw new Exception($"查無訂單({ohid})資訊");
                }
                else throw new Exception("查無ECPay所需參數");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPay=>ECPayRequestBody回傳資料：{ex.Message}");
            }
            return RequestBody;
        }
        private async Task<ECPayThirdPartyDataDto> ECPayGetThirdPartyData()
        {
            ECPayThirdPartyDataDto ThirdPartyData = new ECPayThirdPartyDataDto();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

                var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                     join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                     join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                     where tp.Title == "綠界支付"
                                                     where tpkv.FK_WebsiteId == WebsiteId
                                                     select new KeyValueDto() { Key = tpk.Code, Value = tpkv.Value }).ToListAsync();
                if (thirdPartyKeypairValues != null)
                {
                    var MerchantID = thirdPartyKeypairValues.Find(e => e.Key == "MerchantID")?.Value;
                    var HashKey = thirdPartyKeypairValues.Find(e => e.Key == "HashKey")?.Value;
                    var HashIV = thirdPartyKeypairValues.Find(e => e.Key == "HashIV")?.Value;
                    if (MerchantID == null || HashKey == null || HashIV == null) throw new Exception("查無ThirdParty資料缺漏");
                    else
                    {
                        var thirdPartyDict = thirdPartyKeypairValues.ToDictionary(e => e.Key, e => e.Value);
                        ThirdPartyData.MerchantID = thirdPartyDict.GetValueOrDefault("MerchantID") ?? "";
                        ThirdPartyData.PlatformID = thirdPartyDict.GetValueOrDefault("PlatformID") ?? "";
                        ThirdPartyData.HashKey = thirdPartyDict.GetValueOrDefault("HashKey") ?? "";
                        ThirdPartyData.HashIV = thirdPartyDict.GetValueOrDefault("HashIV") ?? "";
                        ThirdPartyData.ExpireDate = thirdPartyDict.GetValueOrDefault("ExpireDate") ?? "";
                        ThirdPartyData.ExpireDate = GetExpireDate(ThirdPartyData.ExpireDate, 3, 1, 60).ToString();
                        ThirdPartyData.StoreExpireDate_CVS = thirdPartyDict.GetValueOrDefault("StoreExpireDate_CVS") ?? "";
                        ThirdPartyData.StoreExpireDate_CVS = GetExpireDate(ThirdPartyData.StoreExpireDate_CVS, 7, 1, 30).ToString();
                        ThirdPartyData.StoreExpireDate_Barcode = thirdPartyDict.GetValueOrDefault("StoreExpireDate_Barcode") ?? "";
                        ThirdPartyData.StoreExpireDate_Barcode = GetExpireDate(ThirdPartyData.StoreExpireDate_Barcode, 7, 1, 30).ToString();
                    }
                }
                else throw new Exception("查無ThirdParty資料");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPay=>ECPayGetThirdPartyData回傳資料：{ex.Message}");
            }
            return ThirdPartyData;
        }
        private int GetExpireDate(string value, int defaultValue, int min, int max)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            if (!int.TryParse(value, out var parsed)) return defaultValue;
            return Math.Clamp(parsed, min, max);
        }
        private string Encrypt(object data, string hashKey, string hashIV)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            string urlEncodedData = HttpUtility.UrlEncode(jsonData);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(hashKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(hashIV);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encrypted = null;
                using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(urlEncodedData);
                        }
                    }
                    encrypted = msEncrypt.ToArray();
                }

                return Convert.ToBase64String(encrypted);
            }
        }
        private object Decrypt(string data, string hashKey, string hashIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(hashKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(hashIV);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] cipherText = Convert.FromBase64String(data);

                using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            string decryptedData = srDecrypt.ReadToEnd();

                            string decodedData = HttpUtility.UrlDecode(decryptedData);

                            return decodedData;
                        }
                    }
                }
            }
        }
    }
}
