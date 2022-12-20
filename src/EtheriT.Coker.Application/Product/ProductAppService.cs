using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EtheriT.Coker.Application.Product
{
    public class ProductAppService : IProductAppService
    {
        private readonly CokerDbContext db;
        private readonly ILoginUserDataApplication loginUserDataApplication;
        public ProductAppService(
            CokerDbContext db,
            ILoginUserDataApplication loginUserDataApplication
        )
        {
            this.db = db;
            this.loginUserDataApplication = loginUserDataApplication;
        }

        public async Task<ResponseMessageDto> ProductAddUp(ProductDto dto)
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
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        permanent = dto.Permanent
                    };
                    db.Prods.Add(p);
                    db.SaveChanges();
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
        public async Task<ResponseMessageDto> ProductStockAddUp(ProductStockDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0)
                {
                    Core.Models.Prod_Stock ps = new Core.Models.Prod_Stock
                    {
                        FK_Pid = dto.Pid,
                        FK_S1id = dto.FK_S1id,
                        FK_S2id = dto.FK_S2id,
                        Price = dto.Price,
                        Stock = dto.Stock,
                        Min_Qty = dto.Stock,
                        Alert_Qty = dto.Alert_Qty
                    };
                    db.Prod_Stocks.Add(ps);
                    db.SaveChanges();
                }
                else
                {
                    var db_ps = db.Prod_Stocks.Where(e => e.Id == dto.Id).FirstOrDefault();

                    if (db_ps != null)
                    {
                        db_ps.FK_Pid = dto.Pid;
                        db_ps.FK_S1id = dto.FK_S1id;
                        db_ps.FK_S2id = dto.FK_S2id;
                        db_ps.Price = dto.Price;
                        db_ps.Stock = dto.Stock;
                        db_ps.Min_Qty = dto.Stock;
                        db_ps.LastModificationTime = DateTime.Now;
                        //db_ps.Alert_Qty = dto.Alert_Qty;
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
                long webid = await loginUserDataApplication.GetWebsiteId();
                var db_ps = db.Prod_Stocks;
                var db_p = db.Prods;

                if (db_ps != null && db_p != null)
                {
                    var dataQuery = from ps in db_ps
                                    where !ps.IsDeleted
                                    join p in db_p on ps.FK_Pid equals p.Id
                                    where !p.IsDeleted && p.FK_WebsiteId== webid
                                    group ps by new { p.Id, p.Title, p.Disp_Opt, p.Ser_No, p.StartTime, p.EndTime, p.permanent } into s
                                    select new ProductGetAllListDto
                                    {
                                        Id = s.Key.Id,
                                        Title = s.Key.Title,
                                        Disp_Opt = s.Key.Disp_Opt,
                                        Ser_No = s.Key.Ser_No,
                                        Price = s.Min(e => e.Price) == s.Max(e => e.Price) ? s.Min(e => e.Price).ToString() : s.Min(e => e.Price) + " ~ " + s.Max(e => e.Price),
                                        StartTime = s.Key.StartTime,
                                        EndTime = s.Key.EndTime,
                                        Permanent = s.Key.permanent
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
        public async Task<ProductDto> ProdGetOne(long Id)
        {
            try
            {
                var db_p = db.Prods.Where(e => e.Id == Id).FirstOrDefault();
                //var db_ps = db.Prod_Stocks.Where(e => e.Id == db_p.Id).FirstOrDefault();

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
                        StartTime = db_p.StartTime,
                        EndTime = db_p.EndTime,
                        Permanent = db_p.permanent,
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
        public async Task<List<ProductStockDto>> ProdStockGet(long PId)
        {
            try
            {
                var db_ps = db.Prod_Stocks;

                if (db_ps != null)
                {
                    var output = await (from ps in db_ps
                                        where !ps.IsDeleted && ps.FK_Pid == PId
                                        select new ProductStockDto
                                        {
                                            Id = ps.Id,
                                            FK_S1id = ps.FK_S1id,
                                            FK_S2id = ps.FK_S2id,
                                            Price = ps.Price,
                                            Min_Qty = ps.Min_Qty,
                                            Stock = ps.Stock,
                                            Alert_Qty = ps.Alert_Qty
                                        }).ToListAsync();
                    return output;
                }
                else throw new Exception("查無資料");
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
        public async Task<List<ProdIdTitleDto>> GetSpecType(long webid)
        {
            try
            {
                var db_pst = db.Prod_Spec_Types;

                if (db_pst != null)
                {
                    var output = await (from pst in db_pst
                                        where !pst.IsDeleted && pst.FK_WebsiteId == webid
                                        select new ProdIdTitleDto
                                        {
                                            Id = pst.Id,
                                            Title = pst.Type
                                        }).ToListAsync();
                    return output;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<List<ProdIdTitleDto>> GetSpecDetail(long typeid)
        {
            try
            {
                var db_ps = db.Prod_Specs;

                if (db_ps != null)
                {
                    var output = await (from ps in db_ps
                                        where !ps.IsDeleted && ps.FK_Tid == typeid
                                        select new ProdIdTitleDto
                                        {
                                            Id = ps.Id,
                                            Title = ps.Title
                                        }).ToListAsync();
                    return output;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<ResponseMessageDto> ProdDelete(long Id)
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
