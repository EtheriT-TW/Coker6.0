using AutoMapper;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Dto.StoreSet;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Web.Core.Models;
using System.Data;

namespace EtheriT.Coker.Application.StoreSet
{
    public class StoreSetAppService : IStoreSetAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        public StoreSetAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
        }

        public async Task<StoreSetResponseMessageDto> find(string key)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            var result = await db.StoreSet.Where(e => !e.IsDeleted)
                                .Where(e => e.key == key)
                                .FirstOrDefaultAsync();
            if (result != null)
            {
                output.item = mapper.Map<StoreSetOutputDto>(result);
            }
            else output.Message = "查無資料";

            return output;
        }

        public async Task<StoreSetResponseMessageDto> getAll(List<long> StoreSetGroupId)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            var level = await loginUserData.GetWebsiteLevel();
            var result = from g in db.StoreSetGroup.Where(e => !e.IsDeleted && StoreSetGroupId.Contains(e.Id))
                         select new StoreSetGroupOutputDto
                         {
                             Image = g.Image,
                             Description = g.Description!,
                             Title = g.Title,
                             storeSets = (from s in db.StoreSet.Where(e => (e.Level == null || level >= e.Level) && e.FK_StoreSetGroupId == g.Id).OrderBy(e => e.jobID)
                                          select new StoreSetOutputDto
                                          {
                                              key = s.key,
                                              name = s.name,
                                              maxlength = s.maxlength,
                                              memo = s.memo,
                                              pattern = s.pattern,
                                              type = s.type!,
                                              storeSetItemOutputDtos = (
                                                 from item in db.StoreSetItems.Where(e => (e.Level == null || level >= e.Level) && e.FK_StoreSetId == s.Id)
                                                 select new StoreSetItemOutputDto
                                                 {
                                                     Key = item.Key,
                                                     Value = item.Value,
                                                 }
                                             ).ToList()
                                          }).ToList()
                         };
            if (result != null)
            {
                output.storeSets = mapper.Map<List<StoreSetGroupOutputDto>>(result);
                output.Success = true;
            }
            else output.Message = "資料為空";

            return output;
        }
        public async Task<StoreSetResponseMessageDto> getValues(StoreSetGetValueInput dto)
        {
            StoreSetResponseMessageDto output;
            long websiteId;
            if (dto.SiteId == null || dto.SiteId == 0) websiteId = await loginUserData.GetWebsiteId();
            else websiteId = dto.SiteId.Value;
            if (dto.StoreSetGroupId != null) output = await getValuesByGroupId(dto.StoreSetGroupId.Value, websiteId);
            else if (dto.keys != null && dto.keys.Count > 0) output = await getValuesByKeys(dto.keys, websiteId);
            else if (!string.IsNullOrEmpty(dto.key)) output = await getValueByKey(dto.key, websiteId);
            else
            {
                output = new StoreSetResponseMessageDto { Message = "缺少搜尋條件" };
            }
            return output;
        }
        private async Task<StoreSetResponseMessageDto> getValuesByGroupId(long StoreSetGroupId, long websiteId)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();

            try
            {
                var result = await db.StoreSetDetail
                    .Include(e => e.StoreSet)
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.StoreSet.FK_StoreSetGroupId == StoreSetGroupId)
                    .Where(e => e.FK_WebsiteId == websiteId)
                    .ToListAsync();
                output.storeSetDetails = (
                    from s in result
                    select new StoreSetDetailOutputDto
                    {
                        key = s.StoreSet.key,
                        value = s.StoreSet.type != SeoSetDataTypeEnum.textarea ? s.value!.Split(",").ToList().ConvertAll(e => e.Trim()) : s.value != null ? new List<string> { s.value } : new List<string>(),
                    }
                ).ToList();
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return output;
        }
        private async Task<StoreSetResponseMessageDto> getValuesByKeys(List<string> keys, long websiteId)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            try
            {
                var result = await db.StoreSetDetail
                    .Include(e => e.StoreSet)
                    .Where(e => !e.IsDeleted)
                    .Where(e => keys.Contains(e.StoreSet.key))
                    .Where(e => e.FK_WebsiteId == websiteId)
                    .ToArrayAsync();
                output.storeSetDetails = mapper.Map<List<StoreSetDetailOutputDto>>(result);
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return output;
        }
        private async Task<StoreSetResponseMessageDto> getValueByKey(string key, long websiteId)
        {
            StoreSetResponseMessageDto output = new StoreSetResponseMessageDto();
            try
            {
                var result = await db.StoreSetDetail
                    .Include(e => e.StoreSet)
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.StoreSet.key == key)
                    .Where(e => e.FK_WebsiteId == websiteId)
                    .FirstAsync();
                output.detailItem = mapper.Map<StoreSetDetailOutputDto>(result);
                output.detailItem.value = result.value!.Split(",").ToList().ConvertAll(e => e.Trim());
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return output;
        }
        public async Task<ResponseMessageDto> CreateOrUpdate(List<StoreSetDetailOutputDto> datas)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            long websiteId = await loginUserData.GetWebsiteId();
            long userId = await loginUserData.GetUserId();
            var keys = datas.Select(e => e.key);
            var updateItems = await db.StoreSetDetail.Include(e => e.StoreSet)
                .Where(e => !e.IsDeleted)
                .Where(e => keys.Contains(e.StoreSet.key))
                .Where(e => e.FK_WebsiteId == websiteId)
                .ToListAsync();
            try
            {
                if (updateItems.Count != 0)
                {
                    for (int i = 0; i < updateItems.Count; i++)
                    {
                        StoreSetDetailOutputDto? item = datas.Find(e => e.key == updateItems[i].StoreSet.key);
                        if (item != null)
                        {
                            mapper.Map(item, updateItems[i]);
                            updateItems[i].value = String.Join(", ", item.value!.ToArray());
                        }
                    }
                    await db.SaveChangesAsync();
                }
                if (updateItems.Count != datas.Count)
                {
                    var hasKeys = updateItems.Select(e => e.StoreSet.key).ToList();
                    var notHas = datas.Where(e => !hasKeys.Contains(e.key)).ToList();
                    var notHasKeys = notHas.Select(e => e.key).ToList();
                    if (notHas.Count != 0)
                    {
                        var data = await (from a in db.StoreSet
                                          where notHasKeys.Contains(a.key)
                                          select new StoreSetDetail
                                          {
                                              FK_WebsiteId = websiteId,
                                              CreatorUserId = userId,
                                              CreationTime = DateTime.Now,
                                              FK_StoreSetId = a.Id,
                                              IsDeleted = false,
                                              StoreSet = a
                                          }).ToListAsync();
                        if (data.Any())
                        {
                            data.ForEach(e =>
                            {
                                StoreSetDetailOutputDto? item = notHas.Find(n => n.key == e.StoreSet.key);
                                if (item != null && item.value != null)
                                {
                                    e.value = String.Join(", ", item.value.ToArray());
                                }
                            });
                            db.StoreSetDetail.AddRange(data);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }
            return output;
        }
    }
}
