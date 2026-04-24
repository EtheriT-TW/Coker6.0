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
        private void NormalizeDiscountFreight(LogisticsSetting ls)
        {
            var type = ls.DiscountFreightType;
            var lowCon = ls.Low_Con ?? 0;
            var disFreight = ls.Dis_Freight ?? 0;

            // 1️⃣ 沒有折抵 → 全清
            if (type == null)
            {
                ls.Dis_Freight = null;
                return;
            }

            // 2️⃣ 無門檻 → 不套用
            if (lowCon <= 0)
            {
                ls.DiscountFreightType = null;
                ls.Dis_Freight = null;
                return;
            }

            // 3️⃣ 折抵金額 <= 0 → 視為免運（統一）
            if (disFreight <= 0)
            {
                ls.Dis_Freight = 0;

                // 👉 統一為「指定折抵後運費」
                ls.DiscountFreightType = DiscountFreightType.指定折抵後運費;

                return;
            }

            // 4️⃣ 單筆計費限制
            if (ls.FreightType == FreightTypeEnum.單筆計算)
            {
                if (ls.Freight.HasValue && disFreight > ls.Freight.Value)
                {
                    ls.Dis_Freight = ls.Freight.Value;
                }
            }

            // 5️⃣ 已箱計費 → 不限制（讓計算端處理）
        }
        public async Task<ResponseMessageDto> AddUp(FreightDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long websiteId = await loginUserData.GetWebsiteId();
                LogisticsSetting? ls;

                if (dto.Id == null || dto.Id == 0)
                {
                    ls = mapper.Map<LogisticsSetting>(dto);
                    ls.FK_WebsiteId = websiteId;

                    db.LogisticsSettings.Add(ls);
                }
                else
                {
                    ls = await db.LogisticsSettings
                        .Include(x => x.logisticsBoxFees)
                        .FirstOrDefaultAsync(e => e.Id == dto.Id && !e.IsDeleted);

                    if (ls == null)
                        throw new Exception("查無運費資料");

                    mapper.Map(dto, ls);
                }

                NormalizeDiscountFreight(ls);

                await loginUserData.SaveChanges(ls);

                await SyncProdMappingsAsync(ls.Id, dto.ProdIds, websiteId);
                await SyncLogisticsBoxFeesAsync(ls.Id, dto.LogisticsBoxFees, websiteId, dto.FreightType);

                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            await loginUserData.SetLogs(
                JsonConvert.SerializeObject(dto),
                JsonConvert.SerializeObject(output)
            );

            return output;
        }
        private async Task SyncLogisticsBoxFeesAsync(
            long logisticsSettingId,
            List<LogisticsBoxFeeDto>? dtoFees,
            long websiteId,
            FreightTypeEnum freightType)
        {
            dtoFees ??= new List<LogisticsBoxFeeDto>();

            var dbFees = await db.LogisticsBoxFees
                .Where(x => x.FK_LogisticsSettingId == logisticsSettingId && !x.IsDeleted)
                .ToListAsync();

            // 若不是以箱計費，直接把既有明細刪除
            if (freightType != FreightTypeEnum.依箱計費)
            {
                foreach (var fee in dbFees)
                {
                    fee.IsDeleted = true;
                }
                await loginUserData.SaveChanges(dbFees);
                return;
            }

            // 驗證
            if (!dtoFees.Any())
                throw new Exception("以箱計費至少需設定一筆箱型費用。");

            if (dtoFees.Any(x => x.Fee <= 0))
                throw new Exception("箱型費用不可小於等於 0。");

            var duplicatedBoxIds = dtoFees
                .GroupBy(x => x.FK_LogisticsBoxId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedBoxIds.Any())
                throw new Exception("箱型費用設定不可重複。");

            // 驗證箱型是否屬於本站且存在
            var boxIds = dtoFees.Select(x => x.FK_LogisticsBoxId).Distinct().ToList();

            var validBoxIds = await db.LogisticsBoxs
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted && boxIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var invalidBoxIds = boxIds.Except(validBoxIds).ToList();
            if (invalidBoxIds.Any())
                throw new Exception("包含無效的箱型資料。");

            // 先處理更新 / 新增
            foreach (var dtoFee in dtoFees)
            {
                var exist = dbFees.FirstOrDefault(x => x.FK_LogisticsBoxId == dtoFee.FK_LogisticsBoxId);

                if (exist != null)
                {
                    exist.Fee = dtoFee.Fee;
                    exist.IsDeleted = false;
                }
                else
                {
                    db.LogisticsBoxFees.Add(new LogisticsBoxFee
                    {
                        FK_LogisticsSettingId = logisticsSettingId,
                        FK_LogisticsBoxId = dtoFee.FK_LogisticsBoxId,
                        Fee = dtoFee.Fee
                    });
                }
            }

            // 刪除不存在於 dto 的舊資料
            var dtoBoxIds = dtoFees.Select(x => x.FK_LogisticsBoxId).ToHashSet();

            foreach (var dbFee in dbFees)
            {
                if (!dtoBoxIds.Contains(dbFee.FK_LogisticsBoxId))
                {
                    dbFee.IsDeleted = true;
                }
            }
            await loginUserData.SaveChanges(dbFees);
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long websiteId = await loginUserData.GetWebsiteId();

                var data = await db.LogisticsSettings
                    .AsNoTracking()
                    .Include(e => e.logisticsBoxFees)
                        .ThenInclude(e => e.logisticsBox)
                    .Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                    .ToListAsync();

                var mapped = mapper.Map<List<FreightGetAllListDto>>(data);

                var output = DataSourceLoader.Load(mapped, loadOptions);

                return new JsonResult(output, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
            catch (Exception)
            {
            }

            return new JsonResult(new List<FreightGetAllListDto>(), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            });
        }
        private async Task<IQueryable<GetLogisticsBoxAllListInputDto>> GetLogisticsBoxBaseQueryAsync()
        {
            long websiteId = await loginUserData.GetWebsiteId();

            return db.LogisticsBoxs
                .Where(x => x.FK_WebsiteId == websiteId)
                .Select(x => new GetLogisticsBoxAllListInputDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    CapacityPoint = x.CapacityPoint,
                    Sort = x.Sort
                });
        }
        public async Task<JsonResult> GetLogisticsBoxAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var query = await GetLogisticsBoxBaseQueryAsync();
                var output = await DataSourceLoader.LoadAsync(query, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
            catch (Exception)
            {
                return new JsonResult(new List<GetLogisticsBoxAllListInputDto>(), new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
        }
        public async Task<JsonResult> GetLogisticsBoxSelectList(DataSourceLoadOptions loadOptions, string? ids = null)
        {
            try
            {
                List<long> selectedIds = new();

                if (!string.IsNullOrWhiteSpace(ids))
                {
                    selectedIds = ids
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => long.TryParse(x, out var v) ? v : (long?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();
                }

                var baseQuery = await GetLogisticsBoxBaseQueryAsync();

                var query = baseQuery
                    .Where(x => x.IsActive)
                    .Select(x => new GetLogisticsBoxAllSelectDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsActive = x.IsActive,
                        CapacityPoint = x.CapacityPoint,
                        Sort = x.Sort,
                        IsSelected = selectedIds.Contains(x.Id)
                    })
                    .OrderBy(x => x.Sort)
                    .ThenBy(x => x.CapacityPoint)
                    .ThenBy(x => x.Name);

                var output = await DataSourceLoader.LoadAsync(query, loadOptions);

                return new JsonResult(output, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
            catch (Exception ex)
            {
                // 建議至少 log
                // logger.LogError(ex, "GetLogisticsBoxSelectList failed");

                return new JsonResult(new List<GetLogisticsBoxAllSelectDto>(), new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
        }
        public async Task<FreightDto> GetOne(long Id)
        {
            try
            {
                var result = db.LogisticsSettings.Include(e => e.MappingLogisticsSettingAndProds).ThenInclude(e => e.Prod).Include(e => e.logisticsBoxFees).ThenInclude(e => e.logisticsBox).Where(e => e.Id == Id && !e.IsDeleted).FirstOrDefault();

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

                var data = await db.LogisticsSettings
                    .AsNoTracking()
                    .Include(e => e.logisticsBoxFees)
                        .ThenInclude(e => e.logisticsBox)
                    .Where(e => !e.IsDeleted && e.FK_WebsiteId == webid)
                    .Where(e => e.FreightStatusType != FreightStatusTypeEnum.停用)
                    .ToListAsync();

                var output = mapper.Map<List<FreightDisplayDto>>(data);

                return new JsonResult(output, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
            catch (Exception ex)
            {
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
        public async Task<bool> RequiresLogisticsBoxAsync()
        {
            long websiteId = await loginUserData.GetWebsiteId();

            return await db.LogisticsSettings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.FK_WebsiteId == websiteId &&
                    x.FreightStatusType != FreightStatusTypeEnum.停用 &&
                    x.FreightType == FreightTypeEnum.依箱計費 &&
                    x.logisticsBoxFees.Any()
                );
        }
    }
}
