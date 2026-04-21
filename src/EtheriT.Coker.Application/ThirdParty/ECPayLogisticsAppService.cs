using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
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

                if (string.IsNullOrWhiteSpace(ThirdPartyData?.MerchantID)) throw new Exception("商家未確實設置綠界物流資料");

                RequestBody.MerchantID = ThirdPartyData.MerchantID;
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
        public async Task<ECPayLogisticsCreateCVSRequestDto> ECPayLogisticsExpressCVSCreate(long ohid)
        {
            ECPayLogisticsCreateCVSRequestDto RequestBody = new ECPayLogisticsCreateCVSRequestDto();
            try
            {
                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");

                return await ECPayExpressCVSRequestBody(ThirdPartyData, ohdata);
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
        private async Task<ECPayLogisticsCreateCVSRequestDto> ECPayExpressCVSRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata)
        {
            ECPayLogisticsCreateCVSRequestDto RequestBody = new ECPayLogisticsCreateCVSRequestDto();

            try
            {
                RequestBody = mapper.Map<ECPayLogisticsCreateCVSRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata));

                var db_logistics_setting = await db.LogisticsSettings.Where(e => e.Id == ohdata.Shipping).FirstOrDefaultAsync();
                if (db_logistics_setting == null) throw new Exception("查無運費資訊");

                if (db_logistics_setting.SupportCashOnDelivery) RequestBody.CollectionAmount = RequestBody.GoodsAmount;
                else RequestBody.CollectionAmount = 0;

                RequestBody.ReceiverStoreID = ohdata.CVSStoreID;
                RequestBody.ReturnStoreID = ohdata.CVSStoreID;
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
        private async Task<ECPayLogisticsCreateRequestDto> ECPayExpressRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata)
        {
            ECPayLogisticsCreateRequestDto RequestBody = new ECPayLogisticsCreateRequestDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var DateTimeNow = DateTime.Now;
            var BackstageUrl = configuration.GetValue<string>("WebConfig:BackstageUrl");

            try
            {
                var user = await db.FrontUsers.Where(e => e.UUID == ohdata.FK_UUID).FirstOrDefaultAsync();

                RequestBody.MerchantID = ThirdPartyData.MerchantID;
                var MerchantTradeNo = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                RequestBody.MerchantTradeNo = MerchantTradeNo;
                ohdata.MerchantTradeNo = MerchantTradeNo;
                db.SaveChanges();
                RequestBody.MerchantTradeDate = DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");

                var LogisticsSetting = await db.LogisticsSettings.Where(e => e.Id == ohdata.Shipping).FirstOrDefaultAsync();
                if (LogisticsSetting == null) throw new Exception("查無運費設置");
                var LogisticsType = LogisticsSetting.LogisticsType;
                RequestBody.IsCollection = LogisticsSetting.SupportCashOnDelivery ? "Y" : "N";

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
                        RequestBody.LogisticsSubType = "TCAT";
                        break;
                    case 17:
                        RequestBody.LogisticsSubType = "POST";
                        break;
                }
                RequestBody.GoodsAmount = (int)(ohdata.Subtotal + ohdata.Freight);

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
        public async Task<ResponseMessageDto> ECPayLogisticsExpressCreateResponse(Dictionary<string, string> ResultResponseData)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

            try
            {
                await loginUserData.SetLogs(0, WebsiteId, $"ECPayLogisticsExpressCreateResponse", JsonConvert.SerializeObject(ResultResponseData));

                var ohdata = await db.Order_Headers.Where(e => e.MerchantTradeNo == ResultResponseData["MerchantTradeNo"]).FirstOrDefaultAsync();

                if (ohdata == null) throw new Exception("查無訂單資訊");

                ohdata.LogisticsStatusCode = ResultResponseData["RtnCode"];
                ohdata.AllPayLogisticsID = ResultResponseData["AllPayLogisticsID"];

                if (DateTime.TryParseExact(ResultResponseData["UpdateStatusDate"], "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updateDate)) ohdata.LogisticsUpdateStatusDate = updateDate;
                ohdata.CVSPaymentNo = ResultResponseData["CVSPaymentNo"];
                ohdata.CVSValidationNo = ResultResponseData["CVSValidationNo"];
                ohdata.BookingNote = ResultResponseData["BookingNote"];

                db.SaveChanges();

                response.Success = true;
                response.Message = $"/OrderManagement";
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

        public async Task<ResponseMessageDto> ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum type, long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();

                if (ThirdPartyData.MerchantID != null)
                {
                    var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                    if (ohdata == null) throw new Exception($"查無訂單資訊");

                    switch (type)
                    {
                        case ECPayLogisticsPrintOrderInfoEnum.FAMI:
                        case ECPayLogisticsPrintOrderInfoEnum.HILIFE:
                        case ECPayLogisticsPrintOrderInfoEnum.OKMART:
                            var RequestBody_C2C = new ECPayLogisticsPrintShippingLabelC2CRequestDto()
                            {
                                MerchantID = ThirdPartyData.MerchantID,
                                AllPayLogisticsID = ohdata.AllPayLogisticsID,
                                CVSPaymentNo = ohdata.CVSPaymentNo,
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
                                AllPayLogisticsID = ohdata.AllPayLogisticsID,
                                CVSPaymentNo = ohdata.CVSPaymentNo,
                                CVSValidationNo = ohdata.CVSValidationNo,
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
                                AllPayLogisticsID = ohdata.AllPayLogisticsID,
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
