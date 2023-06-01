using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Specification;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Application.Specification
{
    public class SpecificationAppService : ISpecificationAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public SpecificationAppService(
            CokerDbContext db,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }
        public async Task<ResponseMessageDto> TypeAddUp(DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<ProductSpecListDto>(dto.Values);

            try
            {
                long userId = await loginUserData.GetUserId();
                long webid = await loginUserData.GetWebsiteId();

                if (data.Id == 0)
                {
                    Core.Models.Prod_Spec_Type st = new Core.Models.Prod_Spec_Type
                    {
                        FK_WebsiteId = webid,
                        Type = data.Type,
                        CreatorUserId = userId,
                    };
                    db.Prod_Spec_Types.Add(st);
                    await loginUserData.SaveChanges(st);
                }
                else
                {
                    var db_st = await db.Prod_Spec_Types.Where(e => e.Id == data.Id && !e.IsDeleted && e.FK_WebsiteId == webid).FirstOrDefaultAsync();
                    if (db_st != null)
                    {
                        db_st.Type = data.Type;
                        db_st.LastModifierUserId = userId;
                        db_st.LastModificationTime = DateTime.Now;
                        await loginUserData.SaveChanges(db_st);
                    }
                }

                output.Success = true;
                return output;
            }
            catch (Exception e)
            {
                output.Error = e.Message;
                output.Success = false;
                return output;
            }
        }
        public async Task<ResponseMessageDto> SpecAddUp(DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<ProductSpecListDto>(dto.Values);

            try
            {
                long userId = await loginUserData.GetUserId();
                long webid = await loginUserData.GetWebsiteId();

                long tid = 0;

                if (data.Type != null)
                {
                    tid = db.Prod_Spec_Types.Where(e => e.Type == data.Type && !e.IsDeleted).Select(e => e.Id).FirstOrDefault();

                    if (tid == null || tid == 0)
                    {
                        Core.Models.Prod_Spec_Type pst = new Core.Models.Prod_Spec_Type
                        {
                            FK_WebsiteId = webid,
                            Type = data.Type,
                            CreatorUserId = userId,
                        };
                        db.Prod_Spec_Types.Add(pst);
                        db.SaveChanges();
                        tid = pst.Id;
                    }
                }

                if (dto.Key == null)
                {
                    Core.Models.Prod_Spec ps = new Core.Models.Prod_Spec
                    {
                        FK_Tid = tid,
                        Title = data.Title,
                        CreatorUserId = userId,
                    };
                    db.Prod_Specs.Add(ps);
                    db.SaveChanges();
                }
                else
                {
                    var db_ps = db.Prod_Specs.Where(e => e.Id == dto.Key).FirstOrDefault();

                    if (db_ps != null)
                    {
                        if (data.Type != null) { db_ps.FK_Tid = tid; }
                        if (data.Title != null) { db_ps.Title = data.Title; }
                        db_ps.LastModifierUserId = userId;
                        db_ps.LastModificationTime = DateTime.Now;
                        db.SaveChanges();
                    }
                }

                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<JsonResult> GetAllTypeList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from pst in db.Prod_Spec_Types
                                where !pst.IsDeleted && pst.FK_WebsiteId == webid
                                select new ProductSpecListDto
                                {
                                    Id = pst.Id,
                                    Type = pst.Type
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ProductSpecListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<JsonResult> GetAllSpecList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from pst in db.Prod_Spec_Types
                                where !pst.IsDeleted && pst.FK_WebsiteId == webid
                                from ps in db.Prod_Specs
                                where !ps.IsDeleted && ps.FK_Tid == pst.Id
                                select new ProductSpecListDto
                                {
                                    Id = ps.Id,
                                    Type = pst.Type,
                                    Title = ps.Title
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ProductSpecListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> TypeDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto specOutput = new ResponseMessageDto() { Success = true };

            try
            {
                long userId = await loginUserData.GetUserId();
                long websiteid = await loginUserData.GetWebsiteId();

                var db_st = await db.Prod_Spec_Types.Where(e => e.Id == Id && !e.IsDeleted && e.FK_WebsiteId == websiteid).FirstOrDefaultAsync();

                if (db_st != null)
                {
                    var db_ps = await db.Prod_Specs.Where(e => e.FK_Tid == db_st.Id && !e.IsDeleted).ToListAsync();

                    if (db_ps != null)
                    {
                        foreach (var ps in db_ps)
                        {
                            if (specOutput.Success)
                            {
                                specOutput = await this.SpecDelete(ps.Id);
                            }
                        }
                    }

                    db_st.IsDeleted = true;
                    db_st.DeletionTime = DateTime.Now;
                    db_st.DeleterUserId = userId;
                    db.SaveChanges();
                }
                output.Success = specOutput.Success;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> SpecDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto prodasso_output = new ResponseMessageDto() { Success = true };

            try
            {
                long userId = await loginUserData.GetUserId();
                long websiteid = await loginUserData.GetWebsiteId();

                var db_ps = await db.Prod_Specs.Where(e => e.Id == Id && !e.IsDeleted).FirstOrDefaultAsync();

                if (db_ps != null)
                {
                    db_ps.IsDeleted = true;
                    db_ps.DeletionTime = DateTime.Now;
                    db_ps.DeleterUserId = userId;
                    db.SaveChanges();

                    prodasso_output = await this.ProdAssocDelete(db_ps.Id);
                }
                output.Success = prodasso_output.Success;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> ProdAssocDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long userId = await loginUserData.GetUserId();
                long websiteid = await loginUserData.GetWebsiteId();

                var db_ps = await db.Prod_Stocks.Where(e => (e.FK_S1id == Id || e.FK_S2id == Id) && !e.IsDeleted).ToListAsync();
                if (db_ps.Count > 0)
                {
                    foreach (var ps in db_ps)
                    {
                        ps.IsDeleted = true;
                        ps.DeletionTime = DateTime.Now;
                        ps.DeleterUserId = userId;
                        db.SaveChanges();

                        var p = await db.Prods.Where(e => e.Id == ps.FK_Pid && e.FK_WebsiteId == websiteid && !e.IsDeleted).FirstOrDefaultAsync();
                        if (p != null)
                        {
                            var remain_ps = await db.Prod_Stocks.Where(e => e.FK_Pid == p.Id && !e.IsDeleted).ToListAsync();
                            if (remain_ps.Count == 0)
                            {
                                p.Disp_Opt = false;
                                db.SaveChanges();
                            }
                        }
                    }
                }

                output.Success = true;
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
