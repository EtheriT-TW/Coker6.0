using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ECPayAppService : IECPayAppService
    {
        private readonly HttpClient ThirdPartyClient_ECPay;
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly IOrderAppService orderAppService;
        public ECPayAppService(
            IHttpClientFactory httpClientFactory,
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration,
            IOrderAppService orderAppService
        )
        {
            ThirdPartyClient_ECPay = httpClientFactory.CreateClient("ThirdPartyClient_ECPay");
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.orderAppService = orderAppService;
        }
        public async Task<ResponseMessageDto> ECPayGetToken(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
                var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                     join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                     join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                     where tp.Title == "綠界支付"
                                                     where tpkv.FK_WebsiteId == WebsiteId
                                                     select new KeyValueDto() { Key = tpk.Title, Value = tpkv.Value }).ToListAsync();
                var MerchantID = "";
                var HashKey = "";
                var HashIV = "";

                if (thirdPartyKeypairValues.Any())
                {
                    MerchantID = thirdPartyKeypairValues.Find(e => e.Key == "商店代號").Value;
                    HashKey = thirdPartyKeypairValues.Find(e => e.Key == "HashKey").Value;
                    HashIV = thirdPartyKeypairValues.Find(e => e.Key == "HashIV").Value;
                }
                if (MerchantID != "")
                {
                    var RequestUri = $"/GetTokenbyTrade";
                    var RequestBody = ECPayRequest(ohid);

                    //var PostResponse = await ThirdPartyClient_ECPay.PostAsync(RequestUri, RequestBody);
                    //PostResponse.EnsureSuccessStatusCode();
                    //var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                    //var tokenPayResponse = JsonConvert.DeserializeObject<PChomePayTokenDto>(jsonResponse);

                    //if (tokenPayResponse != null)
                    //{
                    //    ThirdPartyClient_ECPay.DefaultRequestHeaders.Add("pcpay-token", tokenPayResponse.token);

                    //    response.Message = $"{tokenPayResponse.token}; {tokenPayResponse.expired_in}; {tokenPayResponse.expired_timestamp}";
                    //    response.Success = true;

                    //}
                    //else throw new Exception("取得ECPayToken發生錯誤");
                }
                else throw new Exception("查無ECPay所需參數");
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

        private async Task<string> ECPayRequest(long ohid)
        {
            var response = "";
            ECPayRequestDto requestBody = new ECPayRequestDto();


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
            return response;
        }
    }
}
