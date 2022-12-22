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
using EtheriT.Coker.Application.Shared.Dto;

namespace EtheriT.Coker.Application.Product
{
    public class ProductAppService : IProductAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        public ProductAppService(
            CokerDbContext db,
            LoginUserData loginUserData
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }

        public async Task<ResponseMessageDto> ProductAddUp(ProductDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0)
                {
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    long WebsiteID = await loginUserDataApplication.GetWebsiteId();
                    if (db_t != null)
                    {
                        Core.Models.Prod p = new Core.Models.Prod
                        {
                            FK_WebsiteId = WebsiteID,
                            Title = dto.Title,
                            Disp_Opt = dto.Disp_Opt,
                            Ser_No = dto.Ser_No,
                            Introduction = dto.Introduction,
                            Description = dto.Description,
                            StartTime = dto.StartTime,
                            EndTime = dto.EndTime,
                            permanent = dto.Permanent,
                            CreatorUserId = (long)db_t.UserID
                        };
                        db.Prods.Add(p);
                        db.SaveChanges();
                        output.Message = p.Id.ToString();
                    }
                }
                else
                {
                    var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_p != null && db_t != null)
                    {
                        db_p.Title = dto.Title;
                        db_p.Disp_Opt = dto.Disp_Opt;
                        db_p.Ser_No = dto.Ser_No;
                        db_p.Introduction = dto.Introduction;
                        db_p.Description = dto.Description;
                        db_p.StartTime = dto.StartTime;
                        db_p.EndTime = dto.EndTime;
                        db_p.permanent = dto.Permanent;
                        db_p.LastModificationTime = DateTime.Now;
                        db_p.LastModifierUserId = db_t.UserID;
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
        public async Task<ResponseMessageDto> StockAddUp(ProductStockDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                if (dto.Id == 0)
                {
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                    if (db_t != null)
                    {
                        Core.Models.Prod_Stock ps = new Core.Models.Prod_Stock
                        {
                            FK_Pid = dto.Pid,
                            FK_S1id = dto.FK_S1id,
                            FK_S2id = dto.FK_S2id,
                            Price = dto.Price,
                            Stock = dto.Stock,
                            Min_Qty = dto.Stock,
                            Alert_Qty = dto.Alert_Qty,
                            CreatorUserId = (long)db_t.UserID,
                        };
                        db.Prod_Stocks.Add(ps);
                        db.SaveChanges();
                    }
                }
                else
                {
                    var db_ps = db.Prod_Stocks.Where(e => e.Id == dto.Id).FirstOrDefault();
                    var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();

                    if (db_ps != null && db_t != null)
                    {
                        db_ps.FK_Pid = dto.Pid;
                        db_ps.FK_S1id = dto.FK_S1id;
                        db_ps.FK_S2id = dto.FK_S2id;
                        db_ps.Price = dto.Price;
                        db_ps.Stock = dto.Stock;
                        db_ps.Min_Qty = dto.Stock;
                        db_ps.LastModificationTime = DateTime.Now;
                        db_ps.Alert_Qty = dto.Alert_Qty;
                        db_ps.LastModifierUserId = db_t.UserID;
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
                long webid = await loginUserData.GetWebsiteId();
                var db_ps = db.Prod_Stocks;
                var db_p = db.Prods;

                if (db_ps != null && db_p != null)
                {
                    var dataQuery = from ps in db_ps
                                    where !ps.IsDeleted
                                    join p in db_p on ps.FK_Pid equals p.Id
                                    where !p.IsDeleted && p.FK_WebsiteId == webid
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
        public async Task<ProductDto> GetProdDataOne(long Id)
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
        public async Task<List<ProductStockDto>> GetStockDataAll(long PId)
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
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public async Task<List<ProductStockDto>> GetDisplayStock(long id)
        {
            try
            {
                var output = await (from ps in db.Prod_Stocks
                                    where ps.FK_Pid == id && !ps.IsDeleted
                                    orderby ps.Price ascending
                                    select new ProductStockDto
                                    {
                                        Id = ps.Id,
                                        FK_S1id = ps.FK_S1id,
                                        FK_S2id = ps.FK_S2id,
                                        Price = ps.Price,
                                        Stock = ps.Stock,
                                        Min_Qty = ps.Min_Qty,
                                    }).ToListAsync();

                var db_spt = db.Prod_Spec_Types.ToList();
                var db_sp = db.Prod_Specs.ToList();

                foreach (var item in output)
                {
                    item.S1_Title = db_spt[0].Type;
                    item.S1_Name = item.FK_S1id == 0 ? "" : db_sp[(int)item.FK_S1id - 1].Title;
                    item.S2_Title = db_spt[1].Type;
                    item.S2_Name = item.FK_S2id == 0 ? "" : db_sp[(int)item.FK_S2id - 1].Title;
                }

                return output;
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public async Task<ProdGetDisplayDto> GetDisplaySimple(long id)
        {
            try
            {
                var db_p = db.Prods.Where(e => e.Id == id).FirstOrDefault();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == db_p.Id).FirstOrDefault();

                if (db_p != null && db_ps != null)
                {
                    ProdGetDisplayDto output = new ProdGetDisplayDto()
                    {
                        Id = db_p.Id,
                        Title = db_p.Title,
                        Introduction = db_p.Introduction,
                        Description = db_p.Description,
                        Link = "/Toilet/" + db_p.Id,
                        Image = "/images/product/pro_0" + db_p.Id + ".png",
                        Price = db_ps.Price.ToString(),
                    };
                    return output;
                }
                else throw new Exception("查無資料");
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public async Task<JsonResult> GetRandomDIsplay(long webid, int num)
        {
            try
            {
                var result = await (from p in db.Prods
                                    where !p.IsDeleted && p.Disp_Opt && p.FK_WebsiteId == webid
                                    where p.permanent || (DateTime.Compare(DateTime.Now, (DateTime)p.StartTime) > 0 && DateTime.Compare(DateTime.Now, (DateTime)p.EndTime) < 0)
                                    orderby p.Ser_No
                                    join ps in db.Prod_Stocks.Where(e => !e.IsDeleted) on p.Id equals ps.FK_Pid
                                    group ps by new { p.Id, p.Title, p.Introduction, p.Description } into r
                                    select new ProdGetDisplayDto
                                    {
                                        Id = r.Key.Id,
                                        Title = r.Key.Title,
                                        Introduction = r.Key.Introduction,
                                        Description = r.Key.Description,
                                        Link = "/Toilet/" + r.Key.Id,
                                        Image = "/images/product/pro_0" + r.Key.Id + ".png",
                                        Price = r.Min(e => e.Price) == r.Max(e => e.Price) ? r.Min(e => e.Price).ToString() : r.Min(e => e.Price) + " ~ " + r.Max(e => e.Price),
                                    }).Take(num).ToArrayAsync();

                return new JsonResult(result, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ProdGetDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<ProdIdTitleDto>> GetSpecType()
        {
            try
            {
                var db_pst = db.Prod_Spec_Types;

                if (db_pst != null)
                {
                    long WebsiteID = await loginUserDataApplication.GetWebsiteId();
                    var output = await (from pst in db_pst
                                        where !pst.IsDeleted && pst.FK_WebsiteId == WebsiteID
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
        public async Task<ResponseMessageDto> ProdDelete(DataDelectDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();
                var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                if (db_p != null)
                {
                    db_p.IsDeleted = true;
                    db_p.DeletionTime = DateTime.Now;
                    db_p.DeleterUserId = db_t.UserID;

                    var db_ps = db.Prod_Stocks.Where(e => e.FK_Pid == dto.Id);
                    if (db_ps != null)
                    {
                        foreach (var ps in db_ps)
                        {
                            ps.IsDeleted = true;
                            ps.DeletionTime = DateTime.Now;
                            ps.DeleterUserId = db_t.UserID;
                        }
                    }
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
        public async Task<ResponseMessageDto> StockDelete(DataDelectDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var db_ps = db.Prod_Stocks.Where(e => e.Id == dto.Id).FirstOrDefault();
                var db_t = db.Tokens.Where(e => e.id == dto.TId).FirstOrDefault();
                if (db_ps != null)
                {
                    db_ps.IsDeleted = true;
                    db_ps.DeletionTime = DateTime.Now;
                    db_ps.DeleterUserId = db_t.UserID;
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
