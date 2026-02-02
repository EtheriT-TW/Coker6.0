using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Token;
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
        public ECPayLogisticsAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IConfiguration configuration
        )
        {
            ThirdPartyClient_ECPayLogistics = httpClientFactory.CreateClient("ThirdPartyClient_ECPayLogistics");
            this.db = db;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
        }
        public async Task<ResponseMessageDto> ECPayLogisticsGetMap(long scid, string LogisticsSubType)
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
                    RequestBody.MerchantTradeNo = DateTime.Now.ToString("yyyyMMdd") + scid.ToString();
                    RequestBody.LogisticsSubType = LogisticsSubType;
                    RequestBody.IsCollection = ThirdPartyData.IsCollection;
                    RequestBody.ServerReplyURL = $"{Website.DefaultUrl}/api/ThirdParty/ECPayLogisticsGetMapResponse";
                    RequestBody.ExtraData = scid.ToString();
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
        public async Task<bool> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData)
        {
            try
            {
                var ThirdPartyData = await ECPayGetThirdPartyData() ?? throw new Exception("商家未確實設置綠界物流資料");

                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsExpressCreate", JsonConvert.SerializeObject(ResultResponseData));

                var scdata = await db.ShoppingCarts.Where(e => e.Id == long.Parse(ResultResponseData.ExtraData)).FirstOrDefaultAsync();
                if (scdata == null) throw new Exception($"查無購物車資訊");
                scdata.CVSStoreID = ResultResponseData.CVSStoreID;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayReturn回傳資料：{ex.Message}");
                await loginUserData.SetLogs(0, configuration.GetValue<long>("WebConfig:SiteId"), $"ECPayLogisticsGetMapResponse", $"ECPayLogistics=>ECPayLogisticsGetMapResponse回傳資料：{ex.Message}");
                return false;
            }
        }
        public async Task<ResponseMessageDto> ECPayLogisticsExpressCreate(long ohid, List<string> prod_titles)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                ECPayThirdPartyDataDto ThirdPartyData = await ECPayGetThirdPartyData();

                if (!(ThirdPartyData.MerchantID != "" && ThirdPartyData.HashKey != "" && ThirdPartyData.HashIV != "")) throw new Exception("查無ECPay所需參數");

                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata == null) throw new Exception("查無訂單資訊");

                var RequestUri = "/Express/Create";

                ECPayLogisticsCreateRequestDto RequestBody = new ECPayLogisticsCreateRequestDto();
                RequestBody = await ECPayExpressRequestBody(ThirdPartyData, ohdata.Id, prod_titles);

                var content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                var PostResponse = await ThirdPartyClient_ECPayLogistics.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var Response = await PostResponse.Content.ReadAsStringAsync();

                if (Response.Length > 0)
                {
                    if(int.Parse(Response.Split('|')[0]) == 1) response.Success = true;
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
        private async Task<ECPayLogisticsCreateRequestDto> ECPayExpressRequestBody(ECPayThirdPartyDataDto ThirdPartyData, long ohid, List<string> prod_titles)
        {
            ECPayLogisticsCreateRequestDto RequestBody = new ECPayLogisticsCreateRequestDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var DateTimeNow = DateTime.Now;

            try
            {
                var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (ohdata == null || !prod_titles.Any()) throw new Exception($"查無訂單資訊");

                var user = await db.FrontUsers.Where(e => e.UUID == ohdata.FK_UUID).FirstOrDefaultAsync();

                RequestBody.MerchantID = ThirdPartyData.MerchantID;
                RequestBody.MerchantTradeNo = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                RequestBody.MerchantTradeDate = DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");

                switch ((int)ohdata.LogisticsSetting.LogisticsType)
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
                }
                RequestBody.GoodsAmount = ohdata.Subtotal + ohdata.Freight;

                RequestBody.IsCollection = ThirdPartyData.IsCollection;
                if (ThirdPartyData.IsCollection == "Y") RequestBody.CollectionAmount = RequestBody.GoodsAmount; else RequestBody.CollectionAmount = 0;

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
                RequestBody.ReceiverStoreID = ohdata.CVSStoreID;

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
                                                     where tp.Title == "綠界支付"
                                                     where tpkv.FK_WebsiteId == WebsiteId
                                                     select new KeyValueDto() { Key = tpk.Code, Value = tpkv.Value }).ToListAsync();

                if (thirdPartyKeypairValues == null) throw new Exception("查無ThirdParty資料");

                var thirdPartyDict = thirdPartyKeypairValues.ToDictionary(e => e.Key, e => e.Value);

                ThirdPartyData.MerchantID = thirdPartyDict.GetValueOrDefault("MerchantID") ?? throw new Exception("商家未確實設置綠界支付資料");
                ThirdPartyData.HashKey = thirdPartyDict.GetValueOrDefault("HashKey") ?? throw new Exception("商家未確實設置綠界支付資料");
                ThirdPartyData.HashIV = thirdPartyDict.GetValueOrDefault("HashIV") ?? throw new Exception("商家未確實設置綠界支付資料");

                //測試特店資料：B2C及宅配
                //ThirdPartyData.MerchantID = "2000132";
                //ThirdPartyData.HashKey = "5294y06JbISpM5x9";
                //ThirdPartyData.HashIV = "v77hoKGq4kWxNNIS";

                //測試特店資料：C2C
                //ThirdPartyData.MerchantID = "2000933";
                //ThirdPartyData.HashKey = "XBERn1YOvpM9nfZc";
                //ThirdPartyData.HashIV = "h1ONHk4P4yqbl5LK";

                ThirdPartyData.IsCollection = "Y";

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
