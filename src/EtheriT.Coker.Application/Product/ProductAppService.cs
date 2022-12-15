using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
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
                        Discount = dto.Discount,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        permanent = dto.Permanent
                    };
                    db.Prods.Add(p);
                    db.SaveChanges();

                    Core.Models.Prod_Stock ps = new Core.Models.Prod_Stock
                    {
                        FK_Pid = p.Id,
                        FK_S1id = dto.FK_S1id,
                        FK_S2id = dto.FK_S2id,
                        Stock = dto.Stock,
                        Price = dto.Price,
                        Min_Qty = dto.Min_Qty == null ? 1 : dto.Min_Qty,
                        Safe_Qty = 5,
                        Ser_No = 500,
                    };
                    db.Prod_Stocks.Add(ps);
                }
                else
                {
                    var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();

                    if (db_p != null)
                    {
                        var db_ps = db.Prod_Stocks.Where(e => e.Id == dto.Id).FirstOrDefault();
                        if (db_ps != null)
                        {
                            db_p.FK_WebsiteId = (long)dto.FK_WebsiteId;
                            db_p.Title = dto.Title;
                            db_p.Disp_Opt = dto.Disp_Opt;
                            db_p.Ser_No = dto.Ser_No;
                            db_p.Introduction = dto.Introduction;
                            db_p.Description = dto.Description;
                            db_p.Discount = dto.Discount;
                            db_p.StartTime = dto.StartTime;
                            db_p.EndTime = dto.EndTime;
                            db_p.permanent = dto.Permanent;
                            db_p.LastModificationTime = DateTime.Now;

                            db_ps.FK_S1id = dto.FK_S1id;
                            db_ps.FK_S2id = dto.FK_S2id;
                            db_ps.Stock = dto.Stock;
                            db_ps.Price = dto.Price;
                            db_ps.Min_Qty = dto.Min_Qty == null ? 1 : dto.Min_Qty;
                        }
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
                var db_p = db.Prods;
                var db_ps = db.Prod_Stocks;

                if (db_p != null && db_ps != null)
                {
                    var dataQuery = from p in db_p
                                    where !p.IsDeleted
                                    from ps in db_ps
                                    where ps.FK_Pid == p.Id
                                    select new ProductGetAllListDto
                                    {
                                        Id = p.Id,
                                        Title = p.Title,
                                        Disp_Opt = p.Disp_Opt,
                                        Ser_No = p.Ser_No,
                                        Introduction = p.Introduction,
                                        Description = p.Description,
                                        Price = ps.Price,
                                        Discount = p.Discount,
                                        StartTime = p.StartTime,
                                        EndTime = p.EndTime,
                                        Permanent = p.permanent
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
                var db_p = db.Prods.Where(e => e.Id == Id).FirstOrDefault();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == db_p.Id).FirstOrDefault();

                if (db_p != null)
                {
                    ProductDto output = new ProductDto()
                    {
                        Id = db_p.Id,
                        Title = db_p.Title,
                        Disp_Opt = db_p.Disp_Opt,
                        Ser_No = db_p.Ser_No,
                        Introduction = db_p.Introduction,
                        Description = db_p.Description,
                        Price = db_ps.Price,
                        Discount = db_p.Discount,
                        StartTime = db_p.StartTime,
                        EndTime = db_p.EndTime,
                        Permanent = db_p.permanent,
                        Stock = db_ps.Stock == null ? 0 : db_ps.Stock,
                        Min_Qty = db_ps.Min_Qty,
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
                var db_p = db.Prods.Where(e => e.Id == id).FirstOrDefault();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == db_p.Id).FirstOrDefault();

                if (db_p != null && db_ps != null)
                {
                    ProdGetOneDto output = new ProdGetOneDto()
                    {
                        Id = db_p.Id,
                        Title = db_p.Title,
                        Introduction = db_p.Introduction,
                        Description = db_p.Description,
                        Price = db_ps.Price,
                        Discount = db_p.Discount,
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
        public async Task<List<long>> GetRandomId(int num)
        {
            try
            {
                var db_p = db.Prods.Where(e => !e.IsDeleted);
                List<long> output = new List<long>();

                do
                {
                    Random myObject = new Random();
                    int r;
                    do
                    {
                        r = myObject.Next(1, db_p.Count() + 1);
                    } while (output.IndexOf(r) > -1);
                    output.Add(r);
                } while (output.Count < 3);

                return output;
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
        public async Task<ResponseMessageDto> ClickLog(ProductLogDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_t = db.Tokens.Where(e => e.id == dto.FK_Tid).FirstOrDefault();

                Core.Models.Prod_Log pl = new Core.Models.Prod_Log
                {
                    FK_Pid = dto.FK_Pid,
                    FK_Uid = db_t.UserID,
                    FK_Tid = dto.FK_Tid,
                    Action = dto.Action,
                };
                db.Prod_Logs.Add(pl);
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
    }
}
