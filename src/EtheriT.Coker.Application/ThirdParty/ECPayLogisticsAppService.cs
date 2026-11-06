using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ECPayLogisticsAppService : IECPayLogisticsAppService
    {
        private readonly HttpClient ThirdPartyClient_ECPayLogistics;
        private readonly CokerDbContext db;
        private readonly IOrderAppService orderAppService;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public ECPayLogisticsAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            IOrderAppService orderAppService,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            ThirdPartyClient_ECPayLogistics = httpClientFactory.CreateClient("ThirdPartyClient_ECPayLogistics");
            this.db = db;
            this.orderAppService = orderAppService;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.mapper = mapper;
        }
        public async Task<ECPayLogisticsMapRequestDto> ECPayLogisticsGetMapRequestBody(string SCIds, string LogisticsSubType, string IsCollection)
        {
            ECPayLogisticsMapRequestDto RequestBody = new ECPayLogisticsMapRequestDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

                var MerchantID = "";

                if (string.IsNullOrWhiteSpace(ThirdPartyData?.MerchantID)) MerchantID = configuration.GetValue<string>("ThirdParty:ECPayLogistics:CokerMerchantID");
                else MerchantID = ThirdPartyData.MerchantID;

                RequestBody.MerchantID = MerchantID;
                RequestBody.MerchantTradeNo = GenMerchantTradeNo();
                RequestBody.LogisticsSubType = LogisticsSubType;
                RequestBody.IsCollection = IsCollection;
                RequestBody.ServerReplyURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayLogisticsGetMapResponse";
                RequestBody.ExtraData = SCIds;
                return RequestBody;
            }
            catch (Exception ex)
            {
                await loginUserData.SetLogs(
                    0,
                    configuration.GetValue<long>("WebConfig:SiteId"),
                    "BuildECPayLogisticsMapRequest",
                    $"failed: {ex.Message}"
                );
                throw;
            }
        }
        private static string GenMerchantTradeNo()
        {
            Random _random = new Random();

            var now = DateTime.Now;

            string yyyy = now.Year.ToString();
            string MM = now.Month.ToString("D2");
            string dd = now.Day.ToString("D2");
            string hh = now.Hour.ToString("D2");
            string mm = now.Minute.ToString("D2");
            string ss = now.Second.ToString("D2");

            string rand = _random.Next(0, 10000).ToString("D4");

            return $"{yyyy}{MM}{dd}{hh}{mm}{ss}{rand}";
        }
        public async Task<ResponseMessageDto> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

            try
            {
                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsExpressCreate", JsonConvert.SerializeObject(ResultResponseData));

                var scids = JsonConvert.DeserializeObject<List<long>>(ResultResponseData.ExtraData);
                var scdatas = await db.ShoppingCarts.Where(e => scids.Contains(e.Id)).ToListAsync();

                if (!scdatas.Any()) throw new Exception($"查無購物車資訊");

                foreach (var scdata in scdatas)
                {
                    scdata.LogisticsSubType = ResultResponseData.LogisticsSubType;
                    scdata.CVSStoreID = ResultResponseData.CVSStoreID;
                    scdata.CVSStoreName = ResultResponseData.CVSStoreName;
                    scdata.CVSAddress = ResultResponseData.CVSAddress;
                    scdata.CVSTelephone = ResultResponseData.CVSTelephone;
                    scdata.CVSOutSide = ResultResponseData.CVSOutSide;
                }
                db.SaveChanges();

                response.Success = true;
                response.Message = $"/{Website.OrgName}/ShoppingCar";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayLogisticsGetMapResponse回傳資料：{ex.Message}");
                response.Message = $"/{Website.OrgName}/ShoppingCar?error={Uri.EscapeDataString(ex.Message)}";
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsExpressCVSCreate(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var RequestUri = $"{configuration["ThirdParty:ECPayLogistics:LogisticsUrl"]}/Express/Create";

                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && !e.IsDeleted && e.FK_WebsiteId == configuration.GetValue<long>("WebConfig:SiteId")).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");

                var RequestBody = await ECPayExpressCVSRequestBody(ThirdPartyData, ohdata);

                var RequestDict = RequestBody.GetType().GetProperties().ToDictionary(
                        p => p.Name,
                        p => p.GetValue(RequestBody)?.ToString() ?? ""
                    );

                var content = new FormUrlEncodedContent(RequestDict);

                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Clear();
                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                var parts = jsonResponse.Split('|', 2);
                var status = parts[0];
                var dataPart = parts.Length == 2 ? parts[1] : jsonResponse;

                if (status != "1") response.Message = dataPart;
                else
                {
                    var ResponseDict = ParseVerifiedResponse(dataPart, ThirdPartyData);

                    var json = JsonConvert.SerializeObject(ResponseDict);

                    var ResponseDto = JsonConvert.DeserializeObject<ECPayLogisticsCreateResponseDto>(json);

                    var logistics = await db.Order_Logistics.Where(e => e.MerchantTradeNo == ResponseDto.MerchantTradeNo).FirstOrDefaultAsync();
                    if (logistics == null || logistics.FK_OhId != ohid || ResponseDto.MerchantTradeNo != RequestBody.MerchantTradeNo) throw new Exception("綠界建單回應與目前訂單不符，請先補查物流單");

                    await db.Entry(logistics).ReloadAsync();
                    var responseDate = ECPayLogisticsState.ParseDate(ResponseDto.UpdateStatusDate) ?? throw new Exception("建單回應時間格式錯誤");
                    if (!logistics.UpdateStatusDate.HasValue || responseDate >= logistics.UpdateStatusDate.Value)
                    {
                        logistics.LogisticsStatusCode = ResponseDto.RtnCode.ToString(CultureInfo.InvariantCulture);
                        logistics.UpdateStatusDate = responseDate;
                    }
                    logistics.AllPayLogisticsID = ResponseDto.AllPayLogisticsID;
                    logistics.CVSPaymentNo = ResponseDto.CVSPaymentNo;
                    logistics.CVSValidationNo = ResponseDto.CVSValidationNo;
                    logistics.BookingNote = ResponseDto.BookingNote;

                    await db.Entry(ohdata).ReloadAsync();
                    ECPayLogisticsState.SyncOrderState(ohdata, logistics.LogisticsStatusCode);
                    db.SaveChanges();
                    response.Success = true;

                    response.Message = $@"{ResponseDto.RtnMsg}<br>更新時間{ResponseDto.UpdateStatusDate}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ECPayLogisticsCreateCVSRequestDto> ECPayExpressCVSRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata)
        {
            ECPayLogisticsCreateCVSRequestDto RequestBody = new ECPayLogisticsCreateCVSRequestDto();

            try
            {
                var LogisticsSetting = await db.LogisticsSettings.Where(e => e.Id == ohdata.Shipping).FirstOrDefaultAsync();
                if (LogisticsSetting == null) throw new Exception("查無運費資訊");

                var logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohdata.Id && !e.IsDeleted).FirstOrDefaultAsync();
                if (logistics == null || logistics.LogisticsType != "CVS") throw new Exception("查無超商物流資訊");

                RequestBody = mapper.Map<ECPayLogisticsCreateCVSRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata, LogisticsSetting));

                RequestBody.CollectionAmount =
                    RequestBody.IsCollection == "Y"
                        ? RequestBody.GoodsAmount
                        : 0;

                RequestBody.ReceiverStoreID = logistics.CVSStoreID;
                RequestBody.ReturnStoreID = logistics.CVSStoreID;
                RequestBody.CheckMacValue = Encrypt(RequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayLogisticsCreateCVSRequestDto回傳資料：{ex.Message}");
                throw new Exception($"ECPayLogistics=>ECPayLogisticsCreateCVSRequestDto回傳資料：{ex.Message}");
            }
            return RequestBody;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsExpressHOMECreate(long ohid, HandleThirdPartyLogisticsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var RequestUri = $"{configuration["ThirdParty:ECPayLogistics:LogisticsUrl"]}/Express/Create";

                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && !e.IsDeleted && e.FK_WebsiteId == configuration.GetValue<long>("WebConfig:SiteId")).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");

                var logisticsData = await db.Order_Logistics.FirstOrDefaultAsync(e => e.FK_OhId == ohid);
                if (logisticsData == null || logisticsData.LogisticsType != "HOME") throw new Exception("查無宅配物流資訊");
                if (!string.IsNullOrEmpty(logisticsData.AllPayLogisticsID)) throw new Exception("此訂單已建立物流單，請勿重複建立");
                if (!Regex.IsMatch(dto.SenderZipCode ?? "", @"^\d{3,6}$") ||
                    string.IsNullOrWhiteSpace(dto.SenderAddress) || dto.SenderAddress.Trim().Length < 7 || dto.SenderAddress.Trim().Length > 60)
                    throw new Exception("請填寫有效的寄件郵遞區號與完整地址");
                if (!Regex.IsMatch(dto.SenderCellPhone ?? "", @"^09\d{8}$")) throw new Exception("請填寫有效的寄件人手機");
                if (!dto.GoodsWeight.HasValue || dto.GoodsWeight <= 0 || dto.GoodsWeight > 20 || decimal.Round(dto.GoodsWeight.Value, 3) != dto.GoodsWeight.Value)
                    throw new Exception("重量須大於 0 且不超過 20 公斤，最多小數 3 位");
                if (!new[] { "0001", "0002", "0003", "0004" }.Contains(dto.Specification)) throw new Exception("箱型規格錯誤");
                if (logisticsData.Temperature != "0001" && dto.Specification == "0004") throw new Exception("冷藏與冷凍不支援 150cm 規格");
                logisticsData.SenderName = ResolveEcpaySenderName(dto.SenderName);
                logisticsData.SenderCellPhone = dto.SenderCellPhone;
                logisticsData.SenderZipCode = dto.SenderZipCode;
                logisticsData.SenderAddress = dto.SenderAddress.Trim();
                logisticsData.GoodsWeight = dto.GoodsWeight;
                logisticsData.Specification = dto.Specification;
                await db.SaveChangesAsync();

                var RequestBody = await ECPayExpressHOMERequestBody(ThirdPartyData, ohdata);

                var RequestDict = RequestBody.GetType().GetProperties().ToDictionary(
                        p => p.Name,
                        p => p.GetValue(RequestBody)?.ToString() ?? ""
                    );

                var content = new FormUrlEncodedContent(RequestDict);

                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Clear();
                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                var parts = jsonResponse.Split('|', 2);
                var status = parts[0];
                var dataPart = parts.Length == 2 ? parts[1] : jsonResponse;

                if (status != "1") response.Message = dataPart;
                else
                {
                    var ResponseDict = ParseVerifiedResponse(dataPart, ThirdPartyData);
                    var json = JsonConvert.SerializeObject(ResponseDict);

                    var ResponseDto = JsonConvert.DeserializeObject<ECPayLogisticsCreateResponseDto>(json);

                    var logistics = await db.Order_Logistics.Where(e => e.MerchantTradeNo == ResponseDto.MerchantTradeNo).FirstOrDefaultAsync();
                    if (logistics == null || logistics.FK_OhId != ohid || ResponseDto.MerchantTradeNo != RequestBody.MerchantTradeNo) throw new Exception("綠界建單回應與目前訂單不符，請先補查物流單");

                    await db.Entry(logistics).ReloadAsync();
                    var responseDate = ECPayLogisticsState.ParseDate(ResponseDto.UpdateStatusDate) ?? throw new Exception("建單回應時間格式錯誤");
                    if (!logistics.UpdateStatusDate.HasValue || responseDate >= logistics.UpdateStatusDate.Value)
                    {
                        logistics.LogisticsStatusCode = ResponseDto.RtnCode.ToString(CultureInfo.InvariantCulture);
                        logistics.UpdateStatusDate = responseDate;
                    }
                    logistics.AllPayLogisticsID = ResponseDto.AllPayLogisticsID;
                    logistics.CVSPaymentNo = ResponseDto.CVSPaymentNo;
                    logistics.CVSValidationNo = ResponseDto.CVSValidationNo;
                    logistics.BookingNote = ResponseDto.BookingNote;

                    ECPayLogisticsState.SyncOrderState(ohdata, logistics.LogisticsStatusCode);
                    db.SaveChanges();

                    response.Success = true;
                    response.Message = $@"{ResponseDto.RtnMsg}<br>更新時間{ResponseDto.UpdateStatusDate}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ECPayLogisticsCreateHomeRequestDto> ECPayExpressHOMERequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata)
        {
            ECPayLogisticsCreateHomeRequestDto RequestBody = new ECPayLogisticsCreateHomeRequestDto();

            try
            {
                var LogisticsSetting = await db.LogisticsSettings.Where(e => e.Id == ohdata.Shipping).FirstOrDefaultAsync();
                if (LogisticsSetting == null) throw new Exception("查無運費資訊");

                var logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohdata.Id && !e.IsDeleted).FirstOrDefaultAsync();
                if (logistics == null) throw new Exception("查無物流資訊");

                RequestBody = mapper.Map<ECPayLogisticsCreateHomeRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata, LogisticsSetting));

                RequestBody.GoodsWeight = logistics.GoodsWeight ?? throw new Exception("請填寫商品重量");
                RequestBody.SenderName = logistics.SenderName;
                RequestBody.SenderPhone = logistics.SenderPhone ?? "";
                RequestBody.SenderCellPhone = logistics.SenderCellPhone;
                RequestBody.SenderZipCode = logistics.SenderZipCode;
                RequestBody.SenderAddress = logistics.SenderAddress;
                RequestBody.ReceiverZipCode = ohdata.RecipientZipCode;
                RequestBody.ReceiverAddress = ohdata.RecipientAddress?.Replace(" ", "");
                RequestBody.Temperature = logistics.Temperature;
                RequestBody.Specification = logistics.Specification;
                if (RequestBody.LogisticsSubType == "POST" && RequestBody.IsCollection == "Y")
                    throw new Exception("中華郵政不支援貨到付款");
                RequestBody.CheckMacValue = Encrypt(RequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayLogisticsCreateCVSRequestDto回傳資料：{ex.Message}");
                throw new Exception($"ECPayLogistics=>ECPayLogisticsCreateCVSRequestDto回傳資料：{ex.Message}");
            }
            return RequestBody;
        }
        private async Task<ECPayLogisticsCreateRequestDto> ECPayExpressRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, Core.Models.LogisticsSetting LogisticsSetting)
        {
            ECPayLogisticsCreateRequestDto RequestBody = new ECPayLogisticsCreateRequestDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var DateTimeNow = DateTime.Now;
            var BackstageUrl = configuration.GetValue<string>("WebConfig:BackstageUrl");

            try
            {
                var user = await db.FrontUsers.Where(e => e.UUID == ohdata.FK_UUID).FirstOrDefaultAsync();
                var logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohdata.Id && !e.IsDeleted).FirstOrDefaultAsync();
                if (logistics == null) throw new Exception("查無物流資訊");
                if (!string.IsNullOrEmpty(logistics.AllPayLogisticsID)) throw new Exception("此訂單已建立物流單，請先查詢物流狀態");
                if (ohdata.State != OrderStatusEnum.待確認 && ohdata.State != OrderStatusEnum.已付款 && ohdata.State != OrderStatusEnum.待付款) throw new Exception("目前訂單狀態不允許建立物流單");

                RequestBody.MerchantID = ThirdPartyData.MerchantID;
                var MerchantTradeNo = logistics.MerchantTradeNo ?? ohdata.Id.ToString("D9", CultureInfo.InvariantCulture);
                RequestBody.MerchantTradeNo = MerchantTradeNo;
                logistics.MerchantTradeNo = MerchantTradeNo;
                db.SaveChanges();
                RequestBody.MerchantTradeDate = DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");

                var LogisticsType = LogisticsSetting.LogisticsType;
                var isCashOnDelivery = await db.PaymentTypes
                    .AnyAsync(x =>
                        x.Id == ohdata.Payment &&
                        x.Code == "COD");
                RequestBody.IsCollection = isCashOnDelivery ? "Y" : "N";

                switch ((int)LogisticsType)
                {
                    case 8:
                        RequestBody.LogisticsSubType = "FAMI";
                        break;
                    case 9:
                        RequestBody.LogisticsSubType = "UNIMART";
                        break;
                    case 10:
                        RequestBody.LogisticsSubType = "UNIMARTFREEZE";
                        break;
                    case 11:
                        RequestBody.LogisticsSubType = "HILIFE";
                        break;
                    case 12:
                        RequestBody.LogisticsSubType = "FAMIC2C";
                        break;
                    case 13:
                        RequestBody.LogisticsSubType = "UNIMARTC2C";
                        break;
                    case 14:
                        RequestBody.LogisticsSubType = "HILIFEC2C";
                        break;
                    case 15:
                        RequestBody.LogisticsSubType = "OKMARTC2C";
                        break;
                    case 16:
                    case 18:
                    case 19:
                        RequestBody.LogisticsSubType = "TCAT";
                        break;
                    case 17:
                        RequestBody.LogisticsSubType = "POST";
                        break;
                }
                var paymentResult = await orderAppService.GetForPaymentAsync(ohdata.Id);
                if (!paymentResult.Success)
                    throw new Exception(paymentResult.Message ?? "取得付款資料失敗");

                var payData = paymentResult.Object as PayOrderData;
                if (payData == null)
                    throw new Exception("付款資料格式錯誤");

                RequestBody.GoodsAmount = payData.PayableAmount;

                var itemlist = "";
                var prod_titles = (await orderAppService.GetOrderDetails(ohdata.Id)).Select(e => e.Title);
                prod_titles = prod_titles.Distinct();
                foreach (var prod_title in prod_titles)
                {
                    var title = prod_title.Length > 50 ? $"{prod_title.Substring(0, 47)}..." : prod_title;
                    if (string.IsNullOrEmpty(itemlist)) itemlist = title;
                    else
                    {
                        title = $"#{title}";
                        if (itemlist.Length + title.Length > 44)
                        {
                            itemlist += "#其他...";
                            break;
                        }
                        itemlist += title;
                    }
                }

                RequestBody.GoodsName = NormalizeEcpayGoodsName(itemlist);
                RequestBody.SenderName = ResolveEcpaySenderName(ohdata.Orderer);
                RequestBody.SenderPhone = ohdata.OrdererTelePhone;
                RequestBody.SenderCellPhone = ohdata.OrdererCellPhone;
                RequestBody.ReceiverName = ResolveEcpaySenderName(ohdata.Recipient);
                RequestBody.ReceiverPhone = ohdata.RecipientTelePhone;
                RequestBody.ReceiverCellPhone = ohdata.RecipientCellPhone;
                RequestBody.ReceiverEmail = ohdata.RecipientEmail;

                RequestBody.TradeDesc = $"{Website.Title}-商品購買交易";

                RequestBody.ServerReplyURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayLogisticsExpressCreateResponse";
                RequestBody.ClientReplyURL = "";

                RequestBody.Remark = ohdata.Remark;
                RequestBody.PlatformID = ThirdPartyData.PlatformID;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayLogisticsCreateRequestDto回傳資料：{ex.Message}");
                throw new Exception($"ECPayLogistics=>ECPayLogisticsCreateRequestDto回傳資料：{ex.Message}");
            }
            return RequestBody;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsExpressCreateResponse(ECPayLogisticsCallbackDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var logistics = await db.Order_Logistics.FirstOrDefaultAsync(e => !e.IsDeleted && e.MerchantTradeNo == dto.MerchantTradeNo);
                if (logistics == null) throw new Exception("查無物流資訊");
                var order = await db.Order_Headers.FirstOrDefaultAsync(e => e.Id == logistics.FK_OhId && !e.IsDeleted && e.FK_WebsiteId == websiteId);
                if (order == null) throw new Exception("查無本站訂單");
                if (!string.IsNullOrEmpty(logistics.AllPayLogisticsID) && logistics.AllPayLogisticsID != dto.AllPayLogisticsID)
                    throw new Exception("物流交易編號不符");
                var updatedAt = ECPayLogisticsState.ParseDate(dto.UpdateStatusDate) ?? throw new Exception("物流通知時間格式錯誤");
                // Retries and out-of-order callbacks must not move a shipment back to an older status.
                if (!logistics.UpdateStatusDate.HasValue || updatedAt >= logistics.UpdateStatusDate.Value)
                {
                    logistics.LogisticsStatusCode = dto.RtnCode.ToString(CultureInfo.InvariantCulture);
                    logistics.UpdateStatusDate = updatedAt;
                    if (!string.IsNullOrEmpty(dto.AllPayLogisticsID)) logistics.AllPayLogisticsID = dto.AllPayLogisticsID;
                    if (!string.IsNullOrEmpty(dto.BookingNote)) logistics.BookingNote = dto.BookingNote;
                    if (!string.IsNullOrEmpty(dto.CVSPaymentNo)) logistics.CVSPaymentNo = dto.CVSPaymentNo;
                    if (!string.IsNullOrEmpty(dto.CVSValidationNo)) logistics.CVSValidationNo = dto.CVSValidationNo;
                    ECPayLogisticsState.SyncOrderState(order, logistics.LogisticsStatusCode);
                    await db.SaveChangesAsync();
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsPrintOrderInfo(ECPayLogisticsPrintOrderInfoEnum type, long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();

                if (ThirdPartyData.MerchantID != null)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && !e.IsDeleted && e.FK_WebsiteId == configuration.GetValue<long>("WebConfig:SiteId")).FirstOrDefaultAsync();
                    if (ohdata == null) throw new Exception($"查無訂單資訊");
                    var logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohdata.Id && !e.IsDeleted).FirstOrDefaultAsync();
                    if (logistics == null) throw new Exception($"查無訂單物流資訊");

                    switch (type)
                    {
                        case ECPayLogisticsPrintOrderInfoEnum.FAMI:
                        case ECPayLogisticsPrintOrderInfoEnum.HILIFE:
                        case ECPayLogisticsPrintOrderInfoEnum.OKMART:
                            var RequestBody_C2C = new ECPayLogisticsPrintShippingLabelC2CRequestDto()
                            {
                                MerchantID = ThirdPartyData.MerchantID,
                                AllPayLogisticsID = logistics.AllPayLogisticsID,
                                CVSPaymentNo = logistics.CVSPaymentNo,
                                PlatformID = ThirdPartyData.PlatformID,
                            };

                            RequestBody_C2C.CheckMacValue = Encrypt(RequestBody_C2C, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                            response.Success = true;
                            response.Message = JsonConvert.SerializeObject(RequestBody_C2C);
                            break;
                        case ECPayLogisticsPrintOrderInfoEnum.UniMart:
                            var RequestBody_711 = new ECPayLogisticsPrintShippingLabelC2C711RequestDto()
                            {
                                MerchantID = ThirdPartyData.MerchantID,
                                AllPayLogisticsID = logistics.AllPayLogisticsID,
                                CVSPaymentNo = logistics.CVSPaymentNo,
                                CVSValidationNo = logistics.CVSValidationNo,
                                PlatformID = ThirdPartyData.PlatformID,
                            };

                            RequestBody_711.CheckMacValue = Encrypt(RequestBody_711, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                            response.Success = true;
                            response.Message = JsonConvert.SerializeObject(RequestBody_711);
                            break;
                        case ECPayLogisticsPrintOrderInfoEnum.B2C:
                            var RequestBody_B2C = new ECPayLogisticsPrintShippingLabelB2CRequestDto()
                            {
                                MerchantID = ThirdPartyData.MerchantID,
                                AllPayLogisticsID = logistics.AllPayLogisticsID,
                                PlatformID = ThirdPartyData.PlatformID,
                                PrintMode = 1,
                            };

                            RequestBody_B2C.CheckMacValue = Encrypt(RequestBody_B2C, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                            response.Success = true;
                            response.Message = JsonConvert.SerializeObject(RequestBody_B2C);
                            break;
                    }
                    return response;
                }
                else throw new Exception("商家未確實設置綠界物流資料");
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
        public async Task<ResponseMessageDto> ECPayLogisticsTradeInfo(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var RequestUri = $"{configuration["ThirdParty:ECPayLogistics:LogisticsUrl"]}/Helper/QueryLogisticsTradeInfo/V5";

                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && !e.IsDeleted && e.FK_WebsiteId == configuration.GetValue<long>("WebConfig:SiteId")).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");
                var order_logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohid && !e.IsDeleted).FirstOrDefaultAsync();
                if (order_logistics == null) throw new Exception("查無訂單物流資訊");

                var DateTimeNow = DateTime.Now;

                var queriedStatus = order_logistics.LogisticsStatusCode;
                var queriedUpdateDate = order_logistics.UpdateStatusDate;
                var hasLogisticsId = !string.IsNullOrEmpty(order_logistics.AllPayLogisticsID);

                var RequestBody = new ECPayLogisticsTradeInfoRequestDto()
                {
                    MerchantID = ThirdPartyData.MerchantID,
                    AllPayLogisticsID = hasLogisticsId ? order_logistics.AllPayLogisticsID : "",
                    MerchantTradeNo = hasLogisticsId ? "" : order_logistics.MerchantTradeNo,
                    TimeStamp = (int)((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds(),
                    PlatformID = ThirdPartyData.PlatformID ?? "",
                };

                RequestBody.CheckMacValue = Encrypt(RequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                var RequestDict = RequestBody.GetType().GetProperties().ToDictionary(
                        p => p.Name,
                        p => p.GetValue(RequestBody)?.ToString() ?? ""
                    );

                var content = new FormUrlEncodedContent(RequestDict);

                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Clear();
                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                if (jsonResponse.StartsWith("0|")) response.Message = jsonResponse.Substring(2);
                else
                {
                    var raw = ParseVerifiedResponse(jsonResponse, ThirdPartyData);
                    var values = raw.ToDictionary(x => x.Key, x => x.Value == "null" ? null : x.Value);
                    var ResponseDto = JsonConvert.DeserializeObject<ECPayLogisticsTradeInfoResponseDto>(JsonConvert.SerializeObject(values))
                        ?? throw new Exception("查詢物流回應格式錯誤");
                    if (ResponseDto.MerchantTradeNo != order_logistics.MerchantTradeNo ||
                        (hasLogisticsId && ResponseDto.AllPayLogisticsID != order_logistics.AllPayLogisticsID))
                        throw new Exception("查詢物流回應與目前訂單不符");
                    if (string.IsNullOrWhiteSpace(ResponseDto.AllPayLogisticsID)) throw new Exception("查詢回應缺少物流交易編號");
                    await db.Entry(order_logistics).ReloadAsync();
                    order_logistics.AllPayLogisticsID = ResponseDto.AllPayLogisticsID;
                    if (!string.IsNullOrEmpty(ResponseDto.BookingNote)) order_logistics.BookingNote = ResponseDto.BookingNote;
                    if (!string.IsNullOrEmpty(ResponseDto.CVSPaymentNo)) order_logistics.CVSPaymentNo = ResponseDto.CVSPaymentNo;
                    if (!string.IsNullOrEmpty(ResponseDto.CVSValidationNo)) order_logistics.CVSValidationNo = ResponseDto.CVSValidationNo;
                    if (order_logistics.UpdateStatusDate == queriedUpdateDate && order_logistics.LogisticsStatusCode == queriedStatus)
                        order_logistics.LogisticsStatusCode = ResponseDto.LogisticsStatus;
                    // Query results have no status timestamp; retain the last callback timestamp.
                    await db.Entry(ohdata).ReloadAsync();
                    if (string.IsNullOrWhiteSpace(ohdata.TrackingNumber))
                        ohdata.TrackingNumber = !string.IsNullOrEmpty(ResponseDto.ShipmentNo) ? ResponseDto.ShipmentNo : ECPayLogisticsState.TrackingNumber(order_logistics, null);
                    ECPayLogisticsState.SyncOrderState(ohdata, order_logistics.LogisticsStatusCode);
                    await db.SaveChangesAsync();
                    response.Success = true;
                    response.Message = order_logistics.LogisticsStatusCode + "|" + ECPayLogisticsState.StatusText(order_logistics.LogisticsStatusCode);
                    response.Object = new
                    {
                        order_logistics.AllPayLogisticsID, order_logistics.BookingNote,
                        TrackingNumber = ECPayLogisticsState.TrackingNumber(order_logistics, ohdata.TrackingNumber),
                        LogisticsStatusStr = ECPayLogisticsState.StatusText(order_logistics.LogisticsStatusCode)
                    };

                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsCVSReturn(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && !e.IsDeleted && e.FK_WebsiteId == configuration.GetValue<long>("WebConfig:SiteId")).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");
                var order_logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohid && !e.IsDeleted).FirstOrDefaultAsync();
                if (order_logistics == null) throw new Exception("查無訂單物流資訊");

                if (!string.IsNullOrEmpty(order_logistics.RtnMerchantTradeNo) || !string.IsNullOrEmpty(order_logistics.RtnOrderNo)) throw new Exception("此訂單已申請退貨");
                if (string.IsNullOrEmpty(order_logistics.AllPayLogisticsID)) throw new Exception("請先建立物流單");
                if (ohdata.State != OrderStatusEnum.已出貨 && ohdata.State != OrderStatusEnum.已完成) throw new Exception("目前訂單狀態不允許退貨");
                if (!new[] { "已取件", "已配達" }.Contains(ECPayLogisticsState.StatusText(order_logistics.LogisticsStatusCode))) throw new Exception("物流尚未完成配達或取件，請先查詢物流狀態");
                var RequestBody = new ECPayLogisticsReturnRequestDto()
                {
                    MerchantID = ThirdPartyData.MerchantID,
                    AllPayLogisticsID = order_logistics.AllPayLogisticsID,
                    ServerReplyURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayLogisticsReturnResponse",
                    CollectionAmount = 0,
                    ServiceType="4",
                    Remark="",
                    PlatformID = ThirdPartyData.PlatformID ?? "",
                };

                var itemlist = "";
                var prod_titles = (await orderAppService.GetOrderDetails(ohdata.Id)).Select(e => e.Title);
                prod_titles = prod_titles.Distinct();
                foreach (var prod_title in prod_titles)
                {
                    var title = prod_title.Length > 50 ? $"{prod_title.Substring(0, 47)}..." : prod_title;
                    if (string.IsNullOrEmpty(itemlist)) itemlist = title;
                    else
                    {
                        title = $"#{title}";
                        if (itemlist.Length + title.Length > 44)
                        {
                            itemlist += "#其他...";
                            break;
                        }
                        itemlist += title;
                    }
                }

                RequestBody.GoodsName = NormalizeEcpayGoodsName(itemlist);
                RequestBody.GoodsAmount = (int)(ohdata.Subtotal + ohdata.Discount + ohdata.Bonus + ohdata.Freight);

                RequestBody.SenderName = ResolveEcpaySenderName(ohdata.Recipient);
                RequestBody.SenderPhone = ohdata.RecipientCellPhone;

                var RequestUri = $"{configuration["ThirdParty:ECPayLogistics:LogisticsUrl"]}/express";

                FormUrlEncodedContent content;

                switch (order_logistics.LogisticsSubType)
                {
                    case "FAMI":
                        RequestUri += "/ReturnCVS";
                        
                        var FAMIRequestBody = mapper.Map<ECPayLogisticsReturnFAMIRequestDto>(RequestBody);
                        FAMIRequestBody.Quantity = (await orderAppService.GetOrderDetails(ohdata.Id)).Sum(e => e.Quantity).ToString();
                        FAMIRequestBody.Cost = FAMIRequestBody.GoodsAmount.ToString();

                        FAMIRequestBody.CheckMacValue = Encrypt(FAMIRequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                        var FAMIRequestDict = FAMIRequestBody.GetType().GetProperties().ToDictionary(
                                p => p.Name,
                                p => p.GetValue(FAMIRequestBody)?.ToString() ?? ""
                            );

                        content = new FormUrlEncodedContent(FAMIRequestDict);

                        break;
                    case "UNIMARTFREEZE":
                    case "UNIMART":
                    case "HILIFE":
                        if (order_logistics.LogisticsSubType == "UNIMARTFREEZE" || order_logistics.LogisticsSubType == "UNIMART") RequestUri += "/ReturnUniMartCVS";
                        else if (order_logistics.LogisticsSubType == "HILIFE") RequestUri += "/ReturnHilifeCVS";

                        RequestBody.CheckMacValue = Encrypt(RequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                        var RequestDict = RequestBody.GetType().GetProperties().ToDictionary(
                                p => p.Name,
                                p => p.GetValue(RequestBody)?.ToString() ?? ""
                            );

                        content = new FormUrlEncodedContent(RequestDict);
                        break;
                    default:
                        throw new ArgumentException($"不支援的物流類型");
                }

                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Clear();
                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();

                if (jsonResponse.StartsWith("|"))
                {
                    var errorMessage = jsonResponse.Substring(1);
                    response.Message = errorMessage;
                }
                else
                {
                    var values = jsonResponse.Split('|');

                    if (values.Length >= 2)
                    {
                        order_logistics.RtnMerchantTradeNo = values[0];
                        order_logistics.RtnOrderNo = values[1];
                        db.SaveChanges();
                        response.Success = true;
                        response.Message = $"退貨編號：{values[1]}";
                    }
                    else response.Message = $"綠界回傳格式錯誤：{jsonResponse}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsHOMEReturn(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid && !e.IsDeleted && e.FK_WebsiteId == configuration.GetValue<long>("WebConfig:SiteId")).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");
                var order_logistics = await db.Order_Logistics.Where(e => e.FK_OhId == ohid && !e.IsDeleted).FirstOrDefaultAsync();
                if (order_logistics == null) throw new Exception("查無訂單物流資訊");

                if (order_logistics.LogisticsType != "HOME" || order_logistics.LogisticsSubType != "TCAT")
                    throw new Exception("僅黑貓宅配支援此退貨流程");
                if (string.IsNullOrEmpty(order_logistics.AllPayLogisticsID)) throw new Exception("請先建立物流訂單");
                if (!string.IsNullOrEmpty(order_logistics.RtnMerchantTradeNo)) throw new Exception("此訂單已申請退貨");
                var correlation = Encrypt(new { OrderId = ohid }, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
                if (ohdata.State != OrderStatusEnum.已出貨 && ohdata.State != OrderStatusEnum.已完成) throw new Exception("目前訂單狀態不允許退貨");
                if (!new[] { "已取件", "已配達" }.Contains(ECPayLogisticsState.StatusText(order_logistics.LogisticsStatusCode))) throw new Exception("物流尚未完成配達或取件，請先查詢物流狀態");
                var RequestBody = new ECPayLogisticsReturnRequestDto()
                {
                    MerchantID = ThirdPartyData.MerchantID,
                    AllPayLogisticsID = order_logistics.AllPayLogisticsID,
                    ServerReplyURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayLogisticsReturnResponse?orderId={ohid}&correlation={correlation}",
                    Remark = "",
                    PlatformID = ThirdPartyData.PlatformID ?? "",
                };

                var itemlist = "";
                var prod_titles = (await orderAppService.GetOrderDetails(ohdata.Id)).Select(e => e.Title);
                prod_titles = prod_titles.Distinct();
                foreach (var prod_title in prod_titles)
                {
                    var title = prod_title.Length > 50 ? $"{prod_title.Substring(0, 47)}..." : prod_title;
                    if (string.IsNullOrEmpty(itemlist)) itemlist = title;
                    else
                    {
                        title = $"#{title}";
                        if (itemlist.Length + title.Length > 44)
                        {
                            itemlist += "#其他...";
                            break;
                        }
                        itemlist += title;
                    }
                }

                RequestBody.GoodsName = NormalizeEcpayGoodsName(itemlist);
                var payment = await orderAppService.GetForPaymentAsync(ohid);
                if (!payment.Success || payment.Object is not PayOrderData payData) throw new Exception("取得訂單金額失敗");
                RequestBody.GoodsAmount = payData.PayableAmount;
                if (RequestBody.GoodsAmount < 1 || RequestBody.GoodsAmount > 20000) throw new Exception("黑貓退貨商品金額須為 1～20000 元");

                // 原宅配單的寄收件資料由綠界自動反轉。

                var RequestUri = $"{configuration["ThirdParty:ECPayLogistics:LogisticsUrl"]}/express";

                FormUrlEncodedContent content;

                RequestUri += "/ReturnHome";

                var HomeRequestBody = mapper.Map<ECPayLogisticsReturnHomeDeliveryRequestDto>(RequestBody);
                HomeRequestBody.SenderName = "";
                HomeRequestBody.SenderPhone = "";
                HomeRequestBody.SenderCellPhone = "";
                HomeRequestBody.SenderZipCode = "";
                HomeRequestBody.SenderAddress = "";
                HomeRequestBody.ReceiverName = "";
                HomeRequestBody.ReceiverPhone = "";
                HomeRequestBody.ReceiverCellPhone = "";
                HomeRequestBody.ReceiverZipCode = "";
                HomeRequestBody.ReceiverAddress = "";
                HomeRequestBody.ReceiverEmail = "";
                HomeRequestBody.Temperature = order_logistics.Temperature;
                HomeRequestBody.Specification = order_logistics.Specification;
                HomeRequestBody.Distance = "00";

                HomeRequestBody.CheckMacValue = Encrypt(HomeRequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);

                var RequestDict = HomeRequestBody.GetType().GetProperties().ToDictionary(
                        p => p.Name,
                        p => p.GetValue(HomeRequestBody)?.ToString() ?? ""
                    );

                content = new FormUrlEncodedContent(RequestDict);

                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Clear();
                ThirdPartyClient_ECPayLogistics.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();

                if (jsonResponse == "1|OK")
                {
                    await db.Entry(order_logistics).ReloadAsync();
                    if (string.IsNullOrEmpty(order_logistics.RtnMerchantTradeNo)) order_logistics.RtnMerchantTradeNo = $"HOME{ohid}";
                    await db.SaveChangesAsync();
                    response.Success = true;
                    response.Message = "已產生逆物流訂單，宅配業者會至退貨地址收件";
                }
                else
                {
                    var errorMessage = jsonResponse.Substring(1);
                    response.Message = errorMessage;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsReturnResponse(ECPayLogisticsCallbackReverseDto dto, long? orderId = null, string? correlation = null)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

            try
            {
                Core.Models.Order_Logistics? logistics;
                if (orderId.HasValue)
                {
                    var keys = await ECPayGetThirdPartyData();
                    if (string.IsNullOrEmpty(correlation) || correlation != Encrypt(new { OrderId = orderId.Value }, keys.HashKey, keys.HashIV))
                        throw new Exception("退貨回呼關聯驗證失敗");
                    logistics = await db.Order_Logistics.FirstOrDefaultAsync(e => e.FK_OhId == orderId.Value && e.LogisticsType == "HOME");
                }
                else logistics = await db.Order_Logistics.FirstOrDefaultAsync(e => e.RtnMerchantTradeNo == dto.RtnMerchantTradeNo);
                if (logistics == null) throw new Exception("查無逆物流資訊");
                var ohdata = await db.Order_Headers.Where(e => e.Id == logistics.FK_OhId).FirstOrDefaultAsync();
                if (ohdata == null || ohdata.IsDeleted || ohdata.FK_WebsiteId != WebsiteId || logistics.IsDeleted) throw new Exception("查無本站訂單");
                if (string.IsNullOrWhiteSpace(dto.RtnMerchantTradeNo)) throw new Exception("逆物流交易編號不可為空");
                var updatedAt = ECPayLogisticsState.ParseDate(dto.UpdateStatusDate) ?? throw new Exception("退貨通知時間格式錯誤");
                var previous = await ECPayLogisticsState.GetReturnAsync(db, WebsiteId, logistics);
                var previousDate = ECPayLogisticsState.ParseDate(previous?.UpdateStatusDate);
                if (previousDate.HasValue && updatedAt <= previousDate.Value)
                {
                    response.Success = true;
                    return response;
                }
                logistics.RtnMerchantTradeNo = dto.RtnMerchantTradeNo;
                db.AuditLogs.Add(new EtheriT.Coker.Core.Models.AuditLog
                {
                    FK_WebsiteId = WebsiteId, ExecutionTime = DateTime.Now,
                    ServiceName = ECPayLogisticsState.ReturnService, MethodName = nameof(ECPayLogisticsReturnResponse),
                    Parameters = ECPayLogisticsState.ReturnKey(logistics), ReturnValue = JsonConvert.SerializeObject(dto)
                });
                await db.SaveChangesAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayLogisticsExpressCreateResponse回傳資料：{ex.Message}");
                response.Message = $"ECPayLogistics=>ECPayLogisticsExpressCreateResponse回傳資料：{ex.Message}";
                await loginUserData.SetLogs(0, WebsiteId, $"ECPayLogisticsExpressCreateError", ex.Message);
            }
            return response;
        }
        private async Task<ECPayThirdPartyDataDto> ECPayGetThirdPartyData()
        {
            ECPayThirdPartyDataDto ThirdPartyData = new ECPayThirdPartyDataDto();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                     join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                     join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                     where tp.Title == "綠界物流"
                                                     where tpkv.FK_WebsiteId == WebsiteId
                                                     select new KeyValueDto() { Key = tpk.Code, Value = tpkv.Value }).ToListAsync();

                if (!thirdPartyKeypairValues.Any()) throw new Exception("商家未確實設置綠界物流資料");

                var thirdPartyDict = thirdPartyKeypairValues.ToDictionary(e => e.Key, e => e.Value);

                ThirdPartyData.MerchantID = thirdPartyDict.GetValueOrDefault("MerchantID") ?? throw new Exception("商家未確實設置綠界物流資料");
                ThirdPartyData.HashKey = thirdPartyDict.GetValueOrDefault("HashKey") ?? throw new Exception("商家未確實設置綠界物流資料");
                ThirdPartyData.HashIV = thirdPartyDict.GetValueOrDefault("HashIV") ?? throw new Exception("商家未確實設置綠界物流資料");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayGetThirdPartyData回傳資料：{ex.Message}");
            }
            return ThirdPartyData;
        }
        public async Task<bool> VerifyLogisticsCallback(Dictionary<string, string> data)
        {
            var keys = await ECPayGetThirdPartyData();
            return !string.IsNullOrEmpty(keys.HashKey) && !string.IsNullOrEmpty(keys.HashIV) &&
                data.TryGetValue("MerchantID", out var merchant) && merchant == keys.MerchantID &&
                data.TryGetValue("CheckMacValue", out var checksum) && !string.IsNullOrEmpty(checksum) &&
                string.Equals(checksum, Encrypt(data, keys.HashKey, keys.HashIV), StringComparison.OrdinalIgnoreCase);
        }
        private Dictionary<string, string> ParseVerifiedResponse(string body, ECPayThirdPartyDataDto keys)
        {
            var parsed = HttpUtility.ParseQueryString(body);
            if (parsed.AllKeys.Any(k => k == null) || parsed.AllKeys.Any(k => parsed.GetValues(k!)?.Length != 1))
                throw new Exception("綠界回應格式錯誤，請先補查物流單");
            var values = parsed.AllKeys.ToDictionary(k => k!, k => parsed[k!] ?? "");
            if (!values.TryGetValue("MerchantID", out var merchant) || merchant != keys.MerchantID ||
                !values.TryGetValue("CheckMacValue", out var checksum) || string.IsNullOrWhiteSpace(checksum) ||
                !string.Equals(checksum, Encrypt(values, keys.HashKey, keys.HashIV), StringComparison.OrdinalIgnoreCase))
                throw new Exception("綠界回應檢查碼驗證失敗，請先補查物流單");
            return values;
        }
        private string Encrypt(object data, string hashKey, string hashIV)
        {
            var jObject = JObject.FromObject(data);
            jObject.Remove("CheckMacValue");
            var sorted = jObject.Properties().OrderBy(p => p.Name);
            string jsonData = string.Join("&", sorted.Select(p => $"{p.Name}={p.Value}"));
            jsonData = $"HashKey={hashKey}&{jsonData}&HashIV={hashIV}";
            string urlEncodedData = HttpUtility.UrlEncode(jsonData).ToLower();
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(urlEncodedData));
            string checkMacValue = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

            return checkMacValue;
        }
        private string ResolveEcpaySenderName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("姓名不可為空");

            input = input.Trim();

            bool isAllChinese = input.All(c => c >= '\u4e00' && c <= '\u9fff');

            bool isAllEnglish = input.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));

            string cn = null;
            string en = null;

            if (isAllChinese)
            {
                cn = input;
                if (cn.Length < 2) throw new Exception("中文姓名至少 2 字");
                if (cn.Length > 5) cn = cn.Substring(0, 5);
                return cn;
            }

            if (isAllEnglish)
            {
                en = input;
                if (en.Length < 4) throw new Exception("英文姓名至少 4 個字母");
                if (en.Length > 10) en = en.Substring(0, 10);
                return en;
            }

            var match = System.Text.RegularExpressions.Regex.Match(input, @"^(?<cn>[\u4e00-\u9fff]{2,5})(?:[\(（](?<en>[A-Za-z]{4,10})[\)）])?$");

            if (match.Success)
            {
                cn = match.Groups["cn"].Value;
                en = match.Groups["en"].Success ? match.Groups["en"].Value : null;
                return cn;
            }

            cn = new string(input.Where(c => c >= '\u4e00' && c <= '\u9fff').ToArray());
            if (cn.Length >= 2) return cn.Substring(0, Math.Min(5, cn.Length));
            en = new string(input.Where(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')).ToArray());
            if (en.Length >= 4) return en.Substring(0, Math.Min(10, en.Length));

            throw new Exception("姓名格式不符合綠界規則");
        }
        private string NormalizeEcpayGoodsName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "商品";
            input = input.Trim();
            input = Regex.Replace(input, @"[^\u4e00-\u9fffA-Za-z0-9()\s/\-]", "");
            input = Regex.Replace(input, @"[^\p{IsCJKUnifiedIdeographs}A-Za-z0-9()\s/\-]", "");
            StringBuilder sb = new StringBuilder();
            int totalBytes = 0;

            foreach (char c in input)
            {
                int charBytes = IsFullWidth(c) ? 2 : 1;
                if (totalBytes + charBytes > 40) break;
                sb.Append(c);
                totalBytes += charBytes;
            }

            var result = sb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(result)) return "商品";
            return result;
        }
        private bool IsFullWidth(char c)
        {
            return c >= 0x4E00 && c <= 0x9FFF;
        }
    }
}
