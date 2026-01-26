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
using System.Text;

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
                    var Response = await PostResponse.Content.ReadAsStringAsync();
                    response.Message = "success";
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
                ThirdPartyData.IsCollection = "Y";

            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ECPayLogistics=>ECPayGetThirdPartyData回傳資料：{ex.Message}");
            }
            return ThirdPartyData;
        }
    }
}
