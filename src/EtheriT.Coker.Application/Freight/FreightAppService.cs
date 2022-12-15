using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.Member;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.Marquee;

namespace EtheriT.Coker.Application.Freight
{
    public class FreightAppService : IFreightAppService
    {
        private readonly CokerDbContext db;
        public FreightAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<ResponseMessageDto> AddUp(FreightDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0)
                {
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_t != null)
                    {
                        Core.Models.LogisticsSetting ls = new Core.Models.LogisticsSetting
                        {
                            FK_WebsiteId = (long)dto.FK_WId,
                            Title = dto.Title,
                            PreserveType = dto.PreserveType,
                            LogisticsType = dto.LogisticsType,
                            FreigntType = dto.FreigntType,
                            Freight = dto.Freight,
                            Low_Con = dto.Low_Con,
                            Dis_Freight = dto.Dis_Freight,
                            Set_Default = dto.Set_Default,
                            CreatorUserId = (long)db_t.UserID,
                        };
                        db.LogisticsSettings.Add(ls);
                    }
                    else throw new Exception("查無會員資料");
                }
                else
                {
                    var db_ls = db.LogisticsSettings.Where(e => e.Id == dto.Id).FirstOrDefault();
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_ls != null && db_t != null)
                    {
                        db_ls.Title = dto.Title;
                        db_ls.PreserveType = dto.PreserveType;
                        db_ls.LogisticsType = dto.LogisticsType;
                        db_ls.FreigntType = dto.FreigntType;
                        db_ls.Freight = dto.Freight;
                        db_ls.Low_Con = dto.Low_Con;
                        db_ls.Dis_Freight = dto.Dis_Freight;
                        db_ls.Set_Default = dto.Set_Default;
                        db_ls.Set_Default = dto.Set_Default;

                        db_ls.LastModificationTime = DateTime.Now;
                        db_ls.LastModifierUserId = db_t.UserID;
                    }
                    else throw new Exception("查無資料");
                }
                db.SaveChanges();
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.LogisticsSettings;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted
                                    select new FreightGetAllListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Describe = ((PreserveTypeEnum)e.PreserveType) + " - " +
                                        ((ShippingTypeEnum)e.LogisticsType).ToString().Replace("_", "/").Replace("Seven", "7-11") + "，" +
                                        (e.FreigntType == 1 ? "免運費" : "單筆運費" + e.Freight + "元(滿" + e.Low_Con + "元" + (e.Dis_Freight == 0 ? "免運)" : "運費" + e.Dis_Freight + "元)")),
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
        public async Task<FreightDto> GetOne(long Id)
        {
            try
            {
                var result = db.LogisticsSettings.Where(e => e.Id == Id && !e.IsDeleted).FirstOrDefault();

                if (result != null)
                {
                    FreightDto output = new FreightDto()
                    {
                        Id = result.Id,
                        Title = result.Title,
                        PreserveType = result.PreserveType,
                        LogisticsType = result.LogisticsType,
                        FreigntType = result.FreigntType,
                        Freight = result.Freight,
                        Low_Con = result.Low_Con,
                        Dis_Freight = result.Dis_Freight,
                        Set_Default = result.Set_Default
                    };
                    return output;
                }
                else throw new Exception("查無運費資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<JsonResult> GetDisplay(long webid)
        {
            try
            {
                var result = db.LogisticsSettings;

                if (result != null)
                {
                    var output = from e in result
                                 where !e.IsDeleted && e.FK_WebsiteId == webid
                                 select new FreightDisplayDto
                                 {
                                     Id = e.Id,
                                     Title = e.Title,
                                     Freight = e.Freight == null ? 0 : e.Freight,
                                     Low_Con = e.Low_Con,
                                     Dis_Freight= e.Dis_Freight,
                                     Set_Default = e.Set_Default,
                                     Describe = ((PreserveTypeEnum)e.PreserveType) + " - " +
                                                ((ShippingTypeEnum)e.LogisticsType).ToString().Replace("_", "/").Replace("Seven", "7-11") + "，" +
                                                (e.FreigntType == 1 ? "免運費" : "單筆運費" + e.Freight + "元(滿" + e.Low_Con + "元" + (e.Dis_Freight == 0 ? "免運)" : "運費" + e.Dis_Freight + "元)")),
                                 };
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無運費資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<FreightDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_ls = db.LogisticsSettings.Where(e => e.Id == Id).FirstOrDefault();
                if (db_ls != null)
                {
                    db_ls.IsDeleted = true;
                    db_ls.DeletionTime = DateTime.Now;
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
    }
}
