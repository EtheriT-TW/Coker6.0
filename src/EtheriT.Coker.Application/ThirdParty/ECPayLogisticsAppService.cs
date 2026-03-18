using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ECPayLogisticsAppService : IECPayLogisticsAppService
    {
        private readonly HttpClient ThirdPartyClient_ECPayLogistics;
        private readonly CokerDbContext db;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public ECPayLogisticsAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            ThirdPartyClient_ECPayLogistics = httpClientFactory.CreateClient("ThirdPartyClient_ECPayLogistics");
            this.db = db;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.mapper = mapper;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsGetMap(string SCIds, string LogisticsSubType)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData();
                var uuid = await tokenAppService.GetUUID();
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();

                if (ThirdPartyData.MerchantID != null)
                {
                    var RequestUri = "/Express/map";

                    var RequestBody = new ECPayLogisticsMapRequestDto();
                    RequestBody.MerchantID = ThirdPartyData.MerchantID;
                    RequestBody.MerchantTradeNo = GenMerchantTradeNo();
                    RequestBody.LogisticsSubType = LogisticsSubType;
                    RequestBody.IsCollection = ThirdPartyData.IsCollection;
                    RequestBody.ServerReplyURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayLogisticsGetMapResponse";
                    RequestBody.ExtraData = SCIds;
                    var content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                    var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                    PostResponse.EnsureSuccessStatusCode();

                    response.Success = true;
                    response.Message = await PostResponse.Content.ReadAsStringAsync();

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
                var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("商家未確實設置綠界物流資料");

                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsExpressCreate", JsonConvert.SerializeObject(ResultResponseData));

                var scids = JsonConvert.DeserializeObject<List<long>>(ResultResponseData.ExtraData);

                var scdatas = await db.ShoppingCarts.Where(e => scids.Contains(e.Id)).ToListAsync();

                if (!scdatas.Any()) throw new Exception($"查無購物車資訊");

                foreach (var scdata in scdatas)
                {
                    scdata.LogisticsSubType = ResultResponseData.LogisticsSubType;
                    scdata.CVSStoreID = ResultResponseData.CVSStoreID;
                    scdata.CVSStoreName = ResultResponseData.CVSStoreName;
                }

                db.SaveChanges();

                response.Success = true;
                response.Message = $"/{Website.OrgName}/ShoppingCar";
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayLogisticsGetMapResponse回傳資料：{ex.Message}");
                response.Message = $"/{Website.OrgName}/ShoppingCar,{ex.Message}";
                return response;
            }
        }
        public async Task<ResponseMessageDto> ECPayLogisticsExpressCreate(long ohid, List<string> prod_titles, ShippingTypeEnum LogisticsType)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (string.IsNullOrEmpty(ThirdPartyData.MerchantID) || string.IsNullOrEmpty(ThirdPartyData.HashKey) || string.IsNullOrEmpty(ThirdPartyData.HashIV)) throw new Exception("查無ECPay所需參數");

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception("查無訂單資訊");

                var RequestUri = "/Express/Create";

                StringContent content = null;

                if ((int)LogisticsType == 16 || (int)LogisticsType == 17)
                {
                    ECPayLogisticsCreateHomeRequestDto RequestBody = new ECPayLogisticsCreateHomeRequestDto();
                    RequestBody = await ECPayExpressHomeRequestBody(ThirdPartyData, ohdata, LogisticsType, prod_titles);

                    content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                }
                else
                {
                    ECPayLogisticsCreateCVSRequestDto RequestBody = new ECPayLogisticsCreateCVSRequestDto();
                    RequestBody = await ECPayExpressCVSRequestBody(ThirdPartyData, ohdata, LogisticsType, prod_titles);

                    content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                }

                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var Response = await PostResponse.Content.ReadAsStringAsync();

                if (Response.Length > 0)
                {
                    if (int.Parse(Response.Split('|')[0]) == 1) response.Success = true;
                    response.Message = Response.Split('|')[1];
                    await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsExpressCreate", Response);

                    return response;
                }
                else throw new Exception($"無法取得門市訂單建立資訊");

            }
            catch (Exception ex)
            {
                response.Message = $"ECPayCreatePayment failed: {ex.Message}";
            }
            return response;
        }
        public async Task<bool> ECPayLogisticsExpressCreateResponse(ECPayLogisticsCreateResponseDto ResultResponseData)
        {
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("商家未確實設置綠界物流資料");

                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsExpressCreate", JsonConvert.SerializeObject(ResultResponseData));

                var ohdata = await db.Order_Headers.Where(e => e.MerchantTradeNo == ResultResponseData.MerchantTradeNo).FirstOrDefaultAsync();
                if (ohdata == null) throw new Exception($"查無訂單資訊");

                ohdata.LogisticsStatusCode = ResultResponseData.RtnCode;
                ohdata.AllPayLogisticsID = ResultResponseData.AllPayLogisticsID;
                ohdata.LogisticsUpdateStatusDate = DateTime.ParseExact(ResultResponseData.UpdateStatusDate, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
                ohdata.CVSPaymentNo = ResultResponseData.CVSPaymentNo;
                ohdata.CVSValidationNo = ResultResponseData.CVSValidationNo;
                ohdata.BookingNote = ResultResponseData.BookingNote;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayReturn回傳資料：{ex.Message}");
                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsExpressCreateResponse", $"ECPayLogistics=>ECPayReturn回傳資料：{ex.Message}");
                return false;
            }
        }
        private async Task<ECPayLogisticsCreateRequestDto> ECPayExpressRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, ShippingTypeEnum LogisticsType, List<string> prod_titles)
        {
            ECPayLogisticsCreateRequestDto RequestBody = new ECPayLogisticsCreateRequestDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var DateTimeNow = DateTime.Now;

            try
            {
                var user = await db.FrontUsers.Where(e => e.UUID == ohdata.FK_UUID).FirstOrDefaultAsync();

                RequestBody.MerchantID = ThirdPartyData.MerchantID;
                RequestBody.MerchantTradeNo = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                RequestBody.MerchantTradeDate = DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");

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
                RequestBody.GoodsAmount = ohdata.Subtotal + ohdata.Freight;

                RequestBody.IsCollection = ThirdPartyData.IsCollection;

                var itemlist = "";
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

                RequestBody.SenderName = ohdata.Orderer;
                RequestBody.SenderPhone = ohdata.OrdererTelePhone;
                RequestBody.SenderCellPhone = ohdata.OrdererCellPhone;
                RequestBody.ReceiverName = ohdata.Recipient;
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
        private async Task<ECPayLogisticsCreateCVSRequestDto> ECPayExpressCVSRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, ShippingTypeEnum LogisticsType, List<string> prod_titles)
        {
            ECPayLogisticsCreateCVSRequestDto RequestBody = new ECPayLogisticsCreateCVSRequestDto();

            try
            {
                RequestBody = mapper.Map<ECPayLogisticsCreateCVSRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata, LogisticsType, prod_titles));

                if (ThirdPartyData.IsCollection == "Y") RequestBody.CollectionAmount = RequestBody.GoodsAmount; else RequestBody.CollectionAmount = 0;

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
        private async Task<ECPayLogisticsCreateHomeRequestDto> ECPayExpressHomeRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, ShippingTypeEnum LogisticsType, List<string> prod_titles)
        {
            //public string GoodsWeight { get; set; }
            //public string Temperature { get; set; }
            //public string Specification { get; set; }

            ECPayLogisticsCreateHomeRequestDto RequestBody = new ECPayLogisticsCreateHomeRequestDto();

            try
            {
                RequestBody = mapper.Map<ECPayLogisticsCreateHomeRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata, LogisticsType, prod_titles));

                RequestBody.GoodsWeight = RequestBody.GoodsWeight;
                RequestBody.SenderAddress = ohdata.OrdererAddress.Replace(" ", "");
                RequestBody.SenderZipCode = "338";
                RequestBody.ReceiverAddress = ohdata.RecipientAddress.Replace(" ", "");
                RequestBody.ReceiverZipCode = "338";

                RequestBody.CheckMacValue = Encrypt(RequestBody, ThirdPartyData.HashKey, ThirdPartyData.HashIV);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayRequestBody回傳資料：{ex.Message}");
                throw new Exception($"ECPayLogistics=>ECPayRequestBody回傳資料：{ex.Message}");
            }
            return RequestBody;
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
                var IsCollection = thirdPartyDict.GetValueOrDefault("IsCollection") ?? throw new Exception("商家未確實設置綠界物流資料");
                ThirdPartyData.IsCollection = IsCollection == "true" ? "Y" : "N";
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
    }
}
