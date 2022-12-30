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
        public async Task<ResponseMessageDto> AddUp(DevExpressDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            var data = JsonConvert.DeserializeObject<ProductSpecListDto>(dto.Values);

            try
            {
                long usetId = await loginUserData.GetUserId();
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
                            CreatorUserId = usetId,
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
                        CreatorUserId = usetId,
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
                        db_ps.LastModifierUserId = usetId;
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
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
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
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_ps = db.Prod_Specs.Where(e => e.Id == Id).FirstOrDefault();
                if (db_ps != null)
                {
                    db_ps.IsDeleted = true;
                    db_ps.DeletionTime = DateTime.Now;
                    db_ps.DeleterUserId = usetId;
                    db.SaveChanges();

                    var check_dbps = db.Prod_Specs.Where(e => !e.IsDeleted && e.FK_Tid == db_ps.FK_Tid).FirstOrDefault();
                    if (check_dbps == null)
                    {
                        var db_pst = db.Prod_Spec_Types.Where(e => e.Id == db_ps.FK_Tid).FirstOrDefault();
                        if (db_pst != null)
                        {
                            db_pst.IsDeleted = true;
                            db_pst.DeletionTime = DateTime.Now;
                            db_pst.DeleterUserId = usetId;
                            db.SaveChanges();
                        }
                    }

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
