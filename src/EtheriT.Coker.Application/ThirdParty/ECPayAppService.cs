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
                // var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();

                var RequestUri = "/GetTokenbyTrade";
                var RequestBody = await ECPayRequestBody(ohid);
                var content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
                var PostResponse = await ThirdPartyClient_ECPay.PostAsync(RequestUri, content);
                PostResponse.EnsureSuccessStatusCode();
                var jsonResponse = await PostResponse.Content.ReadAsStringAsync();
                response.Message = jsonResponse.ToString();
                //var tokenResponse = JsonConvert.DeserializeObject<ECPayResponseDto>(jsonResponse);

                //if (tokenResponse != null)
                //{
                //    response.Message = $"{tokenResponse.Data}";
                //    response.Success = true;
                //}
                //else throw new Exception("取得ECPayToken發生錯誤");
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
        private async Task<ECPayRequestDto> ECPayRequestBody(long ohid)
        {
            ECPayRequestDto RequestBody = new ECPayRequestDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var Website = await db.Websites.Where(e => e.Id == WebsiteId).FirstOrDefaultAsync();
            var DateTimeNow = DateTime.Now;

            try
            {
                var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                     join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                     join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                     where tp.Title == "綠界支付"
                                                     where tpkv.FK_WebsiteId == WebsiteId
                                                     select new KeyValueDto() { Key = tpk.Code, Value = tpkv.Value }).ToListAsync();

                if (thirdPartyKeypairValues.Any())
                {
                    var MerchantID = thirdPartyKeypairValues.Find(e => e.Key == "MerchantID")?.Value;
                    var PlatformID = thirdPartyKeypairValues.Find(e => e.Key == "PlatformID ")?.Value;
                    var HashKey = thirdPartyKeypairValues.Find(e => e.Key == "HashKey")?.Value;
                    var HashIV = thirdPartyKeypairValues.Find(e => e.Key == "HashIV")?.Value;
                    var ExpireDate = thirdPartyKeypairValues.Find(e => e.Key == "ExpireDate")?.Value;
                    var StoreExpireDate_Barcode = thirdPartyKeypairValues.Find(e => e.Key == "StoreExpireDate_Barcode")?.Value;
                    var StoreExpireDate_CVS = thirdPartyKeypairValues.Find(e => e.Key == "StoreExpireDate_CVS")?.Value;

                    if (MerchantID != "" && HashKey != "" && HashIV != "")
                    {
                        var ohdata = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                        var oddatas = await orderAppService.GetOrderDetails(ohid);
                        if (ohdata != null & oddatas.Any())
                        {
                            var user = await db.FrontUsers.Where(e => e.UUID == ohdata.FK_UUID).FirstOrDefaultAsync();

                            if (PlatformID != "") RequestBody.MerchantID = PlatformID;
                            else RequestBody.MerchantID = MerchantID;

                            RequestBody.RqHeader = new RqHeaderDto();
                            RequestBody.RqHeader.Timestamp = ((DateTimeOffset)DateTimeNow).ToUnixTimeSeconds().ToString();

                            ECPayDataDto PaymentBody = new ECPayDataDto();
                            PaymentBody.PlatformID = PlatformID;
                            PaymentBody.MerchantID = MerchantID;
                            PaymentBody.RememberCard = user == null ? 0 : 1;
                            PaymentBody.PaymentUIType = 2;

                            ECPayCardInfoDto CardInfo = new ECPayCardInfoDto();
                            ECPayUnionPayInfoDto UnionPayInfo = new ECPayUnionPayInfoDto();
                            ECPAyATMInfoDto ATMInfo = new ECPAyATMInfoDto();
                            ECPayCVSInfoDto CVSInfo = new ECPayCVSInfoDto();
                            ECPayBarcodeInfoDto BarcodeInfo = new ECPayBarcodeInfoDto();

                            var paytype = await db.PaymentTypes.Where(e => e.Id == ohdata.Payment).Select(e => e.Code).FirstOrDefaultAsync();
                            if (paytype != null)
                            {
                                paytype = paytype.Replace("ECPay", "");
                                var value = "";
                                if (paytype.IndexOf("_") > -1)
                                {
                                    var temp = paytype.Split("_");
                                    paytype = temp[0];
                                    value = temp[1];
                                }
                                switch (paytype)
                                {
                                    case "CreditCard":
                                        PaymentBody.ChoosePaymentList = "1";
                                        CardInfo.OrderResultURL = "";
                                        break;
                                    case "UnionPay":
                                        PaymentBody.ChoosePaymentList = "6";
                                        UnionPayInfo.OrderResultURL = "";
                                        break;
                                    case "CreditInstallment":
                                        PaymentBody.ChoosePaymentList = "2";
                                        CardInfo.OrderResultURL = "";
                                        CardInfo.CreditInstallment = value;
                                        break;
                                    case "ATM":
                                        PaymentBody.ChoosePaymentList = "3";
                                        var expireDate = ExpireDate == "" ? 3 : int.Parse(ExpireDate) < 1 ? 1 : int.Parse(ExpireDate) > 60 ? 60 : int.Parse(ExpireDate);
                                        ATMInfo.ExpireDate = expireDate;
                                        break;
                                    case "Barcode":
                                        PaymentBody.ChoosePaymentList = "5";
                                        var storeExpireDate_Barcode = StoreExpireDate_Barcode == "" ? 7 : int.Parse(StoreExpireDate_Barcode) < 1 ? 1 : int.Parse(StoreExpireDate_Barcode) > 30 ? 30 : int.Parse(StoreExpireDate_Barcode);
                                        BarcodeInfo.StoreExpireDate = storeExpireDate_Barcode;
                                        break;
                                    case "CVS":
                                        PaymentBody.ChoosePaymentList = "4";
                                        CVSInfo.CVSCode = value;
                                        var storeExpireDate_CVS = StoreExpireDate_CVS == "" ? 7 : int.Parse(StoreExpireDate_CVS) < 1 ? 1 : int.Parse(StoreExpireDate_CVS) > 30 ? 30 : int.Parse(StoreExpireDate_CVS);
                                        CVSInfo.StoreExpireDate = storeExpireDate_CVS;
                                        break;
                                    case "ApplePay":
                                        PaymentBody.ChoosePaymentList = "7";
                                        break;
                                }
                            }
                            else throw new Exception("付款資訊錯誤");

                            ECPayOrderInfoDto OrderInfo = new ECPayOrderInfoDto();

                            OrderInfo.MerchantTradeDate = DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");

                            var oid = ($"000000000{ohdata.Id}").Substring((ohdata.Id).ToString().Length);
                            if (ohdata.TransactionId == null) OrderInfo.MerchantTradeNo = $"{DateTimeNow.ToString("yyyyMMdd")}{oid}";
                            else
                            {
                                if (ohdata.RepayTimes == null) ohdata.RepayTimes = 1;
                                else ohdata.RepayTimes += 1;
                                db.SaveChanges();
                                OrderInfo.MerchantTradeNo = $"{DateTimeNow.ToString("yyyyMMdd")}{oid}-{ohdata.RepayTimes}";
                            }
                            OrderInfo.TotalAmount = ohdata.Subtotal + ohdata.Freight;
                            OrderInfo.ReturnURL = $"{Website.DefaultUrl}/{Website.OrgName}/ShoppingCar?{oid}";
                            OrderInfo.TradeDesc = $"{Website.Title}-商品購買交易";

                            var itemlist = "";
                            foreach (var oddata in oddatas)
                            {
                                if (itemlist.Length == 0)
                                {
                                    itemlist = oddata.Title.Length > 200 ? $"{oddata.Title.Substring(0, 197)}..." : oddata.Title;
                                }
                                else
                                {
                                    var templist = $"{itemlist}#{oddata.Title}";
                                    if (templist.Length > 194)
                                    {
                                        itemlist += "#其他...";
                                    }
                                    else itemlist = templist;
                                }
                            }
                            OrderInfo.ItemName = itemlist;

                            PaymentBody.OrderInfo = OrderInfo;

                            PaymentBody.CardInfo = CardInfo;
                            PaymentBody.UnionPayInfo = UnionPayInfo;
                            PaymentBody.ATMInfo = ATMInfo;
                            PaymentBody.CVSInfo = CVSInfo;
                            PaymentBody.BarcodeInfo = BarcodeInfo;

                            PaymentBody.ConsumerInfo = new ECPayConsumerInfoDto();
                            PaymentBody.ConsumerInfo.MerchantMemberID = user?.Id.ToString();
                            PaymentBody.ConsumerInfo.Email = ohdata.OrdererEmail;
                            PaymentBody.ConsumerInfo.Phone = ohdata.OrdererCellPhone;
                            PaymentBody.ConsumerInfo.Name = ohdata.Orderer;
                            PaymentBody.ConsumerInfo.CountryCode = "158";
                            PaymentBody.ConsumerInfo.Address = ohdata.OrdererAddress.Replace(" ", "");

                            RequestBody.Data = Encrypt(PaymentBody, HashKey, HashIV);
                        }
                        else throw new Exception("查無訂單資訊");
                    }
                    else throw new Exception("查無ECPay所需參數");
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
