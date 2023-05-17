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
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EtheriT.Coker.Application.Product
{
    public class ProductAppService : IProductAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        public ProductAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITagAppService tagAppService,
            IFileUploadAppService fileUploadAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tagAppService = tagAppService;
            this.fileUploadAppService = fileUploadAppService;
        }
        /* Add & Update */
        public async Task<ResponseMessageDto> ProductAddUp(ProductDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long userId = await loginUserData.GetUserId();

                if (dto.Id == 0)
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
                        CreatorUserId = userId
                    };
                    db.Prods.Add(p);
                    db.SaveChanges();
                    output.Message = p.Id.ToString();
                }
                else
                {
                    var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_p != null)
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
                        db_p.LastModifierUserId = userId;
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
        public async Task<ResponseMessageDto> StockAddUp(List<ProductStockDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                output.Message = "";
                foreach (var item in dto)
                {
                    if (item.Id == 0)
                    {
                        Core.Models.Prod_Stock ps = new Core.Models.Prod_Stock
                        {
                            FK_Pid = item.Pid,
                            FK_S1id = item.FK_S1id,
                            FK_S2id = item.FK_S2id,
                            Stock = item.Stock,
                            Min_Qty = item.Min_Qty,
                            Alert_Qty = item.Alert_Qty,
                            Ser_No = 500,
                            CreatorUserId = usetId,
                        };
                        db.Prod_Stocks.Add(ps);
                        db.SaveChanges();
                        output.Message += ps.Id + ",";
                    }
                    else
                    {
                        var db_ps = db.Prod_Stocks.Where(e => e.Id == item.Id).FirstOrDefault();

                        if (db_ps != null)
                        {
                            db_ps.FK_S1id = item.FK_S1id;
                            db_ps.FK_S2id = item.FK_S2id;
                            db_ps.Stock = item.Stock;
                            db_ps.Min_Qty = item.Min_Qty;
                            db_ps.Alert_Qty = item.Alert_Qty;
                            db_ps.LastModificationTime = DateTime.Now;
                            db_ps.LastModifierUserId = usetId;
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
        public async Task<ResponseMessageDto> TechCertAddUp(List<ProductTechCertDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                if (usetId != null)
                {
                    foreach (var item in dto)
                    {
                        if (item.Id == 0)
                        {
                            Core.Models.Prod_TechCert ptc = new Core.Models.Prod_TechCert
                            {
                                FK_PId = item.FK_PId,
                                FK_TCId = item.FK_TCId,
                                IsChecked = item.IsChecked,
                                CreatorUserId = usetId,
                            };
                            db.Prod_TechCerts.Add(ptc);
                            db.SaveChanges();
                        }
                        else
                        {
                            var db_ptc = db.Prod_TechCerts.Where(e => e.Id == item.Id).FirstOrDefault();

                            if (db_ptc != null)
                            {
                                db_ptc.IsChecked = item.IsChecked;
                                db_ptc.LastModifierUserId = usetId;
                                db_ptc.LastModificationTime = DateTime.Now;
                            }
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
        public async Task<ResponseMessageDto> ProdPriceAddUp(List<ProductPriceDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                long usetId = await loginUserData.GetUserId();
                if (usetId != null)
                {
                    foreach (var item in dto)
                    {
                        if (item.Id == 0)
                        {
                            Core.Models.Prod_Price pp = new Core.Models.Prod_Price
                            {
                                FK_PSId = item.FK_PSId,
                                FK_RId = item.FK_RId,
                                Price = item.Price,
                                Bonus = item.Bonus,
                                CreatorUserId = usetId,
                            };
                            db.Prod_Prices.Add(pp);
                            db.SaveChanges();
                        }
                        else
                        {
                            var db_pp = db.Prod_Prices.Where(e => e.Id == item.Id).FirstOrDefault();

                            if (db_pp != null)
                            {
                                db_pp.FK_RId = item.FK_RId;
                                db_pp.Price = item.Price;
                                db_pp.Bonus = item.Bonus;
                                db_pp.LastModifierUserId = usetId;
                                db_pp.LastModificationTime = DateTime.Now;
                            }
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
        /* Get Data */
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();

                var dataQuery = from ps in db.Prod_Stocks
                                where !ps.IsDeleted
                                from pp in db.Prod_Prices
                                where !pp.IsDeleted && pp.FK_PSId == ps.Id
                                join p in db.Prods on ps.FK_Pid equals p.Id
                                where !p.IsDeleted && p.FK_WebsiteId == webid
                                group pp by new { p.Id, p.Title, p.Disp_Opt, p.Ser_No, p.StartTime, p.EndTime, p.permanent } into s
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
                var output = await (from ps in db.Prod_Stocks
                                    where !ps.IsDeleted && ps.FK_Pid == PId
                                    orderby ps.Id
                                    select new ProductStockDto
                                    {
                                        Pid = PId,
                                        Id = ps.Id,
                                        FK_S1id = ps.FK_S1id,
                                        FK_S2id = ps.FK_S2id,
                                        Price = ps.Price,
                                        Min_Qty = ps.Min_Qty,
                                        Stock = ps.Stock,
                                        Alert_Qty = ps.Alert_Qty
                                    }).ToListAsync();

                var db_sp = db.Prod_Specs.ToList();

                foreach (var item in output)
                {
                    item.FK_ST1id = (item.FK_S1id != null && (int)item.FK_S1id > 0) ? db_sp[(int)item.FK_S1id - 1].FK_Tid : 0;
                    item.FK_ST2id = (item.FK_S2id != null && (int)item.FK_S2id > 0) ? db_sp[(int)item.FK_S2id - 1].FK_Tid : 0;
                }

                return output;
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<List<TechCertGetAllDto>> GetTechCertDataAll(long PId)
        {
            try
            {
                var output = await (from ptc in db.Prod_TechCerts
                                    where !ptc.IsDeleted && ptc.FK_PId == PId
                                    orderby ptc.Id
                                    select new TechCertGetAllDto
                                    {
                                        Id = ptc.Id,
                                        FK_PId = ptc.FK_PId,
                                        FK_TCId = ptc.FK_TCId,
                                        Img = new List<string>(),
                                        IsChecked = ptc.IsChecked,
                                        Title = "",
                                    }).ToListAsync();


                foreach (var item in output)
                {
                    var tc = db.TechnicalCertificates.Where(e => e.Id == item.FK_TCId && !e.IsDeleted).FirstOrDefault();
                    if (tc != null)
                    {
                        item.Title = tc.Title == null ? "" : tc.Title;
                        var images = await fileUploadAppService.getImgThumbnail(tc.Id);
                        if (images.Count > 0)
                        {
                            foreach (var image in images)
                            {
                                if (image.Link != null)
                                {
                                    item.Img.Add(image.Link);
                                }
                            }
                        }
                    }
                }

                return output;
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<List<ProductPriceDto>> GetPriceDataAll(long PSId)
        {
            try
            {
                var output = await (from pp in db.Prod_Prices
                                    where !pp.IsDeleted && pp.FK_PSId == PSId
                                    orderby pp.FK_PSId
                                    select new ProductPriceDto
                                    {
                                        Id = pp.Id,
                                        FK_PSId = pp.FK_PSId,
                                        FK_RId = pp.FK_RId,
                                        Price = pp.Price,
                                        Bonus = pp.Bonus,
                                    }).ToListAsync();

                return output;
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<List<ProdIdTitleDto>> GetSpecType()
        {
            try
            {
                var db_pst = db.Prod_Spec_Types;

                if (db_pst != null)
                {
                    long WebsiteID = await loginUserData.GetWebsiteId();
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
        /* Get Display */
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
                        Image = "/upload/product/pro_0" + db_p.Id + ".png",
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
                                    orderby Guid.NewGuid()
                                    select new ProdGetDisplayDto
                                    {
                                        Id = r.Key.Id,
                                        Title = r.Key.Title,
                                        Introduction = r.Key.Introduction,
                                        Description = r.Key.Description,
                                        Link = "/Toilet/" + r.Key.Id,
                                        Image = "/upload/product/pro_0" + r.Key.Id + ".png",
                                        Price = r.Min(e => e.Price) == r.Max(e => e.Price) ? r.Min(e => e.Price).ToString() : r.Min(e => e.Price) + " ~ " + r.Max(e => e.Price),
                                    }).Take(num).ToArrayAsync();

                return new JsonResult(result, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ProdGetDisplayDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<ProdDisImgDto>> GetHistoryDisplay(Guid TId)
        {
            try
            {
                var output = await (from pl in db.Prod_Logs
                                    where pl.FK_Tid == TId && pl.Action == 2
                                    orderby pl.Id descending
                                    select new ProdDisImgDto
                                    {
                                        Id = pl.FK_Pid,
                                    }).Take(4).ToListAsync();

                return output;
            }
            catch (Exception e)
            {

            }
            return null;
        }
        /* Delete */
        public async Task<ResponseMessageDto> ProdDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_p = db.Prods.Where(e => e.Id == Id).FirstOrDefault();

                if (db_p != null)
                {
                    db_p.IsDeleted = true;
                    db_p.DeletionTime = DateTime.Now;
                    db_p.DeleterUserId = usetId;

                    var db_ps = db.Prod_Stocks.Where(e => e.FK_Pid == Id);
                    if (db_ps != null)
                    {
                        foreach (var ps in db_ps)
                        {
                            ps.IsDeleted = true;
                            ps.DeletionTime = DateTime.Now;
                            ps.DeleterUserId = usetId;

                            var db_pp = db.Prod_Prices.Where(e => e.FK_PSId == ps.Id);
                            foreach (var item in db_pp)
                            {
                                item.IsDeleted = true;
                                item.DeleterUserId = usetId;
                                item.DeletionTime = DateTime.Now;
                            }
                        }
                    }

                    var db_ptc = db.Prod_TechCerts.Where(e => e.FK_PId == Id);
                    if (db_ptc != null)
                    {
                        foreach (var pst in db_ptc)
                        {
                            pst.IsDeleted = true;
                            pst.DeletionTime = DateTime.Now;
                            pst.DeleterUserId = usetId;
                        }
                    }

                    await tagAppService.TagAssociateDelete(Id);

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
        public async Task<ResponseMessageDto> StockDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_ps = db.Prod_Stocks.Where(e => e.Id == Id).FirstOrDefault();
                if (db_ps != null)
                {
                    db_ps.IsDeleted = true;
                    db_ps.DeletionTime = DateTime.Now;
                    db_ps.DeleterUserId = usetId;
                    db.SaveChanges();
                    output.Success = true;
                }

                var db_pp = db.Prod_Prices.Where(e => e.FK_PSId == Id);
                foreach (var item in db_pp)
                {
                    item.IsDeleted = true;
                    item.DeleterUserId = usetId;
                    item.DeletionTime = DateTime.Now;
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> PriceDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_pp = db.Prod_Prices.Where(e => e.Id == Id).FirstOrDefault();
                if (db_pp != null)
                {
                    db_pp.IsDeleted = true;
                    db_pp.DeletionTime = DateTime.Now;
                    db_pp.DeleterUserId = usetId;
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
        public async Task<ResponseMessageDto> TechCertDelete(long PSId)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var db_pp = await db.Prod_Prices.Where(e => e.FK_PSId == PSId).ToListAsync();

                foreach (var item in db_pp)
                {
                    item.IsDeleted = true;
                    item.DeletionTime = DateTime.Now;
                    item.DeleterUserId = usetId;
                    db.SaveChanges();
                    output.Success = true;
                }
                if (db_pp != null)
                {
                }
            }
            catch
            {

            }

            return output;
        }
        /* Product Log */
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
