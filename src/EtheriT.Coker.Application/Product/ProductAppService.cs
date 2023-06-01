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
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.Dto.Import;
using Microsoft.AspNetCore.Http;
using EtheriT.Coker.Application.Import;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using AutoMapper;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Dto.Files;
using System;

namespace EtheriT.Coker.Application.Product
{
    public class ProductAppService : IProductAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly ITechnicalCertificateAppService technicalCertificateAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ImportAppService importAppService;
        public ProductAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITagAppService tagAppService,
            IConfiguration configuration,
            IMapper mapper,
            ITechnicalCertificateAppService technicalCertificateAppService,
            IFileUploadAppService fileUploadAppService,
            ImportAppService importAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tagAppService = tagAppService;
            this.configuration = configuration;
            this.technicalCertificateAppService = technicalCertificateAppService;
            this.importAppService = importAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.mapper = mapper;
        }
        /* Add & Update */
        public async Task<ResponseMessageDto> ProductAddUp(ProdAddUpDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto tag_response = new ResponseMessageDto() { Success = true };
            ResponseMessageDto techcert_response = new ResponseMessageDto() { Success = true };
            ResponseMessageDto stock_response = new ResponseMessageDto() { Success = true };
            var asoid = dto.Id;

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
                        Introduction = dto.Introduction ?? "",
                        Description = dto.Description ?? "",
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        permanent = dto.Permanent,
                        CreatorUserId = userId
                    };
                    db.Prods.Add(p);
                    db.SaveChanges();
                    asoid = p.Id;
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

                if (asoid != 0)
                {
                    var tagitem = new List<TagAssociateDto>();
                    foreach (var data in dto.TagSelected)
                    {
                        tagitem.Add(new TagAssociateDto()
                        {
                            Id = data.Id,
                            FK_AId = (long)asoid,
                            FK_TId = data.FK_TId,
                            Type = (int)TagAssociateTypeEnum.商品,
                            IsDeleted = data.IsDeleted
                        });
                    }

                    tag_response = await tagAppService.TagAssociateAddDelect(tagitem);

                    var techcertitem = new List<TechCertProdAssociateDto>();
                    foreach (var data in dto.TechCertSelected)
                    {
                        techcertitem.Add(new TechCertProdAssociateDto()
                        {
                            Id = data.Id,
                            FK_PId = (long)asoid,
                            FK_TCId = data.FK_TCId,
                            IsDeleted = data.IsDeleted
                        });
                    }

                    techcert_response = await technicalCertificateAppService.TechCertAssociateAddDelect(techcertitem);

                    stock_response = await this.StockAddUp(asoid, dto.Stocks);
                }

                output.Success = tag_response.Success && techcert_response.Success && stock_response.Success;
                output.Message = asoid.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> StockAddUp(long Pid, List<ProductStockDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto priceresponse = new ResponseMessageDto() { Success = false };
            if (dto.Count == 0)
            {
                output.Success = true;
                return output;
            }
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
                            FK_Pid = Pid,
                            FK_S1id = item.FK_S1id,
                            FK_S2id = item.FK_S2id,
                            Stock = item.Stock,
                            Min_Qty = item.Min_Qty,
                            Alert_Qty = item.Alert_Qty,
                            Ser_No = item.Ser_No,
                            CreatorUserId = usetId,
                        };
                        db.Prod_Stocks.Add(ps);
                        db.SaveChanges();

                        foreach (var price in item.Prices)
                        {
                            price.FK_PSId = ps.Id;

                        }
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
                            db_ps.Ser_No = item.Ser_No;
                            db_ps.LastModificationTime = DateTime.Now;
                            db_ps.LastModifierUserId = usetId;
                        }
                    }

                    priceresponse = await this.PriceAddUp(item.Prices);

                }

                db.SaveChanges();

                output.Success = priceresponse.Success;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> PriceAddUp(List<ProductPriceDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto deleteresponse = new ResponseMessageDto() { Success = true };
            try
            {
                long usetId = await loginUserData.GetUserId();
                if (usetId != 0)
                {
                    foreach (var item in dto)
                    {
                        if (item.Id == 0 && !item.IsDelete)
                        {
                            Core.Models.Prod_Price pp = new Core.Models.Prod_Price
                            {
                                FK_PSId = (long)item.FK_PSId,
                                FK_RId = item.FK_RId,
                                Price = item.Price,
                                Bonus = item.Bonus,
                                CreatorUserId = usetId
                            };
                            db.Prod_Prices.Add(pp);
                            db.SaveChanges();
                        }
                        else if (!item.IsDelete)
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
                        else
                        {
                            deleteresponse = await this.PriceDelete((long)item.Id);
                            if (!deleteresponse.Success)
                            {
                                output.Success = false;
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

                var dataQuery = from p in db.Prods
                                where p.FK_WebsiteId == webid && !p.IsDeleted
                                select new ProductGetAllListDto
                                {
                                    Id = p.Id,
                                    Title = p.Title,
                                    Disp_Opt = p.Disp_Opt,
                                    Ser_No = p.Ser_No,
                                    Price = "",
                                    StartTime = p.StartTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.StartTime),
                                    EndTime = p.EndTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.EndTime),
                                    Permanent = p.permanent
                                };
                //var dataQuery = from ps in db.Prod_Stocks
                //                where !ps.IsDeleted
                //                from pp in db.Prod_Prices
                //                where !pp.IsDeleted && pp.FK_PSId == ps.Id
                //                join p in db.Prods on ps.FK_Pid equals p.Id
                //                where !p.IsDeleted && p.FK_WebsiteId == webid
                //                group pp by new { p.Id, p.Title, p.Disp_Opt, p.Ser_No, p.StartTime, p.EndTime, p.permanent } into s
                //                select new ProductGetAllListDto
                //                {
                //                    Id = s.Key.Id,
                //                    Title = s.Key.Title,
                //                    Disp_Opt = s.Key.Disp_Opt,
                //                    Ser_No = s.Key.Ser_No,
                //                    Price = s.Min(e => e.Price) == s.Max(e => e.Price) ? s.Min(e => e.Price).ToString() : s.Min(e => e.Price) + " ~ " + s.Max(e => e.Price),
                //                    StartTime = s.Key.StartTime,
                //                    EndTime = s.Key.EndTime,
                //                    Permanent = s.Key.permanent
                //                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<ProductGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ProdGetDataDto> GetProdDataOne(long Id)
        {
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var db_p = db.Prods.Where(e => e.Id == Id).OrderBy(e => e.Ser_No).FirstOrDefault();

                if (db_p != null)
                {
                    ProdGetDataDto output = new ProdGetDataDto()
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
                        TagDatas = new List<TagGetSelectedDto>(),
                        TechCertDatas = new List<TechCertGetSelectedDto>(),
                        Stocks = new List<ProductStockDto>(),
                        Files = new List<FileGetProdDisplayDto>(),
                    };

                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = output.Id,
                        Type = (int)TagAssociateTypeEnum.商品,
                    }
                    );

                    if (tagDatas != null)
                    {
                        output.TagDatas = tagDatas;
                    }

                    var techcertDatas = await technicalCertificateAppService.GetTechCertAssociate(db_p.Id);

                    if (techcertDatas != null)
                    {
                        output.TechCertDatas = techcertDatas;
                    }

                    var stockDatas = await this.GetStockDataAll(output.Id);
                    if (stockDatas != null)
                    {
                        output.Stocks = stockDatas;
                    }

                    var fileDatas = await fileUploadAppService.getProdDisplayFiles(output.Id, 1);
                    if (fileDatas != null)
                    {
                        output.Files = fileDatas;
                    }

                    return output;
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception e)
            {
                return null;
            }
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
                                        S1_Title = "",
                                        FK_S2id = ps.FK_S2id,
                                        S2_Title = "",
                                        Price = ps.Price,
                                        Min_Qty = ps.Min_Qty,
                                        Stock = ps.Stock,
                                        Alert_Qty = ps.Alert_Qty,
                                        Prices = new List<ProductPriceDto>(),
                                    }).ToListAsync();

                var db_sp = db.Prod_Specs.ToList();

                foreach (var item in output)
                {
                    item.FK_ST1id = (item.FK_S1id != null && (int)item.FK_S1id > 0) ? db_sp[(int)item.FK_S1id - 1].FK_Tid : 0;
                    item.S1_Title = (item.FK_S1id != null && (int)item.FK_S1id > 0) ? db_sp[(int)item.FK_S1id - 1].Title : "";
                    item.FK_ST2id = (item.FK_S2id != null && (int)item.FK_S2id > 0) ? db_sp[(int)item.FK_S2id - 1].FK_Tid : 0;
                    item.S2_Title = (item.FK_S2id != null && (int)item.FK_S2id > 0) ? db_sp[(int)item.FK_S2id - 1].Title : "";
                    item.Prices = await this.GetPriceDataAll(item.Id);
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
        public async Task<ProdGetMainDisplayDto> GetMainDisplayOne(long Id)
        {
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var db_p = db.Prods.Where(e => e.Id == Id).OrderBy(e => e.Ser_No).FirstOrDefault();

                if (db_p != null)
                {
                    ProdGetMainDisplayDto output = new ProdGetMainDisplayDto()
                    {
                        Id = db_p.Id,
                        Title = db_p.Title,
                        Introduction = db_p.Introduction,
                        Description = db_p.Description,
                        TagDatas = new List<TagGetSelectedDto>(),
                        TechCertDatas = new List<TechCertDisplayDto>(),
                        Stocks = new List<ProductStockDto>(),
                        Files_Original = new List<FileGetProdDisplayDto>(),
                        Files_Medium = new List<FileGetProdDisplayDto>(),
                        Files_Small = new List<FileGetProdDisplayDto>(),
                    };

                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = output.Id,
                        Type = (int)TagAssociateTypeEnum.商品,
                    }
                    );

                    if (tagDatas != null)
                    {
                        output.TagDatas = tagDatas;
                    }

                    var techcertDatas = await technicalCertificateAppService.GetDisplayData(db_p.Id);

                    if (techcertDatas != null)
                    {
                        output.TechCertDatas = techcertDatas;
                    }

                    var stockDatas = await this.GetStockDataAll(output.Id);
                    if (stockDatas != null)
                    {
                        output.Stocks = stockDatas;
                    }

                    var fileDatas_original = await fileUploadAppService.getProdDisplayFiles(output.Id, 1);
                    if (fileDatas_original != null)
                    {
                        output.Files_Original = fileDatas_original;
                    }

                    var fileDatas_medium = await fileUploadAppService.getProdDisplayFiles(output.Id, 2);
                    if (fileDatas_medium != null)
                    {
                        output.Files_Medium = fileDatas_medium;
                    }

                    var fileDatas_small = await fileUploadAppService.getProdDisplayFiles(output.Id, 3);
                    if (fileDatas_small != null)
                    {
                        output.Files_Small = fileDatas_small;
                    }

                    return output;
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(DirectoryReleInfoInputDto dto)
        {

            try
            {
                long WebsiteID = dto.SiteId == 0 ? await loginUserData.GetWebsiteId() : (long)dto.SiteId;
                var output = new List<DirectoryReleInfoDto>();
                var productData = new List<ProdGetDataDto>();

                foreach (var Id in dto.Ids)
                {
                    var result = await db.Prods.Where(e => e.Id == Id && !e.IsDeleted && e.FK_WebsiteId == WebsiteID).FirstOrDefaultAsync();

                    if (result != null)
                    {
                        ProdGetDataDto tempoutput = mapper.Map<ProdGetDataDto>(result);
                        productData.Add(tempoutput);
                    }
                    else throw new Exception("查無商品資料");
                }

                if (productData != null)
                {
                    productData.Sort((x, y) => (x.Ser_No.CompareTo(y.Ser_No) * 2 + x.Id.CompareTo(y.Id)));
                    foreach (var data in productData)
                    {
                        var output_data = new DirectoryReleInfoDto();
                        output_data = mapper.Map(data, output_data);
                        output_data.Link = $"/lcb/product/toilet/{data.Id}";
                        output_data.MainImage = "/upload/product/pro_pic_01.jpg";

                        output.Add(output_data);

                        //var imagedata = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                        //{
                        //    Sid = data.Id,
                        //    Type = (int)FileBindTypeEnum.產品,
                        //    Size = 1
                        //});
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        /* Delete */
        public async Task<ResponseMessageDto> ProdDelete(long Id)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = true };
            ResponseMessageDto tagdeleteresponse = new ResponseMessageDto() { Success = true };
            ResponseMessageDto techcertdeleteresponse = new ResponseMessageDto() { Success = true };
            ResponseMessageDto stockresponse = new ResponseMessageDto() { Success = true };
            ResponseMessageDto fileresponse = new ResponseMessageDto() { Success = true };

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

                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == (int)TagAssociateTypeEnum.商品 && !e.IsDeleted).ToListAsync();

                    if (tagids != null)
                    {
                        foreach (var tagid in tagids)
                        {

                            tagdeleteresponse = await tagAppService.TagAssociateDelete(tagid.Id);
                            if (tagdeleteresponse.Success == false)
                            {
                                output.Success = false;
                            }
                        }
                    }

                    var techcertids = await db.Prod_TechCerts.Where(e => e.FK_PId == Id && !e.IsDeleted).ToListAsync();

                    if (techcertids != null)
                    {
                        foreach (var techcertid in techcertids)
                        {
                            techcertdeleteresponse = await technicalCertificateAppService.TechCertAssociateDelete(techcertid.Id);
                            if (techcertdeleteresponse.Success == false)
                            {
                                output.Success = false;
                            }
                        }
                    }

                    var stockids = await db.Prod_Stocks.Where(e => e.FK_Pid == Id && !e.IsDeleted).ToListAsync();

                    if (stockids != null)
                    {
                        foreach (var stockid in stockids)
                        {
                            stockresponse = await this.StockDelete(stockid.Id);
                            if (stockresponse.Success == false)
                            {
                                output.Success = false;
                            }
                        }
                    }

                    fileresponse = await fileUploadAppService.deleteImgBySId(new FileDeleteDto()
                    {
                        Sid = Id,
                        Type = (int)FileBindTypeEnum.產品,
                    });

                    output.Success = fileresponse.Success;

                    db.SaveChanges();
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
        /* Other Get */
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
        public async Task<ProdGetOneDto> GetDisplayOne(long id)
        {
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var db_p = db.Prods.Where(e => e.Id == id && e.FK_WebsiteId == websiteId)
                    .Where(e => !e.IsDeleted && (e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now < e.EndTime)))
                    .FirstOrDefault();
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
        /* Product Log */
        public async Task<ImportOutputDto> ProdReplace(IList<IFormFile> files)
        {
            ImportOutputDto response = new ImportOutputDto { ErrorList = new List<ImportMassageItem>() };
            List<ProductImportDto> prods = await importAppService.ProdReplace(files);
            for (int i = 0; i < prods.Count; i++)
            {
                ImportMassageItem? error = await InsertOrUpdate(prods[i]);
                if (error != null) response.ErrorList.Add(error);
            }
            return response;
        }
        private async Task<ImportMassageItem?> InsertOrUpdate(ProductImportDto item)
        {
            ImportMassageItem? importMassageItem = null;
            try
            {
                List<string> tagStr = new List<string> { item.Tag1 ?? "_", item.Tag2 ?? "_", item.Tag3 ?? "_", item.Tag4 ?? "_", item.Tag5 ?? "_" };
                List<string> TechStr = new List<string> { item.Tech1 ?? "", item.Tech2 ?? "", item.Tech3 ?? "", item.Tech4 ?? "" };
                ProdAddUpDto prod = mapper.Map<ProdAddUpDto>(item);
                for (int i = 0; i < tagStr.Count; i++)
                {
                    var st = await db.Tags.Where(e => tagStr[i] == e.Title).FirstOrDefaultAsync();
                    if (st == null && !Regex.IsMatch(tagStr[i], "^_"))
                        await tagAppService.TagAddUp(new DevExpressDto { Values = JsonConvert.SerializeObject(new TagGetAllListDto { Title = tagStr[i] }) });
                }
                var p = await db.Prods.Where(e => e.Title == item.Title).FirstOrDefaultAsync();
                var t = await db.Tags.Where(e => tagStr.Contains(e.Title)).Where(e => e.IsDeleted).ToListAsync();
                if (p != null && item.price == 0)
                {
                    mapper.Map(p, prod);
                }
                if (p != null) prod.Id = p.Id;
                if (t.Any()) prod.TagSelected = mapper.Map<List<TagSelectedDto>>(t);
                else prod.TagSelected = new List<TagSelectedDto>();
                prod.TechCertSelected = new List<TechCertSelectedDto>();
                prod.Stocks = new List<ProductStockDto>();
                var response = await ProductAddUp(prod);
                if (response != null && !response.Success)
                {
                    throw new Exception(response.Error);
                }
            }
            catch (Exception e)
            {
                importMassageItem = new ImportMassageItem { Name = item.Title ?? "", Description = e.Message };

            }

            return importMassageItem;
        }

    }
}
