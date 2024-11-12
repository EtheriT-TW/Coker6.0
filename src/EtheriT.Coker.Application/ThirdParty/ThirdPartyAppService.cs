using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ThirdPartyAppService : IThirdPartyAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        public ThirdPartyAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            IConfiguration configuration
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        public async Task<ResponseMessageDto> GetAllThirdParty()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var result = from s in db.ThirdParties.Where(e => !e.IsDeleted)
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
                                                    Value = Value
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
                    foreach (var payment in payments)
                    {
                        payment.Used = false;
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

                var output = from pv in db.PaymentTypesValues
                             join pt in db.PaymentTypes on pv.FK_PaymentTypesId equals pt.Id
                             where pv.FK_WebsiteId == WebsiteId && pv.Used
                             select new PaymentTypeItemOutputDto
                             {
                                 Id = pt.Id,
                                 Title = pt.Title,
                                 Code = pt.Code,
                                 Used = true,
                             };

                if (output.Count() > 0) return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
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
    }
}
