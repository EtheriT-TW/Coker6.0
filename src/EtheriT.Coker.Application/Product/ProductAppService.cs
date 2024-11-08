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
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Specification;
using System.Web;
using System.Data;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using System.IO;
using System.Linq;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.Migrations;
using Microsoft.CodeAnalysis.CSharp;
using EtheriT.Coker.Application.Shared.Dto.Favorites;

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
        private readonly IWebMenuApplication webMenuApplication;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly ISpecificationAppService specificationAppService;
        private readonly ITokenAppService tokenAppService;
        private readonly ImportAppService importAppService;
        public ProductAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITagAppService tagAppService,
            IConfiguration configuration,
            IMapper mapper,
            ITechnicalCertificateAppService technicalCertificateAppService,
            IFileUploadAppService fileUploadAppService,
            ISpecificationAppService specificationAppService,
            IWebMenuApplication webMenuApplication,
            ITokenAppService tokenAppService,
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
            this.specificationAppService = specificationAppService;
            this.webMenuApplication = webMenuApplication;
            this.tokenAppService = tokenAppService;
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
                        CreatorUserId = userId
                    };
                    mapper.Map(dto, p);
                    db.Prods.Add(p);
                    await loginUserData.SaveChanges(p);
                    asoid = p.Id;
                }
                else
                {
                    var db_p = db.Prods.Where(e => e.Id == dto.Id).FirstOrDefault();
                    if (db_p != null)
                    {
                        mapper.Map(dto, db_p);
                        await loginUserData.SaveChanges(db_p);
                    }
                }

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
                for (int i = 0; i < dto.Count; i++)
                {
                    var item = dto[i];
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
                            Price = item.Price,
                            CreatorUserId = usetId,
                        };
                        db.Prod_Stocks.Add(ps);
                        await db.SaveChangesAsync();

                        foreach (var price in item.Prices)
                        {
                            price.FK_PSId = ps.Id;
                        }
                    }
                    else
                    {
                        var db_ps = await db.Prod_Stocks.Where(e => e.Id == item.Id).FirstOrDefaultAsync();

                        if (db_ps != null)
                        {
                            db_ps.FK_S1id = item.FK_S1id;
                            db_ps.FK_S2id = item.FK_S2id;
                            db_ps.Stock = item.Stock;
                            db_ps.Min_Qty = item.Min_Qty;
                            db_ps.Alert_Qty = item.Alert_Qty;
                            db_ps.Ser_No = item.Ser_No;
                            db_ps.Price = item.Price;
                            db_ps.LastModificationTime = DateTime.Now;
                            db_ps.LastModifierUserId = usetId;
                        }
                    }

                    priceresponse = await PriceAddUp(item.Prices);

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
                    for (int i = 0; i < dto.Count; i++)
                    {
                        var item = dto[i];
                        var allPrice = db.Prod_Prices.Where(e => !e.IsDeleted);
                        var thePrice = await allPrice
                                .Where(e => e.FK_PSId == item.FK_PSId)
                                .Where(e => e.FK_RId == item.FK_RId)
                                .FirstOrDefaultAsync();
                        if (thePrice != null && !item.IsDelete) item.Id = thePrice.Id;

                        if (item.Id == 0 && !item.IsDelete)
                        {
                            Prod_Price pp = new Prod_Price
                            {
                                FK_PSId = (long)item.FK_PSId,
                                FK_RId = item.FK_RId,
                                Price = item.Price,
                                Bonus = item.Bonus,
                                CreatorUserId = usetId
                            };
                            db.Prod_Prices.Add(pp);
                            await db.SaveChangesAsync();
                        }
                        else if (!item.IsDelete)
                        {
                            var db_pp = await db.Prod_Prices.Where(e => e.Id == item.Id).FirstOrDefaultAsync();

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

                await db.SaveChangesAsync();
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
                                    Visible = p.Visible,
                                    Ser_No = p.Ser_No,
                                    ItemNo = p.ItemNo ?? "",
                                    Price = "",
                                    StartTime = p.StartTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.StartTime),
                                    EndTime = p.EndTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.EndTime),
                                    Permanent = p.permanent
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                foreach (var data in output.data)
                {
                    int min_price = 0, max_price = 0;
                    var pid = (long)data.GetType().GetProperty("Id").GetValue(data, null);
                    var db_ps = await db.Prod_Stocks.Where(e => e.FK_Pid == pid && !e.IsDeleted).ToListAsync();
                    if (db_ps.Count > 0)
                    {
                        for (var ps_i = 0; ps_i < db_ps.Count(); ps_i++)
                        {
                            var db_pp = await db.Prod_Prices.Where(e => e.FK_PSId == db_ps[ps_i].Id && !e.IsDeleted).ToListAsync();
                            if (db_pp.Count > 0)
                            {
                                for (var pp_i = 0; pp_i < db_pp.Count; pp_i++)
                                {
                                    var price = (int)(db_pp[pp_i].Price ?? 0);
                                    if (min_price == 0 || price < min_price) { min_price = price; }
                                    if (price > max_price) { max_price = price; }
                                }
                            }
                        }
                        var price_text = min_price == max_price ? max_price.ToString("###,###") : $"{min_price.ToString("###,###")}~{max_price.ToString("###,###")}";
                        data.GetType().GetProperty("Price").SetValue(data, price_text);
                    }
                }
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
                        TagDatas = new List<TagGetSelectedDto>(),
                        TechCertDatas = new List<TechCertGetSelectedDto>(),
                        Stocks = new List<ProductStockDto>(),
                        Files = new List<FileGetProdDisplayDto>(),
                        Multimedia = new List<FileGetProdDisplayDto>()
                    };
                    mapper.Map(db_p, output);

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

                    var fileDatas = await fileUploadAppService.getProdFiles(output.Id);
                    if (fileDatas != null)
                    {
                        output.Files = fileDatas;
                    }
                    var mediaDatas = await fileUploadAppService.getProdMultimedia(output.Id, 1);
                    if (mediaDatas != null)
                    {
                        output.Multimedia = mediaDatas;
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
                                        SubItemNo = ps.SubItemNo ?? "",
                                        Prices = new List<ProductPriceDto>(),
                                    }).ToListAsync();

                var db_sp = await db.Prod_Specs.Where(e => !e.IsDeleted).ToListAsync();

                if (db_sp.Count > 0)
                {
                    foreach (var item in output)
                    {
                        item.FK_ST1id = (int)item.FK_S1id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S1id).FK_Tid : 0;
                        item.S1_Title = (int)item.FK_S1id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S1id).Title : "";
                        item.FK_ST2id = (int)item.FK_S2id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S2id).FK_Tid : 0;
                        item.S2_Title = (int)item.FK_S2id != 0 ? db_sp.Find(spec => spec.Id == item.FK_S2id).Title : "";
                        item.Prices = await this.GetPriceDataAll(item.Id);
                    }
                }

                return output;
            }
            catch (Exception e)
            {
                return null;
            }

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
                                        Bonus = pp.Bonus ?? 0,
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
                        Html = db_p.Html ?? "",
                        ItemNo = db_p.ItemNo,
                        Status = db_p.Status,
                        StatusName = ((ProdStatusEnum)db_p.Status).ToString(),
                        TagDatas = new List<TagGetSelectedDto>(),
                        TechCertDatas = new List<TechCertDisplayDto>(),
                        Stocks = new List<ProductStockDto>(),
                        Img_Original = new List<FileGetProdDisplayDto>(),
                        Img_Medium = new List<FileGetProdDisplayDto>(),
                        Img_Small = new List<FileGetProdDisplayDto>(),
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

                    var Files = await fileUploadAppService.getImgFiles(new FileGetImgInputDto()
                    {
                        Sid = output.Id,
                        Size = 1,
                        Type = 8
                    });
                    if (Files != null && Files.Count() > 0) output.Files = Files;

                    var Imgs_original = await fileUploadAppService.getProdMultimedia(output.Id, 1);
                    if (Imgs_original != null && Imgs_original.Count != 0)
                    {
                        output.Img_Original = Imgs_original;
                    }
                    else output.Img_Original.Add(new FileGetProdDisplayDto
                    {
                        Link = new List<string> { "/images/noImg.jpg" },
                        Name = "/images/noImg.jpg",
                        FileType = 1,
                        SerNo = 500
                    });

                    var Imgs_medium = await fileUploadAppService.getProdMultimedia(output.Id, 2);
                    if (Imgs_medium != null && Imgs_medium.Count != 0)
                    {
                        output.Img_Medium = Imgs_medium;
                    }
                    else output.Img_Medium.Add(new FileGetProdDisplayDto
                    {
                        Link = new List<string> { "/images/noImg.jpg" },
                        Name = "/images/noImg.jpg",
                        FileType = 1,
                        SerNo = 500
                    });

                    var Imgs_small = await fileUploadAppService.getProdMultimedia(output.Id, 3);
                    if (Imgs_small != null && Imgs_small.Count != 0)
                    {
                        output.Img_Small = Imgs_small;
                    }
                    else output.Img_Small.Add(new FileGetProdDisplayDto
                    {
                        Link = new List<string> { "/images/noImg.jpg" },
                        Name = "/images/noImg.jpg",
                        FileType = 1,
                        SerNo = 500
                    });

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
                string orgName = await loginUserData.GetWebsiteOrgName();
                var output = new List<DirectoryReleInfoDto>();
                var productData = new List<ProdGetDataDto>();
                var result = await db.Prods.Where(e => dto.Ids.Contains(e.Id) && !e.IsDeleted && e.FK_WebsiteId == WebsiteID)
                    .OrderBy(e => e.Ser_No).ThenByDescending(e => e.Status == 5).ThenBy(e => e.ItemNo).ThenBy(e => e.Title).ThenByDescending(e => e.Id)
                    .ToListAsync();
                if (result != null)
                {
                    productData.AddRange(mapper.Map<List<ProdGetDataDto>>(result));
                }
                else throw new Exception("查無商品資料");

                if (productData != null)
                {
                    output = (from p in productData
                              select new DirectoryReleInfoDto
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  ItemNo = p.ItemNo,
                                  Link = $"/product/{p.Id}",
                                  type = DirectoryTypeEnum.商品,
                                  Description = p.Description,
                                  SerNo = p.Ser_No,
                                  Status = p.Status,
                                  StatusName = Enum.GetName(typeof(ProdStatusEnum), (ProdStatusEnum)p.Status),
                                  tags = (from t in db.Tags.Where(e => e.FK_WebsiteId == WebsiteID)
                                          join a in db.Tag_Associates.Where(e => !e.IsDeleted)
                                                       .Where(e => e.FK_AId == p.Id)
                                                       .Where(e => e.Type == (int)TagAssociateTypeEnum.商品) on t.Id equals a.FK_TId
                                          group t by new { t.Id, t.Title } into g
                                          select new TagGetSelectedDto
                                          {
                                              FK_TId = g.Key.Id,
                                              Tag_Name = g.Key.Title
                                          }).ToList(),
                                  MainImage = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                                  .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == WebsiteID)
                                                  .Where(e => e.fileUpload != null && !e.IsDeleted && !e.fileUpload.IsDeleted)
                                                  .Where(e => e.Sid == p.Id && e.type == (int)FileBindTypeEnum.產品)
                                                  .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                                select new DirectoryReleInfoDto
                                                {
                                                    Link = (f.fileUpload.DownloadFileName ?? "").Replace("upload", $"upload/{orgName}").Replace("//", "/")
                                                }).FirstOrDefault() ?? new DirectoryReleInfoDto()).Link,
                              }).ToList();
                    for (int i = 0; i < output.Count; i++)
                    {
                        var data = output[i];
                        var s = await db.Prod_Stocks.Where(e => e.FK_Pid == data.Id).Where(e => !e.IsDeleted).Select(e => e.Id).ToListAsync();
                        var p = await db.Prod_Prices.Where(x => s.Contains(x.FK_PSId)).Where(e => !e.IsDeleted).ToListAsync();
                        double min = p.Min(e => e.Price) ?? 0;
                        double max = p.Max(e => e.Price) ?? 0;
                        if (min == max) data.Price = $"{max}";
                        else data.Price = $"{min} ~ {max}";

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

                    fileresponse = await fileUploadAppService.deleteFileById(new FileDeleteDto()
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
        public async Task<ResponseMessageDto> ClickLog(long FK_Pid)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var token = tokenAppService.CheckToken();
                Guid UUID = await tokenAppService.GetUUID();

                var prod = db.Prods.Where(e => e.Id == FK_Pid).FirstOrDefault();
                if (prod != null)
                {
                    prod.Clicks = prod.Clicks == null ? 1 : prod.Clicks + 1;
                    await loginUserData.SaveChanges(prod);

                    var userid = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e => e.FK_User).FirstOrDefaultAsync();

                    Core.Models.Prod_Log prod_log = new Core.Models.Prod_Log
                    {
                        FK_Pid = FK_Pid,
                        Action = (int)LogActionEnum.點擊,
                        UUID = UUID,
                        FK_UserId = userid,
                        Db_Name = "Prods"
                    };

                    db.Prod_Logs.Add(prod_log);
                    db.SaveChanges();
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
        /* Other Get */
        public async Task<ProdGetOneDto> GetDisplayOne(long id)
        {
            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var db_p = await db.Prods.Where(e => e.Id == id && e.FK_WebsiteId == websiteId)
                    .Where(e => !e.IsDeleted && (e.permanent || (DateTime.Now >= e.StartTime && DateTime.Now < e.EndTime)))
                    .FirstOrDefaultAsync();
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
                                    where !p.IsDeleted && p.Visible && p.FK_WebsiteId == webid
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
        public async Task<ProdGetHistoryDisplayAllDto> GetHistoryDisplay(int page)
        {
            var output = new ProdGetHistoryDisplayAllDto();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var prod_Logs = await (from prod_log in db.Prod_Logs
                                       where prod_log.UUID == UUID
                                       where prod_log.Action == (int)ProdLogActionEnum.點擊
                                       where (DateTime.Compare(DateTime.Now.AddDays(-30), (DateTime)prod_log.CreationTime) < 0)
                                       orderby prod_log.CreationTime descending
                                       select prod_log.FK_Pid).ToListAsync();
                List<long> pids = new List<long>();

                if (prod_Logs.Count > 0)
                {
                    foreach (var prod_log in prod_Logs)
                    {
                        if (!pids.Contains(prod_log))
                        {
                            pids.Add(prod_log);
                        }
                    }
                }

                if (pids.Count > 0)
                {
                    var prod_log_data = (from pid in pids
                                         join prod in db.Prods on pid equals prod.Id
                                         select new ProdGetHistoryDisplayOneDto()
                                         {
                                             PId = prod.Id,
                                             Title = prod.Title,
                                             Introduction = prod.Introduction,
                                             Description = prod.Description,
                                             Link = "/product/" + prod.Id,
                                             Image = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                                     .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == WebsiteId)
                                                     .Where(e => e.fileUpload != null)
                                                     .Where(e => e.Sid == prod.Id && e.type == (int)FileBindTypeEnum.產品)
                                                     .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                                       select new DirectoryReleInfoDto
                                                       {
                                                           Link = f.fileUpload != null ? f.fileUpload.DownloadFileName ?? "" : ""
                                                       }).FirstOrDefault() ?? new DirectoryReleInfoDto()).Link,
                                             Price = new List<double>(),
                                             ItemNo = prod.ItemNo,
                                         }).ToList();

                    output.Page_Total = (int)Math.Ceiling(prod_log_data.Count / 8.0);
                    prod_log_data = prod_log_data.Skip((page - 1) * 8).Take(8).ToList();

                    for (var i = 0; i < prod_log_data.Count; i++)
                    {
                        var prod_prices = await (from prod_stock in db.Prod_Stocks
                                                 join prod_price in db.Prod_Prices on prod_stock.Id equals prod_price.FK_PSId
                                                 where prod_stock.FK_Pid == prod_log_data[i].PId
                                                 where prod_price.Price > 0
                                                 orderby prod_price descending
                                                 select prod_price).ToListAsync();
                        if (prod_prices.Count > 1)
                        {
                            prod_log_data[i].Price.Add((double)prod_prices[0].Price);
                            prod_log_data[i].Price.Add((double)prod_prices[prod_prices.Count - 1].Price);
                        }
                        else if (prod_prices.Count == 1)
                        {
                            prod_log_data[i].Price.Add((double)prod_prices[0].Price);
                        }
                        else prod_log_data[i].Price.Add(0);
                    }

                    output.Data = prod_log_data;
                    output.Success = true;

                    return output;
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }
            return output;
        }
        /* Product Import */
        public async Task<ImportOutputDto> ProdReplace(IList<IFormFile> files)
        {
            ImportOutputDto response = new ImportOutputDto { ErrorList = new List<ImportMassageItem>() };
            ProdImportAllDto fileData = await importAppService.ProdReplace(files);
            long WebsiteID = await loginUserData.GetWebsiteId();
            if (fileData.Products.Any())
            {
                List<ProductImportDto> allData = fileData.Products.FindAll(e => !string.IsNullOrEmpty(e.ProdName));
                List<ProductImportDto> prods = new List<ProductImportDto>();
                List<string> allTitles = allData.Select(p => p.ProdName).ToList();
                List<string> allItemNos = allData.Select(p => p.ItemNo).ToList();
                var updateItems = db.Prods.Where(e => !e.IsDeleted)
                    .Where(p => string.IsNullOrEmpty(p.ItemNo) ? allTitles.Contains(p.Title) : allItemNos.Contains(p.ItemNo))
                    .Select(s => new { s.Id, s.ItemNo, s.Title }).ToList();
                ProductImportDto dto = null;
                for (int i = 0; i < allData.Count; i++)
                {
                    var el = allData[i];
                    var item = updateItems.Find(e => string.IsNullOrEmpty(el.ItemNo) ? e.Title == el.ProdName : e.ItemNo == el.ItemNo);
                    el.FK_WebsiteId = WebsiteID;
                    if (item != null) el.Id = item.Id;
                    var preProds = prods.Find(e => string.IsNullOrEmpty(el.ItemNo) ? e.ProdName == el.ProdName : e.ItemNo == el.ItemNo);
                    if (preProds == null)
                    {
                        dto = el;
                        dto.stocks = new List<ProductStockDto>();
                        prods.Add(el);
                    }
                    else dto = preProds;
                    if (dto != null && dto.stocks != null) dto.stocks.Add(mapper.Map<ProductStockDto>(el));
                }
                try
                {
                    await InsertOrUpdateProd(prods, response.ErrorList);
                    await ImportProdMediaLinks(prods, response.ErrorList);
                    await ImportProdTags(prods, response.ErrorList);
                    await importTechs(prods, response.ErrorList);
                    response.Success = true;
                }
                catch (Exception ex)
                {
                    response.ErrorList.Add(new ImportMassageItem { Name = "error", Description = ex.Message });
                }
            }
            if (fileData.Directories.Any()) await imporDirectories(fileData.Directories);

            return response;
        }
        private async Task imporDirectories(List<DirectoryImportDto> directories)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                List<string> manuNames = new List<string>();
                manuNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Level1)).Select(e => (e.Level1 ?? "").Trim()).ToList());
                manuNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Level2)).Select(e => (e.Level2 ?? "").Trim()).ToList());
                manuNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Level3)).Select(e => (e.Level3 ?? "").Trim()).ToList());

                List<string> tagNames = new List<string>();
                tagNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Tag1)).Select(e => (e.Tag1 ?? "").Trim()).ToList());
                tagNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Tag2)).Select(e => (e.Tag2 ?? "").Trim()).ToList());
                tagNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Tag3)).Select(e => (e.Tag3 ?? "").Trim()).ToList());

                await importMenus(WebsiteID, manuNames);
                await importTags(WebsiteID, tagNames);

                var menus = await db.WebMenus.Where(e => !e.IsDeleted)
                                .Where(e => !string.IsNullOrEmpty(e.Title) && manuNames.Contains(e.Title))
                                .Where(e => e.FK_WebsiteId == WebsiteID).ToListAsync();

                var Tags = await db.Tags.Where(e => !e.IsDeleted)
                               .Where(e => !string.IsNullOrEmpty(e.Title) && tagNames.Contains(e.Title))
                               .Where(e => e.FK_WebsiteId == WebsiteID).ToListAsync();

                List<DirectoryArrangeImportDto> menuMap = new List<DirectoryArrangeImportDto>();
                for (int i = 0; i < directories.Count; i++)
                {
                    var directory = directories[i];
                    DirectoryArrangeImportDto? item = menuMap.Find(e => e.Name == directory.Level1);
                    if (string.IsNullOrEmpty(directory.Level1)) break;

                    var menu = menus.Where(e => e.Title == directory.Level1).FirstOrDefault();
                    if (menu == null) break;

                    if (item == null)
                    {
                        item = new DirectoryArrangeImportDto { Id = menu.Id, Name = directory.Level1 };
                        menuMap.Add(item);
                    }
                    else item.Id = menu.Id;

                    if (string.IsNullOrEmpty(directory.Level2)) continue;
                    var menu2 = menus.Where(e => e.Title == directory.Level2).FirstOrDefault();
                    if (menu2 != null)
                    {
                        menu2.FK_TopNodeId = menu.Id;
                        menu2.FK_RootNodeId = menu.Id;
                        DirectoryArrangeImportDto? item2 = item.Child.Find(e => e.Name == directory.Level2);
                        if (item2 == null)
                        {
                            item2 = new DirectoryArrangeImportDto { Id = menu2.Id, Name = directory.Level2 };
                            item.Child.Add(item2);

                            if (string.IsNullOrEmpty(directory.Level3))
                            {
                                await addDirectoryToTags(directory, item2, Tags);
                            }
                            else
                            {
                                var menu3 = menus.Where(e => e.Title == directory.Level3).FirstOrDefault();
                                if (menu3 != null)
                                {
                                    menu3.FK_TopNodeId = menu2.Id;
                                    menu3.FK_RootNodeId = menu.Id;
                                    DirectoryArrangeImportDto? item3 = item.Child.Find(e => e.Name == directory.Level3);
                                    if (item3 == null)
                                    {
                                        item3 = new DirectoryArrangeImportDto { Id = menu3.Id, Name = directory.Level3 };
                                    }
                                    item2.Child.Add(item3);
                                    await addDirectoryToTags(directory, item3, Tags);
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(directory.Level3))
                            {
                                await addDirectoryToTags(directory, item2, Tags);
                            }
                            else
                            {
                                var menu3 = menus.Where(e => e.Title == directory.Level3).FirstOrDefault();
                                if (menu3 != null)
                                {
                                    menu3.FK_TopNodeId = menu2.Id;
                                    menu3.FK_RootNodeId = menu.Id;
                                    DirectoryArrangeImportDto? item3 = item.Child.Find(e => e.Name == directory.Level3);
                                    if (item3 == null)
                                    {
                                        item3 = new DirectoryArrangeImportDto { Id = menu3.Id, Name = directory.Level3 };
                                    }
                                    item2.Child.Add(item3);
                                    await addDirectoryToTags(directory, item3, Tags);
                                }
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();
                await createDirectory(menuMap);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private string getMenuInitHtml(SelectDto dto)
        {
            return HttpUtility.HtmlEncode($@"<div class=""container"">
                <div class=""two_three_block d-flex flex-wrap"">
                    <div class=""col-3"">
                        <div data-dirid=""6"" data-diridname=""頁左選單"" class=""menu_directory bg-white"">
                            <div class=""title custom_h5 fw-bold py-3 px-3""></div>
                            <div class=""accordion accordion-flush""></div>
                            <div id=""TemplateAccordionItem"" class=""d-none"">
                                <div class=""accordion-item border-0 border-bottom"">
                                    <div class=""accordion-header"">
                                        <button type=""button"" data-bs-toggle=""collapse"" data-bs-target="""" aria-expanded=""false"" aria-controls="""" class=""accordion-button collapsed custom_h5 sectitle""></button>
                                    </div>
                                    <div aria-labelledby="""" class=""accordion-collapse collapse"">
                                        <div class=""accordion-body p-0""></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class=""col"">
                        <div class=""custom_h3 my-2 fw-bold"">
                            <b>
                                <span>{dto.Name}</span>
                            </b>
                        </div>
                        <div class=""container edit_lock my-0"">
                            <div data-dirid=""{dto.Id}"" data-diridname=""{dto.Name}"" class=""frame type_change_frame catalog_frame allowedit"">
                                <div class=""d-flex justify-content-end switch_control allowedit mb-2"">
                                    <button id=""ig77w"" class=""btn_prod_grid d-flex bg-transparent border-0 align-items-center mx-1 allowedit"">
                                        <span class=""material-symbols-outlined fs-5 me-1"">grid_on</span>商品圖片
                                    </button>
                                    <button id=""i13wzg5"" class=""btn_prod_list d-flex bg-transparent border-0 align-items-center mx-1 text-black-50 allowedit"">
                                        <span class=""material-symbols-outlined fs-5 me-1"">view_list</span>商品圖文
                                    </button>
                                </div>
                                <div class=""catalog content row row-cols-lg-4 gx-0 rounded-lg bg-light px-2"">
                                    <div class=""templatecontent d-none"">
                                        <div class=""template p-2 py-2"">
                                            <div class=""col bg-white p-2 position-sticky p-1 type4 rounded-lg h-100"">
                                                <a href="""" title="""" target=""_self"" class=""text-black"">
                                                    <figure class=""d-flex justify-content-center mb-0 h-100 max-h flex-column"">
                                                        <div class=""image_frame d-flex flex-grow-1 justify-content-center align-items-center type4-image-frame w-100"">
                                                            <img src=""/upload/Product/Photo/C656NA.jpg"" alt="""" class=""image gjs-plh-image img-fluid"" />
                                                        </div>
                                                        <figcaption class=""w-100 position-relative pb1 type4-caption d-flex flex-column"">
                                                            <div class=""item-header d-flex"">
                                                                <div class=""itemNo m-0 p-0 align-itmes-center type4-title d-inline fw-bold"">
                                                                    {{產品編號}}
                                                                </div>
                                                            </div>
                                                            <div class=""item-title"">
                                                                <div class=""catalog-number itemNo m-0 p-2 align-itmes-center type4-title d-inline"">
                                                                    {{產品編號}}
                                                                </div>
                                                                <div class=""title m-0 fs-5 align-itmes-center type4-title d-inline fs-6"">
                                                                </div>
                                                                <div class=""like-and-share top_line d-none"">
                                                                    <div class=""btn_favorites bg-transparent border-0 d-none"">
                                                                        <i class=""fs-5 fa-regular fa-heart"">
                                                                        </i><span class=""d-none"">關注</span>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <p class=""description mt-1 overflow-hidden p-2 d-none type2-content"">
                                                            </p>
                                                            <div class=""more-btn position-absolute d-flex justify-content-center align-items-center d-none"">
                                                                <div class=""fas fa-angle-right"">
                                                                </div>
                                                            </div>
                                                            <div class=""date d-flex justify-content-end align-items-center p-2 d-none"">
                                                            </div>
                                                            <div class=""more text-end mt-1 d-none"">
                                                                詳細介紹
                                                            </div>
                                                            <div class=""price price-grid mt-auto type2-title d-inline fw-bold fs-7"">{{price}}</div>
                                                            <div class=""bottom-row d-flex align-text-bottom"">
                                                                <div class=""tags""></div>
                                                                <div class=""purchase d-none"">
                                                                    <div class=""price price-discount me-auto p-2 align-itmes-center type2-title d-none"">
                                                                        {{min price}}
                                                                    </div>
                                                                    <div class=""price normal-price me-auto p-2 align-itmes-center type2-title d-inline fw-bold"">
                                                                        {{max price}}
                                                                    </div>
                                                                    <span class=""cart badge rounded-pill bg-secondary text-white me-auto p-2 px-4 align-itmes-center type2-title d-none fw-normal"">
                                                                        <i class=""fa-solid fa-cart-shopping fa-inverse""></i>
                                                                        放入購物車
                                                                    </span>
                                                                </div>
                                                            </div>
                                                        </figcaption>
                                                    </figure>
                                                </a>
                                                <div class=""shareBlock"">
                                                    <button class=""btn_share bg-transparent border-0"">
                                                        <i class=""fs-5 fa-solid fa-share"">
                                                        </i><span class=""d-none"">分享</span>
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class=""templatecontent-tag d-none"">
                                        <span class=""badge rounded-pill bg-light text-secondary fw-normal me-1 px-2"">{{Tag Name}}</span>
                                    </div>
                                </div>
                                <nav aria-label=""Catalog Page"">
                                    <ul class=""page_btn d-flex justify-content-center my-5 pagination"">
                                        <li class=""page-item btn_prev"">
                                            <button class=""page-link text-black"">
                                                <i class=""fa-solid fa-angle-left""></i>
                                            </button>
                                        </li>
                                        <li class=""page-item btn_next"">
                                            <button class=""page-link text-black"">
                                                <i class=""fa-solid fa-angle-right""></i>
                                            </button>
                                        </li>
                                    </ul>
                                </nav>
                            </div>
                        </div>
                        <!-- Content here -->
                    </div>
                </div>
                <!-- Content here -->
            </div>");
        }
        private async Task createDirectory(List<DirectoryArrangeImportDto> menuMap)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            long UserID = await loginUserData.GetUserId();
            List<string> strings = menuMap.Where(e => !string.IsNullOrEmpty(e.Name)).Select(e => e.Name).ToList();
            List<Core.Models.Directory> Directory = new List<Core.Models.Directory>();
            List<Tag_Associate> associates = new List<Tag_Associate>();
            List<Core.Models.Directory> oldDirectory = await db.Directory
                .Where(e => !e.IsDeleted)
                .Where(e => e.FK_WebsiteId == WebsiteID)
                .Where(e => strings.Contains(e.Title)).ToListAsync();
            var webMenu = await db.WebMenus.Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == WebsiteID)
                .Where(e => !string.IsNullOrEmpty(e.Title) && strings.Contains(e.Title)).ToListAsync();
            var TagAssociate = await db.Tag_Associates.Include(t => t.Tag)
                    .Where(e => !e.IsDeleted)
                    .Where(e => e.Type == (int)TagAssociateTypeEnum.目錄)
                    .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID).ToListAsync();
            for (int i = 0; i < menuMap.Count; i++)
            {
                var menu = menuMap[i];
                if (menu.Child.Any()) await createDirectory(menu.Child);
                else
                {
                    var dir = oldDirectory.Where(e => e.Title == menu.Name).FirstOrDefault();
                    if (dir == null)
                    {
                        dir = new Core.Models.Directory
                        {
                            FK_WebsiteId = WebsiteID,
                            Title = menu.Name,
                            Type = (int)DirectoryTypeEnum.商品
                        };
                        db.Directory.Add(dir);
                        await loginUserData.SaveChanges(dir);
                    }
                    if (menu.Tags != null && menu.Tags.Any())
                    {
                        var tagIds = menu.Tags.FindAll(e => e.Id != null).Select(e => e.Id).ToList();
                        menu.Tags.ForEach(tag =>
                        {
                            if (tag.Id != null && !TagAssociate.Exists(e => e.FK_AId == dir.Id && e.FK_TId == tag.Id))
                            {
                                Tag_Associate associate = new Tag_Associate
                                {
                                    FK_AId = dir.Id,
                                    FK_TId = tag.Id.Value,
                                    Type = (int)TagAssociateTypeEnum.目錄
                                };
                                loginUserData.setOptionParameter(associate, UserID);
                                associates.Add(associate);
                            }
                        });
                        var oldTagBind = TagAssociate.FindAll(e => e.FK_AId == dir.Id && !tagIds.Contains(e.FK_TId)).ToList();
                        for (int j = 0; j < oldTagBind.Count(); j++)
                        {
                            oldTagBind[i].IsDeleted = true;
                            await loginUserData.setOptionParameter(oldTagBind[i]);
                        }
                    }
                    var myMenu = webMenu.Where(e => e.Title == menu.Name).FirstOrDefault();
                    if (myMenu != null && string.IsNullOrEmpty(myMenu.SaveHtml) && !string.IsNullOrEmpty(dir.Title))
                    {
                        myMenu.Html = getMenuInitHtml(new SelectDto { Id = dir.Id, Name = dir.Title });
                        myMenu.SaveHtml = myMenu.Html;
                    }
                }
            };
            db.Tag_Associates.AddRange(associates);
            await db.SaveChangesAsync();
        }
        private async Task addDirectoryToTags(DirectoryImportDto directory, DirectoryArrangeImportDto item, List<Core.Models.Tag> Tags)
        {
            item.Tags = new List<TagGetSelectedDto>();
            if (!string.IsNullOrEmpty(directory.Tag1))
            {
                var tag1 = Tags.Where(e => e.Title == directory.Tag1).FirstOrDefault();
                if (tag1 != null) item.Tags.Add(new TagGetSelectedDto { Id = tag1.Id, Tag_Name = tag1.Title });
            }
            if (!string.IsNullOrEmpty(directory.Tag2))
            {
                var tag2 = Tags.Where(e => e.Title == directory.Tag2).FirstOrDefault();
                if (tag2 != null) item.Tags.Add(new TagGetSelectedDto { Id = tag2.Id, Tag_Name = tag2.Title });
            }
            if (!string.IsNullOrEmpty(directory.Tag3))
            {
                var tag3 = Tags.Where(e => e.Title == directory.Tag3).FirstOrDefault();
                if (tag3 != null) item.Tags.Add(new TagGetSelectedDto { Id = tag3.Id, Tag_Name = tag3.Title });
            }
        }
        private async Task importMenus(long WebsiteID, List<string> manuNames)
        {
            var menus = await db.WebMenus.Where(e => !e.IsDeleted)
                        .Where(e => e.FK_WebsiteId == WebsiteID)
                        .Where(e => !string.IsNullOrEmpty(e.Title) && manuNames.Contains(e.Title))
                        .ToListAsync();
            var hasMenusTitle = menus.Select(e => e.Title).ToArray();

            var needAddMenus = manuNames.Where(e => !hasMenusTitle.Contains(e)).ToList();
            List<SelectDto> addMmenus = new List<SelectDto>();
            needAddMenus.ForEach(e =>
            {
                if (!addMmenus.Exists(m => m.Name == e))
                    addMmenus.Add(new SelectDto { Name = e });
            });
            await webMenuApplication.insertMenus(addMmenus);
        }
        private async Task importTags(long WebsiteID, List<string> tagNames)
        {
            long userId = await loginUserData.GetUserId();
            var tags = await db.Tags.Where(e => !e.IsDeleted)
               .Where(e => e.FK_WebsiteId == WebsiteID)
               .Where(e => !string.IsNullOrEmpty(e.Title) && tagNames.Contains(e.Title))
               .ToListAsync();
            var hasTagsTitle = tags.Select(e => e.Title).ToArray();
            var needAddTagss = tagNames.Where(e => !hasTagsTitle.Contains(e)).ToList();
            List<SelectDto> addTags = new List<SelectDto>();
            needAddTagss.ForEach(e =>
            {
                if (!addTags.Exists(m => m.Name == e))
                    addTags.Add(new SelectDto { Name = e });
            });

            var newTags = mapper.Map<List<Core.Models.Tag>>(addTags);
            newTags.ForEach(e =>
            {
                e.FK_WebsiteId = WebsiteID;
                e.CreatorUserId = userId;
                e.CreationTime = DateTime.Now;
                e.IsDeleted = false;
            });
            db.Tags.AddRange(newTags);
            db.SaveChanges();
        }
        private async Task importTechs(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            List<TechCertDto> allTech = new List<TechCertDto>();
            for (int i = 0; i < prods.Count; i++)
            {
                var prod = prods[i];
                if (prod.Techs != null) allTech.AddRange(prod.Techs);
            }
            await technicalCertificateAppService.AddAll(allTech);
            await importProdTech(prods, errors);
        }
        private async Task importProdTech(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            var prodGroup = prods.GroupBy(x => new { x.ItemNo, x.ProdName }).Select(e => new { e.Key.ItemNo, e.Key.ProdName }).ToList();
            var prodTitles = prodGroup.Select(e => e.ProdName).ToList();
            var prodItemNos = prodGroup.Select(e => e.ItemNo).ToList();
            var crrenProds = db.Prods.Where(e => !e.IsDeleted)
                    .Where(e => string.IsNullOrEmpty(e.ItemNo) ? prodTitles.Contains(e.Title) : prodItemNos.Contains(e.ItemNo))
                    .Select(e => new { e.Id, e.Title, e.ItemNo }).ToList();
            var techs = db.TechnicalCertificates.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Title }).ToList();

            List<TechCertProdAssociateDto> techCertProdAssociateDtos = new List<TechCertProdAssociateDto>();
            for (int i = 0; i < prods.Count; i++)
            {
                var prod = prods[i];
                var n = crrenProds.Find(e => string.IsNullOrEmpty(e.ItemNo) ? e.Title == prod.ProdName : e.ItemNo == prod.ItemNo);
                if (n == null || prod.Techs == null) continue;
                for (int j = 0; j < prod.Techs.Count; j++)
                {
                    var item = prod.Techs[j];
                    var tec = techs.Find(e => e.Title == item.Title);
                    if (tec != null)
                    {
                        techCertProdAssociateDtos.Add(new TechCertProdAssociateDto
                        {
                            FK_PId = n.Id,
                            FK_TCId = tec.Id,
                            IsDeleted = false,
                        });
                    }
                }
            }
            await technicalCertificateAppService.TechCertAssociateAddDelect(techCertProdAssociateDtos);
        }
        private async Task ImportProdTags(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            long WebsiteId = await loginUserData.GetWebsiteId();
            long userId = await loginUserData.GetUserId();
            List<string?> TagStr = prods.Where(e => !string.IsNullOrEmpty(e.Tag1)).Select(e => e.Tag1).ToList();
            List<string?> TagStr2 = prods.Where(e => !string.IsNullOrEmpty(e.Tag2)).Select(e => e.Tag2).ToList();
            List<string?> TagStr3 = prods.Where(e => !string.IsNullOrEmpty(e.Tag3)).Select(e => e.Tag3).ToList();
            List<string?> TagStr4 = prods.Where(e => !string.IsNullOrEmpty(e.Tag4)).Select(e => e.Tag4).ToList();
            List<string?> TagStr5 = prods.Where(e => !string.IsNullOrEmpty(e.Tag5)).Select(e => e.Tag5).ToList();
            List<string?> TagStr6 = prods.Where(e => !string.IsNullOrEmpty(e.Tag6)).Select(e => e.Tag6).ToList();
            List<string?> ProdStr = prods.Where(e => !string.IsNullOrEmpty(e.ProdName)).Select(e => e.ProdName).ToList();
            TagStr.AddRange(TagStr2);
            TagStr.AddRange(TagStr3);
            TagStr.AddRange(TagStr4);
            TagStr.AddRange(TagStr5);
            TagStr.AddRange(TagStr6);
            TagStr = TagStr.Where(e => !string.IsNullOrEmpty(e)).GroupBy(e => e).Select(e => e.Key).ToList();

            List<string> nowTags = db.Tags.Where(e => e.FK_WebsiteId == WebsiteId)
                                    .Where(e => !e.IsDeleted)
                                    .Select(e => e.Title).ToList();

            TagStr = TagStr.FindAll(e => !nowTags.Contains(e ?? ""));
            List<Core.Models.Tag> addTads = new List<Core.Models.Tag>();
            for (int i = 0; i < TagStr.Count; i++)
            {
                string? title = TagStr[i];
                if (!string.IsNullOrEmpty(title))
                {
                    addTads.Add(new Core.Models.Tag
                    {
                        Title = title,
                        FK_WebsiteId = WebsiteId,
                        CreatorUserId = userId,
                        CreationTime = DateTime.Now,
                    });
                }
            }
            db.Tags.AddRange(addTads);
            await db.SaveChangesAsync();

            await ImportProdAssociates(prods, errors);
        }
        private async Task ImportProdAssociates(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            long WebsiteId = await loginUserData.GetWebsiteId();
            var nowTags = db.Tags.Where(e => e.FK_WebsiteId == WebsiteId)
                                    .Where(e => !e.IsDeleted)
                                    .Select(e => new { e.Id, e.Title }).ToList();
            var allProd = db.Prods.Where(e => e.FK_WebsiteId == WebsiteId)
                                    .Where(e => !e.IsDeleted)
                                    .Select(e => new { e.Id, e.Title, e.ItemNo }).ToList();
            List<TagAssociateDto> TagAssociates = new List<TagAssociateDto>();
            for (int i = 0; i < prods.Count; i++)
            {
                var item = prods[i];
                var el = allProd.Find(e => e.Title == item.ProdName && e.ItemNo == item.ItemNo);
                if (el == null)
                {
                    errors.Add(new ImportMassageItem
                    {
                        Name = item.ProdName,
                        Description = "商品標籤榜定失敗。"
                    });
                    continue;
                }
                item.Id = allProd.Find(e => e.Title == item.ProdName && e.ItemNo == item.ItemNo).Id;
                var tag = nowTags.FindAll(e => e.Title == item.Tag1 || e.Title == item.Tag2 || e.Title == item.Tag3 || e.Title == item.Tag4 || e.Title == item.Tag5 || e.Title == item.Tag6);
                if (tag != null)
                {
                    for (int j = 0; j < tag.Count; j++)
                    {
                        TagAssociates.Add(new TagAssociateDto
                        {
                            Type = (int)TagAssociateTypeEnum.商品,
                            FK_TId = tag[j].Id,
                            FK_AId = item.Id,
                            IsDeleted = false
                        });
                    }
                }
            }
            await tagAppService.TagAssociateAddDelect(TagAssociates);
        }
        private async Task ImportProdMediaLinks(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            List<string?> ImagStr = prods.Where(e => !string.IsNullOrEmpty(e.Image1)).Select(e => e.Image1).ToList();
            List<string?> ImagStr2 = prods.Where(e => !string.IsNullOrEmpty(e.Image2)).Select(e => e.Image2).ToList();
            List<string?> ImagStr3 = prods.Where(e => !string.IsNullOrEmpty(e.Image3)).Select(e => e.Image3).ToList();
            List<string?> ImagStr4 = prods.Where(e => !string.IsNullOrEmpty(e.Image4)).Select(e => e.Image4).ToList();
            List<string?> ImagStr5 = prods.Where(e => !string.IsNullOrEmpty(e.Image5)).Select(e => e.Image5).ToList();
            List<string?> ImagStr6 = prods.Where(e => !string.IsNullOrEmpty(e.Image6)).Select(e => e.Image6).ToList();
            List<string?> ImagStr7 = prods.Where(e => !string.IsNullOrEmpty(e.Image7)).Select(e => e.Image7).ToList();
            List<string?> ProdStr = prods.Where(e => !string.IsNullOrEmpty(e.ProdName)).Select(e => e.ProdName).ToList();
            ImagStr.AddRange(ImagStr2);
            ImagStr.AddRange(ImagStr3);
            ImagStr.AddRange(ImagStr4);
            ImagStr.AddRange(ImagStr5);
            ImagStr.AddRange(ImagStr6);
            ImagStr.AddRange(ImagStr7);
            ImagStr = ImagStr.Where(e => !string.IsNullOrEmpty(e)).GroupBy(e => e).Select(e => e.Key).ToList();
            List<FileImageImportDto> importDtos = new List<FileImageImportDto>();
            var fileProds = db.Prods.Where(e => !e.IsDeleted).Where(e => ProdStr.Contains(e.Title)).ToList();
            foreach (var prod in prods)
            {
                var myProd = fileProds.Where(e => e.Title == prod.ProdName && e.ItemNo == prod.ItemNo).FirstOrDefault();
                if (myProd != null)
                {
                    List<string?> fileName =
                        ImagStr.FindAll(e => e == prod.Image1 || e == prod.Image2 || e == prod.Image3 || e == prod.Image4 || e == prod.Image5 || e == prod.Image6 || e == prod.Image7);
                    for (int i = 0; i < fileName.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(fileName[i]))
                        {
                            importDtos.Add(new FileImageImportDto
                            {
                                SId = myProd.Id,
                                Type = FileBindTypeEnum.產品,
                                mediaLink = fileName[i] ?? "",
                                SerNo = 500
                            });
                        }
                    }
                }
            }
            await fileUploadAppService.uploadImageLink(importDtos);
            await ImportProdDownloadFileLinks(prods, errors);
        }
        private async Task ImportProdDownloadFileLinks(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            List<string?> FileStr = prods.Where(e => !string.IsNullOrEmpty(e.File1)).Select(e => e.File1).ToList();
            List<string?> FileStr2 = prods.Where(e => !string.IsNullOrEmpty(e.File2)).Select(e => e.File2).ToList();
            List<string?> FileStr3 = prods.Where(e => !string.IsNullOrEmpty(e.File3)).Select(e => e.File3).ToList();
            List<string?> FileStr4 = prods.Where(e => !string.IsNullOrEmpty(e.File4)).Select(e => e.File4).ToList();
            List<string?> FileStr5 = prods.Where(e => !string.IsNullOrEmpty(e.File5)).Select(e => e.File5).ToList();
            List<string?> FileNameStr = prods.Where(e => !string.IsNullOrEmpty(e.FileName1)).Select(e => e.FileName1).ToList();
            List<string?> FileNameStr2 = prods.Where(e => !string.IsNullOrEmpty(e.FileName2)).Select(e => e.FileName2).ToList();
            List<string?> FileNameStr3 = prods.Where(e => !string.IsNullOrEmpty(e.FileName3)).Select(e => e.FileName3).ToList();
            List<string?> FileNameStr4 = prods.Where(e => !string.IsNullOrEmpty(e.FileName4)).Select(e => e.FileName4).ToList();
            List<string?> FileNameStr5 = prods.Where(e => !string.IsNullOrEmpty(e.FileName5)).Select(e => e.FileName5).ToList();
            List<string?> ProdStr = prods.Where(e => !string.IsNullOrEmpty(e.ProdName)).Select(e => e.ProdName).ToList();
            FileStr.AddRange(FileStr2);
            FileStr.AddRange(FileStr3);
            FileStr.AddRange(FileStr4);
            FileStr.AddRange(FileStr5);
            FileStr = FileStr.Where(e => !string.IsNullOrEmpty(e)).GroupBy(e => e).Select(e => e.Key).ToList();

            FileNameStr.AddRange(FileNameStr2);
            FileNameStr.AddRange(FileNameStr3);
            FileNameStr.AddRange(FileNameStr4);
            FileNameStr.AddRange(FileNameStr5);
            FileNameStr = FileNameStr.Where(e => !string.IsNullOrEmpty(e)).GroupBy(e => e).Select(e => e.Key).ToList();
            List<FileImageImportDto> importDtos = new List<FileImageImportDto>();
            var fileProds = db.Prods.Where(e => !e.IsDeleted).Where(e => ProdStr.Contains(e.Title)).ToList();
            foreach (var prod in prods)
            {
                var myProd = fileProds.Where(e => e.Title == prod.ProdName && e.ItemNo == prod.ItemNo).FirstOrDefault();
                if (myProd != null)
                {
                    List<string?> fileLink =
                        FileStr.FindAll(e => e == prod.File1 || e == prod.File2 || e == prod.File3 || e == prod.File4 || e == prod.File5);
                    List<string?> fileName =
                        FileNameStr.FindAll(e => e == prod.FileName1 || e == prod.FileName2 || e == prod.FileName3 || e == prod.FileName4 || e == prod.FileName5);
                    if (fileLink.Count() != fileName.Count())
                    {
                        int ll = fileLink.Count();
                        int nl = fileName.Count();
                        int all = ll + nl;
                    }
                    for (int i = 0; i < fileLink.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(fileLink[i]))
                        {
                            string l = fileLink[i], n = "";
                            if (i < fileName.Count()) n = fileName[i];
                            importDtos.Add(new FileImageImportDto
                            {
                                SId = myProd.Id,
                                Type = FileBindTypeEnum.產品檔案,
                                Name = fileName[i] ?? "",
                                mediaLink = fileLink[i] ?? "",
                                SerNo = 500
                            });
                        }
                    }
                }
            }
            await fileUploadAppService.uploadImageLink(importDtos);
        }
        private async Task InsertOrUpdateProd(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            List<ProductImportDto> AddProds = prods.FindAll(e => e.Id == 0);
            List<ProductImportDto> Prods = prods.FindAll(e => e.Id != 0);
            await InsetProdSpecTypes(prods);
            await InsetProdSpec(prods);

            if (AddProds.Count != 0) await InsertProds(AddProds, errors);
            if (Prods.Count != 0) await UpdateProds(Prods, errors);
        }
        private async Task InsertProds(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            long userId = await loginUserData.GetUserId();
            List<Prod> news = mapper.Map<List<Prod>>(prods);
            /*List<Prod> news = new List<Prod>();
            foreach (ProductImportDto p in prods)
            {
                try
                {
                    news.Add(mapper.Map<Prod>(p));
                }
                catch (Exception e)
                {
                    errors.Add(new ImportMassageItem { Name = p.ProdName, Description = e.Message });
                }
            }*/
            foreach (Prod prod in news)
            {
                try
                {
                    var item = prods.Find(p => string.IsNullOrEmpty(prod.ItemNo) ? p.ProdName == prod.Title : p.ItemNo == prod.ItemNo);
                    if (item != null && item.stocks != null)
                    {
                        prod.Prod_Stocks = await InsertOrUpdateStore(item);
                        prod.Visible = true;
                        if (prod.Html != null)
                        {
                            prod.Html = $"<div class=\"container\">{prod.Html.Trim().Replace(Environment.NewLine, "<br />")}</div>";
                            prod.SaveHtml = prod.Html;
                        }
                        prod.RemovedFromShelves = false;
                        ProdStatusEnum statusType;
                        if (Enum.TryParse(item.Status, out statusType))
                        {
                            prod.Status = (int)statusType;
                        }
                        else prod.Status = 0;
                    }
                    prod.CreatorUserId = userId;
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportMassageItem { Name = prod.Title, Description = ex.Message });
                }
            }
            db.AddRange(news);
            await db.SaveChangesAsync();
        }
        private async Task UpdateProds(List<ProductImportDto> prods, List<ImportMassageItem> errors)
        {
            long userId = await loginUserData.GetUserId();
            List<string> titles = prods.Select(e => e.ProdName ?? "").ToList();
            List<string> itemNos = prods.Select(e => e.ItemNo ?? "").ToList();
            var items = await db.Prods.Where(e => !e.IsDeleted)
                .Where(e => string.IsNullOrEmpty(e.ItemNo) ? titles.Contains(e.Title) : itemNos.Contains(e.ItemNo))
                .ToListAsync();
            foreach (var prod in items)
            {
                try
                {
                    ProductImportDto? item = prods.Find(e => string.IsNullOrEmpty(e.ItemNo) ? e.ProdName == prod.Title : e.ItemNo == prod.ItemNo);
                    if (item != null)
                    {
                        var s = mapper.Map<ProductImportUpateDto>(item);
                        mapper.Map(s, prod);
                        if (item.stocks != null)
                        {
                            prod.Prod_Stocks = await InsertOrUpdateStore(item);
                        }
                        if (prod.Html != null)
                        {
                            prod.Html = $"<div class=\"container\"><p>{prod.Html.Trim().Replace("\n", "<br />")}</p></div>";
                            prod.SaveHtml = prod.Html;
                            prod.Css = "";
                            prod.SaveCss = "";
                        }
                        if (prod.ItemNo == "C206-12")
                        {
                            Console.WriteLine(prod.ItemNo);
                        }
                        ProdStatusEnum statusType;
                        if (Enum.TryParse(item.Status, out statusType))
                        {
                            prod.Status = (int)statusType;
                        }
                        else prod.Status = 0;
                    }
                    prod.LastModifierUserId = userId;
                    prod.LastModificationTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportMassageItem { Name = prod.Title, Description = ex.Message });
                }
            }
            await db.SaveChangesAsync();
        }
        private async Task<List<Prod_Stock>> InsertOrUpdateStore(ProductImportDto item)
        {
            var ProdSpecTitleList = db.Prod_Specs.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Title }).ToList();

            List<Prod_Stock> stockDto = new List<Prod_Stock>();
            if (item.stocks == null) item.stocks = new List<ProductStockDto>();

            for (int i = 0; i < item.stocks.Count; i++)
            {
                Prod_Stock? myStore = null;
                var stock = item.stocks[i];
                var r1 = ProdSpecTitleList.Find(e => e.Title == stock.S1_Title);
                var r2 = ProdSpecTitleList.Find(e => e.Title == stock.S2_Title);
                if (r1 != null) stock.FK_S1id = r1.Id;
                else stock.FK_S1id = 0;
                if (r2 != null) stock.FK_S2id = r2.Id;
                else stock.FK_S2id = 0;

                if (item.Id != 0)
                {
                    myStore = await db.Prod_Stocks
                            .Where(e => !e.IsDeleted)
                            .Where(e => e.FK_S1id == stock.FK_S1id)
                            .Where(e => e.FK_S2id == stock.FK_S2id)
                            .Where(e => e.FK_Pid == item.Id)
                            .FirstOrDefaultAsync();

                    if (myStore != null)
                    {
                        myStore.Stock = stock.Stock;
                        myStore.SubItemNo = stock.SubItemNo;
                        stockDto.Add(myStore);
                    }
                    else
                    {
                        myStore = mapper.Map<Prod_Stock>(stock);
                        myStore.Id = 0;
                        stockDto.Add(myStore);
                    }
                }
                else
                {
                    myStore = mapper.Map<Prod_Stock>(stock);
                    stockDto.Add(myStore);
                }
                myStore.Prod_Prices = mapper.Map<List<Prod_Price>>(stock.Prices);

            }
            return stockDto;
        }
        private async Task InsetProdSpecTypes(List<ProductImportDto> prods)
        {
            if (prods.Count == 0) return;
            long userId = await loginUserData.GetUserId();
            long WebsiteId = await loginUserData.GetWebsiteId();
            var ProdSpecTitleList = db.Prod_Spec_Types
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => e.FK_WebsiteId == prods[0].FK_WebsiteId)
                                    .Select(e => e.Type).ToList();
            List<Prod_Spec_Type> news = new List<Prod_Spec_Type>();
            for (int i = 0; i < prods.Count; i++)
            {
                var items = prods[i];
                if (items.stocks != null)
                {
                    var Adds1 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S1_Name ?? "")).Select(e => e.S1_Name).ToList();
                    var Adds2 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S2_Name ?? "")).Select(e => e.S2_Name).ToList();
                    Adds1.AddRange(Adds2);

                    var allAdds = Adds1.GroupBy(o => o ?? "").Select(o => o.Key).ToList();
                    var nowTitle = news.Select(e => e.Type);
                    var Adds = allAdds.FindAll(e => !nowTitle.Contains(e));

                    for (int j = 0; j < Adds.Count; j++)
                    {
                        var item = Adds[j];
                        if (!string.IsNullOrEmpty(item))
                        {
                            news.Add(new Prod_Spec_Type
                            {
                                Type = item,
                                FK_WebsiteId = items.FK_WebsiteId ?? 0,
                                CreationTime = DateTime.Now,
                                CreatorUserId = userId
                            });
                        }
                    }
                }
            }
            if (news.Count == 0 && ProdSpecTitleList.Count == 0)
            {
                news.Add(new Prod_Spec_Type
                {
                    Type = "規格",
                    FK_WebsiteId = WebsiteId,
                    CreationTime = DateTime.Now,
                    CreatorUserId = userId
                });
            }
            db.Prod_Spec_Types.AddRange(news);
            await db.SaveChangesAsync();
        }
        private async Task InsetProdSpec(List<ProductImportDto> prods)
        {
            long userId = await loginUserData.GetUserId();
            var ProdSpecTitleList = db.Prod_Specs
                                    .Where(e => !e.IsDeleted)
                                    .Select(e => e.Title).ToList();
            List<Prod_Spec> news = new List<Prod_Spec>();
            for (int i = 0; i < prods.Count; i++)
            {
                var items = prods[i];
                if (items.stocks != null)
                {
                    var Adds1 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S1_Title ?? "")).Select(e => new { name = e.S1_Name, title = e.S1_Title }).ToList();
                    var Adds2 = items.stocks.FindAll(e => !ProdSpecTitleList.Contains(e.S2_Title ?? "")).Select(e => new { name = e.S2_Name, title = e.S2_Title }).ToList();
                    Adds1.AddRange(Adds2);

                    var allAdds = Adds1
                                .Where(e => e.title != "")
                                .Where(e => e.title != null)
                                .GroupBy(o => o.title).Select(o => o.Key).ToList();

                    var nowTitle = news.Select(e => e.Title);
                    var Adds = allAdds.FindAll(e => !nowTitle.Contains(e));

                    var types = db.Prod_Spec_Types.Where(e => !e.IsDeleted).Select(e => new { e.Id, e.Type }).ToList();
                    var specs = db.Prod_Specs.Where(e => !e.IsDeleted).Select(e => new { e.FK_Tid, e.Title }).ToList();

                    for (int j = 0; j < Adds.Count; j++)
                    {
                        var item = Adds[j];
                        if (!string.IsNullOrEmpty(item))
                        {
                            var Spec = Adds1.Find(e => e.title == item);
                            string SpecTypeName = "";
                            if (Spec != null) SpecTypeName = Spec.name ?? "";
                            var SpecType = types.Find(o => o.Type == SpecTypeName);
                            long SpecTypeId = types.Count == 0 ? 0 : types.FirstOrDefault().Id;
                            if (SpecType == null)
                            {
                                var c = specs.Find(e => e.Title == item);
                                if (c != null) SpecTypeId = c.FK_Tid;
                            }
                            else SpecTypeId = SpecType.Id;
                            if (SpecTypeId != 0)
                            {
                                news.Add(new Prod_Spec
                                {
                                    Title = item,
                                    FK_Tid = SpecTypeId,
                                    CreationTime = DateTime.Now,
                                    CreatorUserId = userId
                                });
                            }

                        }
                    }
                }
            }
            db.Prod_Specs.AddRange(news);
            await db.SaveChangesAsync();
        }
        public async Task<GetProdContenDto> GetConten(SearchIDDto dto)
        {
            GetProdContenDto results = new GetProdContenDto();
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var prod = await db.Prods.Where(e => e.FK_WebsiteId == siteId)
                                    .Where(e => e.Id == dto.Id)
                                    .Where(e => !e.IsDeleted)
                                    .FirstOrDefaultAsync();
                if (prod != null)
                {
                    results.Conten = new ProdSaveContenDto
                    {
                        SaveHtml = prod.SaveHtml,
                        SaveCss = prod.SaveCss
                    };
                    results.Conten.SaveHtml = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(results.Conten.SaveHtml));
                    results.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                results.Success = false;
                results.Error = ex.Message;
            }
            return results;
        }
        public async Task<ResponseMessageDto> ImportConten(ProdSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var userId = await loginUserData.GetUserId();

                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
                ProdContenDto importDto = new ProdContenDto
                {
                    Id = dto.Id,
                    Html = dto.SaveHtml,
                    Css = dto.SaveCss
                };
                var s = await SaveConten(dto);
                var user = await loginUserData.GetUser();
                var prod = await db.Prods.FirstOrDefaultAsync(e => e.Id == dto.Id);
                if (prod != null)
                {
                    string Orgname = await loginUserData.GetWebsiteOrgName();
                    importDto.Html = (importDto.Html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.Css = (importDto.Css ?? "").Replace($"/upload/{Orgname}/", "/upload/");

                    prod.Html = importDto.Html;
                    prod.Css = importDto.Css;
                    prod.LastModificationTime = DateTime.Now;
                    prod.LastModifierUserId = userId;

                    await loginUserData.SaveChanges(prod);
                    response.Success = true;
                }
                else throw new Exception("資料不存在");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> SaveConten(ProdSaveContenDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
                var user = await loginUserData.GetUser();
                var prod = await db.Prods.FirstOrDefaultAsync(e => e.Id == dto.Id);

                prod.SaveHtml = dto.SaveHtml;
                prod.SaveCss = dto.SaveCss;
                prod.LastModificationTime = DateTime.Now;
                prod.LastModifierUserId = user.Id;

                db.SaveChanges();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }
            return response;
        }
        public async Task<GetFrontContenOutputDto> GetFrontConten(ProdGetFrontContenInputDto dto)
        {
            if (dto.siteId == null)
            {
                dto.siteId = configuration.GetValue<long>("WebConfig:SiteId");
            }
            GetFrontContenOutputDto result = new GetFrontContenOutputDto();
            try
            {
                var side = await db.Websites.Where(e => e.Id == dto.siteId).FirstOrDefaultAsync();
                var prod = await db.Prods.Where(e => e.Id == dto.prodId).Where(e => !e.IsDeleted).Where(e => e.FK_WebsiteId == dto.siteId).FirstOrDefaultAsync();
                if (side != null)
                {
                    result.SiteName = side.Title;
                    if (prod != null && !prod.RemovedFromShelves)
                    {
                        result.Id = (int)prod.Id;
                        result.Title = prod.Title;
                        result.Description = prod.Description;
                        result.Html = prod.Html;
                        result.Css = prod.Css;
                        result.Html = result.Html == null ? "" : result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "");
                    }
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }
    }
}
