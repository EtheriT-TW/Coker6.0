using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Specification;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Specification;

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
            var data = JsonConvert.DeserializeObject<SpecTypeListDto>(dto.Values);
            data.Id = dto.Key == null ? 0 : (long)dto.Key;

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
            var data = JsonConvert.DeserializeObject<SpecSpecListDto>(dto.Values);
            data.Id = dto.Key == null ? 0 : (long)dto.Key;

            try
            {
                output = await this.SpecAddUp(data);

                return output;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
                return output;
            }
        }
        public async Task<ResponseMessageDto> SpecAddUp(SpecSpecListDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long userId = await loginUserData.GetUserId();
                var psid = dto.Id;

                if (dto.Id == 0)
                {
                    Core.Models.Prod_Spec ps = new Core.Models.Prod_Spec
                    {
                        FK_Tid = dto.FK_Tid,
                        Title = dto.Title,
                        CreatorUserId = userId,
                    };
                    db.Prod_Specs.Add(ps);
                    db.SaveChanges();
                    psid = ps.Id;
                }
                else
                {
                    var db_ps = await db.Prod_Specs.Where(e => e.Id == dto.Id && !e.IsDeleted).FirstOrDefaultAsync();
                    if (db_ps != null)
                    {
                        db_ps.Title = dto.Title == null ? db_ps.Title : dto.Title;
                        db_ps.FK_Tid = dto.FK_Tid == 0 ? db_ps.FK_Tid : dto.FK_Tid;
                        db_ps.LastModifierUserId = userId;
                        db_ps.LastModificationTime = DateTime.Now;
                        db.SaveChanges();
                    }
                }

                output.Success = true;
                output.Message = psid.ToString();
                return output;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
                return output;
            }
        }
        public async Task<JsonResult> GetAllTypeList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from pst in db.Prod_Spec_Types
                                where !pst.IsDeleted && pst.FK_WebsiteId == webid
                                select new SpecTypeListDto
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

            return new JsonResult(new List<SpecTypeListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
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
                                select new SpecSpecListDto
                                {
                                    Id = ps.Id,
                                    FK_Tid = pst.Id,
                                    Type = pst.Type,
                                    Title = ps.Title
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<SpecSpecListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<SpecTypeListDto>> GetPickTypeList()
        {
            var output = new List<SpecTypeListDto>();

            try
            {
                var websiteid = await loginUserData.GetWebsiteId();

                var db_pt = await db.Prod_Spec_Types.Where(e => e.FK_WebsiteId == websiteid && !e.IsDeleted).ToListAsync();
                if (db_pt.Count > 0)
                {
                    for (var pt_i = 0; pt_i < db_pt.Count; pt_i++)
                    {
                        output.Add(new SpecTypeListDto()
                        {
                            Id = db_pt[pt_i].Id,
                            Type = db_pt[pt_i].Type,
                        });
                    }
                }
            }
            catch (Exception err)
            {
            }
            return output;

        }
        public async Task<List<SpecTypePickListDto>> GetPickSpecList()
        {
            var output = new List<SpecTypePickListDto>();

            try
            {
                var websiteid = await loginUserData.GetWebsiteId();

                var db_pt = await db.Prod_Spec_Types.Where(e => e.FK_WebsiteId == websiteid && !e.IsDeleted).ToListAsync();
                if (db_pt.Count > 0)
                {
                    for (var pt_i = 0; pt_i < db_pt.Count; pt_i++)
                    {
                        var speclist = new List<SpecSpecPickListDto>();
                        var db_ps = await db.Prod_Specs.Where(e => e.FK_Tid == db_pt[pt_i].Id && !e.IsDeleted).ToListAsync();
                        if (db_ps.Count > 0)
                        {
                            for (var ps_i = 0; ps_i < db_ps.Count; ps_i++)
                            {
                                speclist.Add(new SpecSpecPickListDto()
                                {
                                    Id = db_ps[ps_i].Id,
                                    Title = db_ps[ps_i].Title,
                                });
                            }
                        }
                        output.Add(new SpecTypePickListDto()
                        {
                            Id = db_pt[pt_i].Id,
                            Type = db_pt[pt_i].Type,
                            Specs = speclist,
                        });
                    }
                }
            }
            catch (Exception err)
            {
            }
            return output;

        }
        // 規格類型刪除前檢查底下是否有規格
        public async Task<ResponseMessageDto> CheckRelatSpec(long Id)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var specs = await db.Prod_Specs.Where(e => e.FK_Tid == Id).ToListAsync();
                if (specs.Any())
                {
                    var specnum = specs.Count > 1 ? (specs.Count - 1).ToString() : ",";
                    response.Message = $"{specs[0].Title},{specnum}";
                }
                else response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
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
        // 規格刪除前檢查是否有商品綁定
        public async Task<ResponseMessageDto> CheckRelatProd(long Id)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var prodids = await db.Prod_Stocks.Where(e => e.FK_S1id == Id || e.FK_S2id == Id).Select(e => e.FK_Pid).Distinct().ToListAsync();
                if (prodids.Any())
                {
                    var prods = await db.Prods.Where(e => prodids.Contains(e.Id)).ToListAsync();
                    if (prods.Any())
                    {
                        response.Message = $"{prods[0].Id},{prods[0].Title}";
                        response.Message += prods.Count > 1 ? $",{prods.Count - 1}" : ",";
                    }
                    else response.Success = true;
                }
                else response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> SpecDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto prodasso_output = new ResponseMessageDto() { Success = true };

            try
            {
                long userId = await loginUserData.GetUserId();
                long websiteid = await loginUserData.GetWebsiteId();

                var db_ps = await db.Prod_Specs.Where(e => e.Id == Id).FirstOrDefaultAsync();

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
                                p.Visible = false;
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
