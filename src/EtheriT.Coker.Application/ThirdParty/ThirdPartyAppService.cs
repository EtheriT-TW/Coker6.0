using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.Arm;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.Application.ThirdParty
{
    public class ThirdPartyAppService: IThirdPartyAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        public ThirdPartyAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
        }

        public async Task<ResponseMessageDto> GetAllThirdParty()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try {
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
                                                 let Used = payment == null? false: payment.Used
                                                 select new PaymentTypeItemOutputDto { 
                                                    Id = Id,
                                                    Code = p.Code??"",
                                                    Title = p.Title??"",
                                                    Used = Used
                                                 }).ToList(),
                                 ThirdPartyKeypairs = (
                                                from k in db.ThirdPartyKeypairs.Where(e => !e.IsDeleted && e.FK_TPid == s.Id)
                                                join v in db.ThirdPartyKeypairValues.Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                                                on k.Id equals v.FK_ThirdPartyKeypairId into kv
                                                from values in kv.DefaultIfEmpty()
                                                let Id = values == null ? 0 : values.Id
                                                let Value = values == null ? "" : values.Value
                                                select new ThirdPartyKeypairItemOutputDto { 
                                                    Id=Id,
                                                    Code=k.Code??"",
                                                    Title=k.Title??"",
                                                    Value= Value
                                                }).ToList()
                             }; 
                response.Object = new GetAllThirdPartyOutputDto { 
                    thirdPartyItems = await result.ToListAsync()
                };
                response.Success = true;
            }
            catch ( Exception ex )
            {
                response.Error = ex.Message;
            }
            return response;
        }

        public async Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try {
                List<long> thirdPartyIds = dto.ThirdParties?.Select(e => e.Id).ToList() ?? new List<long>();
                long websiteId = await loginUserData.GetWebsiteId();
                long userId = await loginUserData.GetUserId();
                var reg = (from values in db.ThirdPartyKeypairValues.Include(e => e.ThirdPartyKeypair).Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                          join key in db.ThirdPartyKeypairs.Where(e => !e.IsDeleted) on values.FK_ThirdPartyKeypairId equals key.Id
                          where thirdPartyIds.Contains(key.FK_TPid)
                          select values).ToList();
                var ThirdPartyKeypairs = await db.ThirdPartyKeypairs.Where(e => !e.IsDeleted).ToListAsync();
                List<ThirdPartyKeypairValue> AddData = new List<ThirdPartyKeypairValue>();
                if (dto.ThirdParties != null) {
                    foreach ( var kvp in reg )
                    {
                        var item = dto.ThirdParties.Find(e => e.Id == kvp.ThirdPartyKeypair.FK_TPid);
                        if (item != null)
                        {
                            string? value = item.value?.Find(e => e.key == kvp.ThirdPartyKeypair.Code)?.value;
                            if (value != null) kvp.Value = String.Join(", ", value);
                        }
                    }
                    foreach ( var kvp in dto.ThirdParties)
                    {
                        var item = reg.Find(e => e.ThirdPartyKeypair.FK_TPid == kvp.Id);
                        if (item == null)
                        {
                            var key = ThirdPartyKeypairs.Where(e => e.Id == kvp.Id).FirstOrDefault();
                            if (key != null)
                            {
                                var v = kvp.value?.Find(e => e.key == key.Code);
                                if (v != null)
                                {
                                    ThirdPartyKeypairValue value = new ThirdPartyKeypairValue
                                    {
                                        FK_ThirdPartyKeypairId = kvp.Id,
                                        FK_WebsiteId = websiteId,
                                        Value = v.value
                                    };
                                    loginUserData.setOptionParameter(value, userId);
                                    AddData.Add(value);
                                }
                            }
                        }
                    }
                    if (AddData.Count > 0) { 
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
    }
}
