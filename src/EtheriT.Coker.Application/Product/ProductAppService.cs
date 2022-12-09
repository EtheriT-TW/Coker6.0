using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.Product
{
    public class ProductAppService : IProductAppService
    {
        private readonly CokerDbContext db;
        public ProductAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }

        public async Task<ResponseMessageDto> AddUp(ProductDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0)
                {
                    Core.Models.Prod p = new Core.Models.Prod
                    {
                        FK_WebsiteId = (long)dto.FK_WebsiteId,
                        Title = dto.Title,
                        Disp_Opt = dto.Disp_Opt,
                        Ser_No = dto.Ser_No,
                        Introduction = dto.Introduction,
                        Description = dto.Description,
                        Price = dto.Price,
                        Discount = dto.Discount,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        permanent = dto.Permanent
                    };
                    db.Prods.Add(p);
                }
                else
                {
                    var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_p != null)
                    {
                        db_p.FK_WebsiteId = (long)dto.FK_WebsiteId;
                        db_p.Title = dto.Title;
                        db_p.Disp_Opt = dto.Disp_Opt;
                        db_p.Ser_No = dto.Ser_No;
                        db_p.Introduction = dto.Introduction;
                        db_p.Description = dto.Description;
                        db_p.Price = dto.Price;
                        db_p.Discount = dto.Discount;
                        db_p.StartTime = dto.StartTime;
                        db_p.EndTime = dto.EndTime;
                        db_p.permanent = dto.Permanent;
                        db_p.LastModificationTime = DateTime.Now;
                    }
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
                var result = db.Prods;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted
                                    select new ProductGetAllListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Disp_Opt = e.Disp_Opt,
                                        Ser_No = e.Ser_No,
                                        Introduction = e.Introduction,
                                        Description = e.Description,
                                        Price = e.Price,
                                        Discount = e.Discount,
                                        StartTime = e.StartTime,
                                        EndTime = e.EndTime,
                                        Permanent = e.permanent
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ProductGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }

        public async Task<ProductDto> GetOne(long Id)
        {
            try
            {
                var result = db.Prods.Where(e => e.Id == Id).FirstOrDefault();

                if (result != null)
                {
                    ProductDto output = new ProductDto()
                    {
                        Id = result.Id,
                        Title = result.Title,
                        Disp_Opt = result.Disp_Opt,
                        Ser_No = result.Ser_No,
                        Introduction = result.Introduction,
                        Description = result.Description,
                        Price = result.Price,
                        Discount = result.Discount,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                        Permanent = result.permanent
                    };
                    return output;
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public async Task<ProdGetOneDto> GetDisplayOne(long id)
        {
            try
            {
                var result = db.Prods.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    ProdGetOneDto output = new ProdGetOneDto()
                    {
                        Id = result.Id,
                        Title = result.Title,
                        Introduction = result.Introduction,
                        Description = result.Description,
                        Price = result.Price
                    };
                    return output;
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public async Task<ResponseMessageDto> Delete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_ls = db.Prods.Where(e => e.Id == Id).FirstOrDefault();
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
