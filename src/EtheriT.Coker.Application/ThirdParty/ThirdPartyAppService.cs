using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Policy;
using System.Text;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ThirdPartyAppService : IThirdPartyAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;
        private readonly ITokenAppService tokenAppService;
        private readonly HttpClient ThirdPartyClient_Front;
        private readonly IWebHostEnvironment _env;
        public ThirdPartyAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration,
            ITokenAppService tokenAppService,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
            this.tokenAppService = tokenAppService;
            this._env = env;
            ThirdPartyClient_Front = httpClientFactory.CreateClient("ThirdPartyClient_Front");
        }

        public async Task<ResponseMessageDto> GetAllThirdParty(ThirdPartyServiceTypeEnum ServeiceType)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var result = from s in db.ThirdParties.Where(e => !e.IsDeleted)
                             where s.ServiceType == ServeiceType
                             orderby s.ser_no
                             select new ThirdPartyItemOutputDto
                             {
                                 Id = s.Id,
                                 Title = s.Title,
                                 PaymentTypes = (from p in db.PaymentTypes.Where(e => !e.IsDeleted && e.FK_ThirdPartyId == s.Id)
                                                 join pv in db.PaymentTypesValues.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                                                 on p.Id equals pv.FK_PaymentTypesId into pt
                                                 from payment in pt.DefaultIfEmpty()
                                                 let Id = payment == null ? 0 : payment.Id
                                                 let Used = payment == null ? false : payment.Used
                                                 select new PaymentTypeItemOutputDto
                                                 {
                                                     Id = Id,
                                                     Code = p.Code ?? "",
                                                     Title = p.Title ?? "",
                                                     Used = Used
                                                 }).ToList(),
                                 ThirdPartyKeypairs = (
                                                from k in db.ThirdPartyKeypairs.Where(e => !e.IsDeleted && e.FK_TPid == s.Id)
                                                join v in db.ThirdPartyKeypairValues.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                                                on k.Id equals v.FK_ThirdPartyKeypairId into kv
                                                from values in kv.DefaultIfEmpty()
                                                let Id = values == null ? 0 : values.Id
                                                let Value = values == null ? "" : values.Value
                                                select new ThirdPartyKeypairItemOutputDto
                                                {
                                                    Id = Id,
                                                    Code = k.Code ?? "",
                                                    Title = k.Title ?? "",
                                                    Value = Value,
                                                    PromptText = k.PromptText ?? "",
                                                    InputType = k.InputType.ToString() ?? "",
                                                }).ToList()
                             };
                response.Object = new GetAllThirdPartyOutputDto
                {
                    thirdPartyItems = await result.ToListAsync()
                };
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                List<long> thirdPartyIds = dto.ThirdParties?.Select(e => e.Id).ToList() ?? new List<long>();
                long websiteId = await loginUserData.GetWebsiteId();
                long userId = await loginUserData.GetUserId();

                var payment_type = await db.PaymentTypes.ToListAsync();
                var payments = await db.PaymentTypesValues.Where(e => e.FK_WebsiteId == websiteId).ToListAsync();
                List<PaymentTypesValue> AddPayments = new List<PaymentTypesValue>();

                if (payment_type != null && payments != null)
                {
                    if (dto.ServiceType == ThirdPartyServiceTypeEnum.Payment)
                    {
                        foreach (var payment in payments)
                        {
                            payment.Used = false;
                        }
                    }
                    if (dto.PaymentType != null)
                    {
                        foreach (var item in dto.PaymentType)
                        {
                            var type = payment_type.Find(e => e.Code == item);
                            if (type != null)
                            {
                                if (payments.Find(e => e.FK_PaymentTypesId == type.Id) != null)
                                {
                                    payments.Find(e => e.FK_PaymentTypesId == type.Id).Used = true;
                                }
                                else
                                {
                                    AddPayments.Add(new PaymentTypesValue()
                                    {
                                        Used = true,
                                        FK_PaymentTypesId = type.Id,
                                        FK_WebsiteId = websiteId,
                                    });
                                }
                            }
                        }
                        if (AddPayments.Count > 0) db.AddRange(AddPayments);
                    }
                    await db.SaveChangesAsync();
                }

                var reg = (from values in db.ThirdPartyKeypairValues.Include(e => e.ThirdPartyKeypair).Where(e => e.FK_WebsiteId == websiteId)
                           join key in db.ThirdPartyKeypairs.Where(e => !e.IsDeleted) on values.FK_ThirdPartyKeypairId equals key.Id
                           where thirdPartyIds.Contains(key.FK_TPid)
                           select values).ToList();
                var ThirdPartyKeypairs = await db.ThirdPartyKeypairs.Where(e => !e.IsDeleted).ToListAsync();
                List<ThirdPartyKeypairValue> AddData = new List<ThirdPartyKeypairValue>();
                if (dto.ThirdParties != null)
                {
                    foreach (var kvpr in reg)
                    {
                        var items = dto.ThirdParties.Find(e => e.Id == kvpr.ThirdPartyKeypair.FK_TPid);
                        if (items != null)
                        {
                            foreach (var item in items.value)
                            {
                                string? value = item.key != null ? item.key == kvpr.ThirdPartyKeypair.Code ? item.value : null : null;
                                if (value != null) kvpr.Value = String.Join(", ", value);
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                    foreach (var kvps in dto.ThirdParties)
                    {
                        if (kvps.value != null)
                        {
                            foreach (var kvp in kvps.value)
                            {
                                var item = reg.Find(e => e.ThirdPartyKeypair.FK_TPid == kvps.Id && e.ThirdPartyKeypair.Code == kvp.key);
                                if (item == null || kvp.value != item.Value)
                                {
                                    ThirdPartyKeypairValue value = new ThirdPartyKeypairValue
                                    {
                                        FK_ThirdPartyKeypairId = ThirdPartyKeypairs.Where(e => e.Code == kvp.key && e.FK_TPid == kvps.Id).Select(e => e.Id).FirstOrDefault(),
                                        FK_WebsiteId = websiteId,
                                        Value = kvp.value
                                    };
                                    loginUserData.setOptionParameter(value, userId);
                                    AddData.Add(value);
                                }
                            }
                        }
                    }
                    if (AddData.Count > 0)
                    {
                        db.AddRange(AddData);
                        await db.SaveChangesAsync();
                    }
                }
                if (dto.ServiceType == ThirdPartyServiceTypeEnum.Logistics)
                {
                    foreach (var ThirdPartie in dto.ThirdParties)
                    {
                        foreach (var value in ThirdPartie.value)
                        {
                            var LogisticsType = new List<ShippingTypeEnum> { };

                            if (value.key == "EnableB2C" && value.value == "false")
                            {
                                LogisticsType.AddRange(new[]
                                {
                                    ShippingTypeEnum.綠界_大宗寄倉_全家,
                                    ShippingTypeEnum.綠界_大宗寄倉_711冷凍店取,
                                    ShippingTypeEnum.綠界_大宗寄倉_711超商,
                                    ShippingTypeEnum.綠界_大宗寄倉_萊爾富,
                                });
                            }
                            if (value.key == "EnableC2C" && value.value == "false")
                            {
                                LogisticsType.AddRange(new[]
                                {
                                    ShippingTypeEnum.綠界_門市寄取_711超商,
                                    ShippingTypeEnum.綠界_門市寄取_OK超商,
                                    ShippingTypeEnum.綠界_門市寄取_全家,
                                    ShippingTypeEnum.綠界_門市寄取_萊爾富,
                                });
                            }
                            if (value.key == "EnableHomeDelivery" && value.value == "false")
                            {
                                LogisticsType.AddRange(new[]
                                {
                                    ShippingTypeEnum.綠界_黑貓,
                                    ShippingTypeEnum.綠界_中華郵政,
                                });
                            }

                            var db_logistics = await db.LogisticsSettings.Where(e => e.FK_WebsiteId == websiteId && LogisticsType.Contains(e.LogisticsType) && e.FreigntStatusType != FreigntStatusTypeEnum.停用).ToListAsync();
                            foreach (var item in db_logistics)
                            {
                                item.FreigntStatusType = FreigntStatusTypeEnum.停用;
                                item.LastModifierUserId = userId;
                                item.LastModificationTime = DateTime.Now;
                            }

                            await db.SaveChangesAsync();
                        }
                    }
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<JsonResult> GetDisplayPayment()
        {
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var output = await (from pv in db.PaymentTypesValues
                                    join pt in db.PaymentTypes on pv.FK_PaymentTypesId equals pt.Id
                                    where pv.FK_WebsiteId == WebsiteId && pv.Used
                                    orderby pt.SerNo
                                    select new PaymentTypeItemOutputDto
                                    {
                                        Id = pt.Id,
                                        Title = pt.Title,
                                        Code = pt.Code,
                                        Icon = pt.Icons != "" ? $"/images/paymenticon/{pt.Icons}" : "",
                                        Used = true,
                                        MaxAmount = pt.MaxAmount,
                                        MinAmount = pt.MinAmount,
                                    }).ToListAsync();

                if (output.Any())
                {
                    const int defaultMax = 20000;
                    const int defaultMin = 31;

                    var ecpayItems = output
                        .Where(x => x.Code?.ToLower().Contains("ecpay") == true)
                        .ToList();

                    var nonEcpayItems = output
                        .Where(x => x.Code?.ToLower().Contains("ecpay") != true)
                        .ToList();

                    if (ecpayItems.Any())
                    {
                        int maxAmount = ecpayItems
                            .Where(x => x.MaxAmount.HasValue)
                            .Select(x => x.MaxAmount.Value)
                            .DefaultIfEmpty(defaultMax)
                            .Max();

                        int minAmount = ecpayItems
                            .Select(x => x.MinAmount)
                            .DefaultIfEmpty(defaultMin)
                            .Min();

                        nonEcpayItems.Add(new PaymentTypeItemOutputDto
                        {
                            Id = ecpayItems.First().Id,
                            Title = "其他支付方式",
                            Code = "ECPay",
                            Icon = "",
                            Used = true,
                            MaxAmount = maxAmount,
                            MinAmount = minAmount
                        });
                    }

                    output = nonEcpayItems;

                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無運費資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<PaymentTypeItemOutputDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<ThirdPartyKeypairItemOutputDto>> GetPaymentResult(long paytypeid)
        {
            var output = new List<ThirdPartyKeypairItemOutputDto>();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

            try
            {
                var TPid = await db.PaymentTypes.Where(e => e.Id == paytypeid).Select(e => e.FK_ThirdPartyId).FirstOrDefaultAsync();
                if (TPid != null)
                {
                    var thirdpartykeypairs = await db.ThirdPartyKeypairs.Where(e => e.FK_TPid == TPid).ToListAsync();
                    if (thirdpartykeypairs != null && thirdpartykeypairs.Count > 0)
                    {
                        foreach (var thirdpartykeypair in thirdpartykeypairs)
                        {
                            var value = await db.ThirdPartyKeypairValues.Where(e => e.FK_ThirdPartyKeypairId == thirdpartykeypair.Id && e.FK_WebsiteId == WebsiteId).FirstOrDefaultAsync();
                            if (value != null)
                            {
                                output.Add(new ThirdPartyKeypairItemOutputDto()
                                {
                                    Id = thirdpartykeypair.Id,
                                    Title = thirdpartykeypair.Title,
                                    Value = value.Value,
                                    Code = thirdpartykeypair.Code,
                                    PromptText = thirdpartykeypair.PromptText,
                                });
                            }
                        }
                    }
                }
                else throw new Exception("付款資訊有誤");
            }
            catch (Exception e)
            {
            }
            return output;
        }
        public async Task<ResponseMessageDto> CheckSource(string token)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null) return response;

                string tokenid = jsonToken.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
                string username = jsonToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                long expTimestamp = long.Parse(jsonToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value ?? "0");
                DateTime expDate = DateTimeOffset.FromUnixTimeSeconds(expTimestamp).UtcDateTime;

                if (expDate < DateTime.UtcNow) throw new Exception("Token 已過期");
                else
                {
                    response.Success = true;
                    var db_token = await db.Tokens.Where(e => e.id.ToString() == tokenid).FirstOrDefaultAsync();
                    if (db_token != null && db_token.UserID != null)
                    {
                        var user = await db.Users.Where(e => e.Id == db_token.UserID && e.Name == username).FirstOrDefaultAsync();
                        if (user != null) response.Success = true;
                        else throw new Exception("查無使用者資訊");
                    }
                    else throw new Exception("Token資訊錯誤");
                }

            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> HandleThirdPartyPayment(HandleThirdPartyPaymentDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                response = await CallFrontApi("HandleThirdPartyPayment", dto);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> HandleThirdPartyLogistics(HandleThirdPartyLogisticsDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                response = await CallFrontApi("HandleThirdPartyLogistics", dto);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ResponseMessageDto> CallFrontApi(string apiPath, object dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var token = await tokenAppService.CheckToken(null);
                if (token == null) throw new Exception("取得Token發生錯誤");

                var websiteId = await loginUserData.GetWebsiteId();

                var website = await db.Websites.Where(e => e.Id == websiteId).FirstOrDefaultAsync();
                if (website == null) throw new Exception("取得網站內容發生錯誤");

                var dtoType = dto.GetType();
                var tokenProp = dtoType.GetProperty("Token");
                tokenProp?.SetValue(dto, token.Token);

                var frontApiUrl = _env.IsProduction() ? $"{website.DefaultUrl}/api/ThirdParty/{apiPath}" : $"https://lcb.develop.coker.ezsale.tw/api/ThirdParty/{apiPath}";
                var jsonContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                var postresponse = await ThirdPartyClient_Front.PostAsync(frontApiUrl, jsonContent);

                if (postresponse.IsSuccessStatusCode)
                {
                    var content = await postresponse.Content.ReadAsStringAsync();
                    var idx = content.IndexOf("\"message\":");
                    if (idx > -1)
                    {
                        var sub = content.Substring(idx + 10);
                        var html = sub;
                    }
                    var obj = JsonConvert.DeserializeObject<ResponseMessageDto>(content.Trim().Trim('"').Replace("\\r\\n", ""));
                    response = JsonConvert.DeserializeObject<ResponseMessageDto>(content);
                }
                else response.Message = $"{postresponse.StatusCode}, Failed to call front API";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
