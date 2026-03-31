using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EtheriT.Coker.Application.Freight
{
    public class FreightAppService : IFreightAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration Configuration;
        private readonly IMapper mapper;
        public FreightAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration Configuration,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.Configuration = Configuration;
            this.mapper = mapper;
        }
        public async Task<ResponseMessageDto> AddUp(FreightDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                LogisticsSetting? ls;
                if (dto.Id == 0)
                {
                    ls = mapper.Map<LogisticsSetting>(dto);
                    ls.FK_WebsiteId = WebsiteID;
                    db.LogisticsSettings.Add(ls);
                }
                else
                {
                    ls = db.LogisticsSettings.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (ls != null)
                    {
                        mapper.Map(dto, ls);
                    }
                    else throw new Exception("查無運費資料");
                }
                await loginUserData.SaveChanges(ls);
                await SyncProdMappingsAsync(ls.Id, dto.ProdIds, WebsiteID);
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto),JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.LogisticsSettings;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                    select new FreightGetAllListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Describe =
                                            e.FreigntStatusType.ToString() + " - " +
                                            e.LogisticsType.ToString()
                                                .Replace("_", "/")
                                                .Replace("Seven", "7-11") + "，" +
                                            (e.FreigntType == FreigntTypeEnum.免運費
                                                ? e.FreigntType.ToString()
                                                : e.Freight == e.Dis_Freight
                                                    ? $"單筆計算{e.Freight}元"
                                                    : $"單筆計算{e.Freight}元(滿{e.Low_Con}元{(e.Dis_Freight == 0 ? "免運" : $"運費{e.Dis_Freight}元")})")
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無運費資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<FreightGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetLogisticsBoxAllList(DataSourceLoadOptions loadOptions) {
            try {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var dataQuery = db.LogisticsBoxs
                    .Where(x => x.FK_WebsiteId == WebsiteID)
                    .Select(x => new GetLogisticsBoxAllListInputDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsActive = x.IsActive,
                        CapacityPoint = x.CapacityPoint,
                        Sort = x.Sort
                    });
                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e) { }
            return new JsonResult(new List<FreightGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<FreightDto> GetOne(long Id)
        {
            try
            {
                var result = db.LogisticsSettings.Include(e => e.MappingLogisticsSettingAndProds).ThenInclude(e => e.Prod).Where(e => e.Id == Id && !e.IsDeleted).FirstOrDefault();

                if (result != null)
                {
                    var output = mapper.Map<FreightDto>(result);
                    return output;
                }
                else throw new Exception("查無運費資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<JsonResult> GetDisplay()
        {
            try
            {
                var webid = Configuration.GetValue<long>("WebConfig:SiteId");
                var result = db.LogisticsSettings;

                if (result != null)
                {
                    var output = from e in result
                                 where !e.IsDeleted && e.FK_WebsiteId == webid
                                 select new FreightDisplayDto
                                 {
                                     Id = e.Id,
                                     Title = e.Title,
                                     Freight = e.Freight ?? 0,
                                     Low_Con = e.Low_Con,
                                     Dis_Freight = e.Dis_Freight,
                                     Set_Default = e.Set_Default,
                                     freigntStatusType = (int)e.FreigntStatusType,
                                     Describe =
                                         e.LogisticsType.ToString()
                                             .Replace("_", "/")
                                             .Replace("Seven", "7-11") + "，" +
                                         (
                                             e.FreigntType == FreigntTypeEnum.免運費
                                                 ? e.FreigntType.ToString()
                                                 : e.Freight == e.Dis_Freight
                                                     ? $"單筆計算{e.Freight}元"
                                                     : $"單筆計算{e.Freight}元(滿{e.Low_Con}元{(e.Dis_Freight == 0 ? "免運" : $"運費{e.Dis_Freight}元")})"
                                         ),
                                     GetMap = (int)e.LogisticsType >= 8 && (int)e.LogisticsType <= 15,
                                     LogisticsSubType = GetLogisticsSubType((int)e.LogisticsType)
                                 };

                    return new JsonResult(output, new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver()
                    });
                }
                else
                {
                    throw new Exception("查無運費資料");
                }
            }
            catch (Exception ex)
            {
                // 建議可記錄 log
                Console.WriteLine(ex.Message);
            }

            return new JsonResult(new List<FreightDisplayDto>(), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            });
        }

        private static string GetLogisticsSubType(int typecode)
        {
            string LogisticsSubType = "";

            switch (typecode)
            {
                case 8:
                    LogisticsSubType = "FAMI";
                    break;
                case 9:
                    LogisticsSubType = "UNIMART";
                    break;
                case 10:
                    LogisticsSubType = "UNIMARTFREEZE";
                    break;
                case 11:
                    LogisticsSubType = "HILIFE";
                    break;
                case 12:
                    LogisticsSubType = "FAMIC2C";
                    break;
                case 13:
                    LogisticsSubType = "UNIMARTC2C";
                    break;
                case 14:
                    LogisticsSubType = "HILIFEC2C";
                    break;
                case 15:
                    LogisticsSubType = "OKMARTC2C";
                    break;
            }

            return LogisticsSubType;
        }

        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            long usetId = await loginUserData.GetUserId();

            try
            {
                var db_ls = db.LogisticsSettings.Where(e => e.Id == Id).FirstOrDefault();
                if (db_ls != null)
                {
                    db_ls.IsDeleted = true;
                    db_ls.DeletionTime = DateTime.Now;
                    db_ls.DeleterUserId = usetId;
                    db.SaveChanges();
                    output.Success = true;
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        private async Task SyncProdMappingsAsync(long lsId, IEnumerable<ProdSelectedDto>? prodIds, long websiteId)
        {
            // 去重、過濾無效
            var allSet = new HashSet<long>((prodIds ?? Enumerable.Empty<ProdSelectedDto>()).Select(x => x.FK_ProdId).Distinct());
            var newSet = new HashSet<long>((prodIds ?? Enumerable.Empty<ProdSelectedDto>()).Where(x => !x.IsDeleted).Select(x => x.FK_ProdId).Distinct());

            // （可選）安全性：確保傳入的 prod 都屬於此網站
            if (allSet.Count > 0)
            {
                var validCount = await db.Prods
                    .Where(p => p.FK_WebsiteId == websiteId && !p.IsDeleted && allSet.Contains(p.Id))
                    .Select(p => p.Id)
                    .CountAsync();

                if (validCount != allSet.Count)
                    throw new Exception("包含無效或非本網站的商品 Id。");
            }

            // 取目前資料庫的關聯
            var current = await db.MappingLogisticsSettingAndProd
                .Where(m => m.FK_LogisticsSettingId == lsId)
                .Select(m => m.FK_ProdId)
                .ToListAsync();

            var newItems = newSet.Except(current).ToList();
            var toRemove = current.Except(newSet).ToList();

            if (newItems.Count > 0)
            {
                var addEntities = newItems.Select(pid => new MappingLogisticsSettingAndProd
                {
                    FK_LogisticsSettingId = lsId,
                    FK_ProdId = pid
                });
                await db.MappingLogisticsSettingAndProd.AddRangeAsync(addEntities);
            }

            if (toRemove.Count > 0)
            {
                var delEntities = await db.MappingLogisticsSettingAndProd
                    .Where(m => m.FK_LogisticsSettingId == lsId && toRemove.Contains(m.FK_ProdId))
                    .ToListAsync();
                db.MappingLogisticsSettingAndProd.RemoveRange(delEntities);
            }

            await db.SaveChangesAsync();
        }
        public async Task<ResponseMessageDto> LogisticsBoxAddUp(GetLogisticsBoxAllListInputDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    output.ErrorCode = ErrorCodeEnum.ValidationError;
                    output.Error = "名稱不可為空";
                    output.Message = "儲存失敗";
                    return output;
                }

                LogisticsBox? entity;

                if (dto.Id == 0)
                {
                    entity = new LogisticsBox
                    {
                        FK_WebsiteId = websiteId,
                        Name = dto.Name.Trim(),
                        IsActive = dto.IsActive,
                        CapacityPoint = dto.CapacityPoint,
                        Sort = dto.Sort
                    };

                    db.LogisticsBoxs.Add(entity);
                }
                else
                {
                    entity = await db.LogisticsBoxs
                        .Where(x => x.Id == dto.Id && x.FK_WebsiteId == websiteId && !x.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (entity == null)
                    {
                        output.ErrorCode = ErrorCodeEnum.NotFound;
                        output.Error = "查無箱型資料";
                        output.Message = "儲存失敗";
                        return output;
                    }

                    entity.Name = dto.Name.Trim();
                    entity.IsActive = dto.IsActive;
                    entity.CapacityPoint = dto.CapacityPoint;
                    entity.Sort = dto.Sort;
                }

                await loginUserData.SaveChanges(entity);

                output.Success = true;
                output.Message = "儲存成功";
                output.Object = new
                {
                    id = entity.Id
                };
            }
            catch (Exception e)
            {
                output.Success = false;
                output.ErrorCode = ErrorCodeEnum.ServerError;
                output.Error = e.Message;
                output.Message = "儲存失敗";
            }

            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(dto),
                JsonConvert.SerializeObject(output)
            );

            return output;
        }

        public async Task<ResponseMessageDto> LogisticsBoxGetOne(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();

                var entity = await db.LogisticsBoxs
                    .Where(x => x.Id == Id && x.FK_WebsiteId == websiteId && !x.IsDeleted)
                    .Select(x => new GetLogisticsBoxAllListInputDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsActive = x.IsActive,
                        CapacityPoint = x.CapacityPoint,
                        Sort = x.Sort
                    })
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    output.ErrorCode = ErrorCodeEnum.NotFound;
                    output.Error = "查無箱型資料";
                    output.Message = "查詢失敗";
                    return output;
                }

                output.Success = true;
                output.Message = "查詢成功";
                output.Object = entity;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.ErrorCode = ErrorCodeEnum.ServerError;
                output.Error = e.Message;
                output.Message = "查詢失敗";
            }

            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(new { Id }),
                JsonConvert.SerializeObject(output)
            );

            return output;
        }

        public async Task<ResponseMessageDto> LogisticsBoxDelete(long Id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                long userId = await loginUserData.GetUserId();

                var entity = await db.LogisticsBoxs
                    .Where(x => x.Id == Id && x.FK_WebsiteId == websiteId && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    output.ErrorCode = ErrorCodeEnum.NotFound;
                    output.Error = "查無箱型資料";
                    output.Message = "刪除失敗";
                    return output;
                }

                entity.IsDeleted = true;
                entity.DeletionTime = DateTime.Now;
                entity.DeleterUserId = userId;

                await db.SaveChangesAsync();

                output.Success = true;
                output.Message = "刪除成功";
            }
            catch (Exception e)
            {
                output.Success = false;
                output.ErrorCode = ErrorCodeEnum.ServerError;
                output.Error = e.Message;
                output.Message = "刪除失敗";
            }

            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(new { Id }),
                JsonConvert.SerializeObject(output)
            );

            return output;
        }
    }
}
