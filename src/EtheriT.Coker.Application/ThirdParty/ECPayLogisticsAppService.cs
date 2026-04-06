using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.ShoppingCart;
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
using System.Web;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ECPayLogisticsAppService : IECPayLogisticsAppService
    {
        private readonly HttpClient ThirdPartyClient_ECPayLogistics;
        private readonly CokerDbContext db;
        private readonly IOrderAppService orderAppService;
        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public ECPayLogisticsAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            IOrderAppService orderAppService,
            IShoppingCartAppService shoppingCartAppService,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            ThirdPartyClient_ECPayLogistics = httpClientFactory.CreateClient("ThirdPartyClient_ECPayLogistics");
            this.db = db;
            this.orderAppService = orderAppService;
            this.shoppingCartAppService = shoppingCartAppService;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.mapper = mapper;
        }
        public async Task<ECPayLogisticsMapRequestDto> ECPayLogisticsGetMapRequestBody(string SCIds, string LogisticsSubType)
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
                RequestBody.IsCollection = ThirdPartyData.IsCollection;
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
        private async Task<ECPayLogisticsCreateRequestDto> ECPayExpressRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, ShippingTypeEnum LogisticsType)
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
        private async Task<ECPayLogisticsCreateCVSRequestDto> ECPayExpressCVSRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, ShippingTypeEnum LogisticsType)
        {
            ECPayLogisticsCreateCVSRequestDto RequestBody = new ECPayLogisticsCreateCVSRequestDto();

            try
            {
                RequestBody = mapper.Map<ECPayLogisticsCreateCVSRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata, LogisticsType));

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
        private async Task<ECPayLogisticsCreateHomeRequestDto> ECPayExpressHomeRequestBody(ECPayThirdPartyDataDto ThirdPartyData, Core.Models.Order_Header ohdata, ShippingTypeEnum LogisticsType)
        {
            //public string GoodsWeight { get; set; }
            //public string Temperature { get; set; }
            //public string Specification { get; set; }

            ECPayLogisticsCreateHomeRequestDto RequestBody = new ECPayLogisticsCreateHomeRequestDto();

            try
            {
                RequestBody = mapper.Map<ECPayLogisticsCreateHomeRequestDto>(await ECPayExpressRequestBody(ThirdPartyData, ohdata, LogisticsType));

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
