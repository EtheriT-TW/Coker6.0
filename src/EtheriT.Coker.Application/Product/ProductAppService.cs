using AutoMapper;
using DevExpress.XtraCharts;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using EtheriT.Coker.Application.Shared.Dto.Favorites;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Processor;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Core.Product;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniExcel = MiniExcelLibs.MiniExcel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Web;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;

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
        private readonly IHtmlProcessor htmlProcessor;
        private readonly IStoreSetAppService storeSetAppService;
        private readonly StringHandler stringHandler;
        private readonly IFrontRoleContextService frontRoleContextService;
        private readonly IProductDisplayPriceService productDisplayPriceService;
        private readonly IHtmlSanitizeService htmlSanitizeService;
        public ProductAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITagAppService tagAppService,
            IConfiguration configuration,
            IMapper mapper,
            ITechnicalCertificateAppService technicalCertificateAppService,
            IFileUploadAppService fileUploadAppService,
            ISpecificationAppService specificationAppService,
            IStoreSetAppService storeSetAppService,
            IWebMenuApplication webMenuApplication,
            ITokenAppService tokenAppService,
            IHtmlProcessor htmlProcessor,
            StringHandler stringHandler,
            IFrontRoleContextService frontRoleContextService,
            IProductDisplayPriceService productDisplayPriceService,
            IHtmlSanitizeService htmlSanitizeService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tagAppService = tagAppService;
            this.configuration = configuration;
            this.technicalCertificateAppService = technicalCertificateAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.specificationAppService = specificationAppService;
            this.storeSetAppService = storeSetAppService;
            this.webMenuApplication = webMenuApplication;
            this.tokenAppService = tokenAppService;
            this.stringHandler = stringHandler;
            this.htmlProcessor = htmlProcessor;
            this.frontRoleContextService = frontRoleContextService;
            this.productDisplayPriceService = productDisplayPriceService;
            this.htmlSanitizeService = htmlSanitizeService;
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
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                output.Message = "商品名稱不可為空";
                return output;
            }
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                long userId = await loginUserData.GetUserId();
                var stock_change = false;

                if (dto.Id == 0)
                {
                    Prod p = new Prod
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
                    var db_p = db.Prods
                        .Where(e => e.Id == dto.Id && e.FK_WebsiteId == WebsiteID && !e.IsDeleted)
                        .FirstOrDefault();
                    if (db_p != null)
                    {
                        var stocks = await db.Prod_Stocks.Where(e => e.FK_Pid == db_p.Id).ToListAsync();
                        var stockids = stocks.Select(e => e.Id).ToList();
                        var scs = await db.ShoppingCarts.Where(e => stockids.Contains(e.FK_PSid) && !e.IsOrder).OrderByDescending(e => e.CreationTime).ToListAsync();
                        if (dto.status != db_p.Status && dto.status == ProdStatusEnum.售完 && !dto.NoStockManagement)
                        {
                            foreach (var sc in scs)
                            {
                                var index = stocks.FindIndex(e => e.Id == sc.FK_PSid);
                                var dtoindex = dto.Stocks.FindIndex(e => e.Id == stocks[index].Id);
                                stocks[index].Stock += sc.Quantity;
                                dto.Stocks[dtoindex].OldStock += sc.Quantity;
                                sc.Quantity = 0;
                                stock_change = true;
                            }
                        }
                        else if (dto.status != db_p.Status && db_p.Status == ProdStatusEnum.售完 && !dto.NoStockManagement)
                        {
                            foreach (var sc in scs)
                            {
                                var index = stocks.FindIndex(e => e.Id == sc.FK_PSid);
                                var dtoindex = dto.Stocks.FindIndex(e => e.Id == stocks[index].Id);
                                if (stocks[index].Stock >= sc.OldQuantity)
                                {
                                    sc.Quantity = sc.OldQuantity;
                                    stocks[index].Stock -= sc.Quantity;
                                    dto.Stocks[dtoindex].OldStock -= sc.Quantity;
                                    stock_change = true;
                                }
                                else
                                {
                                    sc.Quantity = (int?)stocks[index].Stock ?? 0;
                                    dto.Stocks[dtoindex].OldStock -= stocks[index].Stock;
                                    stocks[index].Stock = 0;
                                    stock_change = true;
                                }
                            }
                        }
                        db.SaveChanges();
                        mapper.Map(dto, db_p);
                        await loginUserData.SaveChanges(db_p);
                    }
                    else throw new Exception("商品不屬於目前網站，可能已在其他分頁切換網站");
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
                            Type = TagAssociateTypeEnum.商品,
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
                output.Error = stock_change ? "" : stock_response.Message == "庫存變動" ? stock_response.Message : "";
                output.Message = asoid.ToString();
                output.Object = stock_response.Object;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
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
                var idMappings = new List<StockIdMappingDto>();
                var websiteId = await loginUserData.GetWebsiteId();
                var noStockManagement = await db.Prods
                    .Where(e => e.Id == Pid && e.FK_WebsiteId == websiteId && !e.IsDeleted)
                    .Select(e => (bool?)e.NoStockManagement)
                    .FirstOrDefaultAsync();
                if (!noStockManagement.HasValue)
                    throw new Exception("商品不屬於目前網站，已停止儲存");
                long usetId = await loginUserData.GetUserId();
                output.Message = "";
                for (int i = 0; i < dto.Count; i++)
                {
                    var item = dto[i];
                    if (item.Id == 0)
                    {
                        Prod_Stock ps = new Prod_Stock
                        {
                            FK_Pid = Pid,
                            FK_S1id = item.FK_S1id,
                            FK_S2id = item.FK_S2id,
                            Stock = noStockManagement.Value ? (int?) null : item.Stock,
                            PackingPoint = item.PackingPoint,
                            Min_Qty = item.Min_Qty,
                            Alert_Qty = item.Alert_Qty,
                            IsTimePrice = item.TimePrice,
                            Ser_No = item.Ser_No,
                            Price = item.Price,
                            SubItemNo = item.SubItemNo,
                            SpecDescription = item.SpecDescription,
                            CreatorUserId = usetId,
                            Visible = item.Visible,
                        };
                        db.Prod_Stocks.Add(ps);
                        await db.SaveChangesAsync();

                        idMappings.Add(new StockIdMappingDto {
                            TempPSid = item.TempPSid,
                            Id = ps.Id
                        });

                        foreach (var price in item.Prices)
                        {
                            price.FK_PSId = ps.Id;
                        }
                    }
                    else
                    {
                        var db_ps = await db.Prod_Stocks.Include(e => e.Prod)
                            .Where(e => e.Id == item.Id && e.FK_Pid == Pid &&
                                        e.Prod != null && e.Prod.FK_WebsiteId == websiteId)
                            .FirstOrDefaultAsync();
                        if (db_ps != null)
                        {
                            if (db_ps.Stock == 0 && item.Stock != 0 && db_ps.Prod != null)
                            {
                                if (db_ps.Prod.oStatus == null) db_ps.Prod.Status = ProdStatusEnum.一般;
                                else db_ps.Prod.Status = db_ps.Prod.oStatus.Value;
                            }
                            db_ps.Stock = noStockManagement.Value ? (int?) null : item.Stock;
                            db_ps.IsTimePrice = item.TimePrice;
                            db_ps.FK_S1id = item.FK_S1id;
                            db_ps.FK_S2id = item.FK_S2id;
                            db_ps.Min_Qty = item.Min_Qty;
                            db_ps.Alert_Qty = item.Alert_Qty;
                            db_ps.Ser_No = item.Ser_No;
                            db_ps.SubItemNo = item.SubItemNo;
                            db_ps.SpecDescription = item.SpecDescription;
                            db_ps.PackingPoint = item.PackingPoint;
                            db_ps.Price = item.Price;
                            db_ps.LastModificationTime = DateTime.Now;
                            db_ps.LastModifierUserId = usetId;
                            db_ps.Visible = item.Visible;
                        }
                        else throw new Exception("商品規格不屬於目前網站，已停止儲存");
                    }

                    priceresponse = await PriceAddUp(item.Prices);

                }

                db.SaveChanges();

                output.Object = idMappings;

                output.Success = priceresponse.Success;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> StockBatchSet(List<StockBatchSetDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            if (dto == null || dto.Count == 0)
            {
                output.Success = true;
                return output;
            }

            try
            {
                var websiteId = await loginUserData.GetWebsiteId();

                var idToQty = dto
                    .GroupBy(x => x.Id)
                    .ToDictionary(g => g.Key, g => g.Last().StockQuantity);

                var ids = idToQty.Keys.ToList();
                var stocks = await db.Prod_Stocks.Include(s => s.Prod)
                    .Where(s => s.Prod != null && s.Prod.FK_WebsiteId == websiteId && ids.Contains(s.Id)).ToListAsync();

                foreach (var s in stocks)
                {
                    if (idToQty.TryGetValue(s.Id, out var qty))
                    {
                        s.Stock = qty;
                        if (s.Prod != null && s.Prod.Status == ProdStatusEnum.售完)
                        {
                            if (s.Prod.oStatus == null)
                                s.Prod.Status = ProdStatusEnum.一般;
                            else
                                s.Prod.Status = s.Prod.oStatus.Value;
                        }
                    }
                }

                await loginUserData.SaveChanges(stocks);
                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Success = false;
                output.Error = ex.Message;
            }
            await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(output));
            return output;
        }
        public async Task<JsonResult> SaleQuantityStaging(DataSourceLoadOptions loadOptions)
        {
            long webid = await loginUserData.GetWebsiteId();

            var rows = await (
                from p in db.Prods
                join s in db.Prod_Stocks on p.Id equals s.FK_Pid

                // S1 規格 Left Join（允許 s.FK_S1id = 0 或 NULL 時仍保留 s）
                join s1 in db.Prod_Specs on s.FK_S1id equals s1.Id into s1g
                from n1 in s1g.DefaultIfEmpty()

                    // S2 規格 Left Join
                join s2 in db.Prod_Specs on s.FK_S2id equals s2.Id into s2g
                from n2 in s2g.DefaultIfEmpty()

                where p.FK_WebsiteId == webid
                      && p.Status != ProdStatusEnum.停產
                      && !p.RemovedFromShelves
                      && p.Visible
                      && !p.NoStockManagement
                      && s.Alert_Qty != null
                      && s.Stock <= s.Alert_Qty
                select new
                {
                    StockId = s.Id,
                    ProductId = p.Id,
                    SaleQuantity = s.Alert_Qty ?? 0,
                    StockQuantity = s.Stock ?? 0,
                    Name = p.Title,
                    S1Title = n1 != null ? string.IsNullOrEmpty(n1.Title.Trim()) ? null : n1.Title.Trim() : null,
                    S2Title = n2 != null ? string.IsNullOrEmpty(n2.Title.Trim()) ? null : n2.Title.Trim() : null
                }
            ).ToListAsync();

            // 在記憶體中安全組字串（避免 EF 翻譯問題）
            var prods = rows.Select(x => new SaleQuantityStagingOutputDto
            {
                Id = x.StockId,
                ProductId = x.ProductId,
                SaleQuantity = x.SaleQuantity,
                StockQuantity = x.StockQuantity,
                Name = x.Name,
                Specs = (x.S1Title == null && x.S2Title == null)
                        ? "無規格"
                        : string.Join(" / ",
                            new[] { x.S1Title, x.S2Title }
                                .Where(t => t != null)
                                .Distinct(StringComparer.OrdinalIgnoreCase))
            }).ToList();

            var output = DataSourceLoader.Load(prods, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            });
        }
        public async Task<ResponseMessageDto> PriceAddUp(List<ProductPriceDto> dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            ResponseMessageDto deleteresponse = new ResponseMessageDto() { Success = true };
            try
            {
                long usetId = await loginUserData.GetUserId();
                long websiteId = await loginUserData.GetWebsiteId();
                if (usetId != 0)
                {
                    for (int i = 0; i < dto.Count; i++)
                    {
                        var item = dto[i];
                        var stockBelongsToWebsite = await db.Prod_Stocks
                            .AnyAsync(e => e.Id == item.FK_PSId && e.Prod != null &&
                                           e.Prod.FK_WebsiteId == websiteId && !e.Prod.IsDeleted);
                        if (!stockBelongsToWebsite)
                            throw new Exception("商品價格不屬於目前網站，已停止儲存");
                        //var allPrice = db.Prod_Prices.Where(e => !e.IsDeleted);
                        //var thePrice = await allPrice
                        //        .Where(e => e.FK_PSId == item.FK_PSId)
                        //        .Where(e => e.FK_RId == item.FK_RId)
                        //        .FirstOrDefaultAsync();
                        //if (thePrice != null && !item.IsDelete) item.Id = thePrice.Id;

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
                            var db_pp = await db.Prod_Prices
                                .Where(e => e.Id == item.Id && e.FK_PSId == item.FK_PSId)
                                .FirstOrDefaultAsync();

                            if (db_pp != null)
                            {
                                db_pp.FK_RId = item.FK_RId;
                                db_pp.Price = item.Price;
                                db_pp.Bonus = item.Bonus;
                                db_pp.LastModifierUserId = usetId;
                                db_pp.LastModificationTime = DateTime.Now;
                            }
                            else throw new Exception("商品價格不屬於目前網站，已停止儲存");
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
        public async Task<byte[]> ExportProductData()
        {
            var websiteId = await loginUserData.GetWebsiteId();
            return await ExportProductData(websiteId, null);
        }

        public async Task<byte[]> ExportProductData(
            long websiteId,
            Action<int, string>? reportProgress)
            => await ExportProductData(websiteId, "full", reportProgress);

        public async Task<byte[]> ExportProductData(
            long websiteId,
            string exportVersion,
            Action<int, string>? reportProgress)
        {
            var priceAndStockOnly = string.Equals(
                exportVersion,
                "price",
                StringComparison.OrdinalIgnoreCase);
            reportProgress?.Invoke(5, "正在讀取商品資料");
            var orgName = await loginUserData.GetWebsiteOrgName();
            var products = await db.Prods
                .AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId && !e.IsDeleted)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .ToListAsync();

            reportProgress?.Invoke(12, "正在讀取庫存與規格");
            var productIds = products.Select(e => e.Id).ToList();
            var stocks = await db.Prod_Stocks
                .AsNoTracking()
                .Where(e => productIds.Contains(e.FK_Pid) && !e.IsDeleted)
                .OrderBy(e => e.Ser_No)
                .ThenBy(e => e.Id)
                .ToListAsync();

            var specIds = stocks
                .SelectMany(e => new long?[] { e.FK_S1id, e.FK_S2id })
                .Where(e => e.HasValue && e.Value != 0)
                .Select(e => e!.Value)
                .Distinct()
                .ToList();
            var specs = await db.Prod_Specs
                .AsNoTracking()
                .Where(e => specIds.Contains(e.Id) && !e.IsDeleted)
                .ToDictionaryAsync(e => e.Id);
            var specTypeIds = specs.Values.Select(e => e.FK_Tid).Distinct().ToList();
            var specTypes = await db.Prod_Spec_Types
                .AsNoTracking()
                .Where(e => specTypeIds.Contains(e.Id) && !e.IsDeleted)
                .ToDictionaryAsync(e => e.Id, e => e.Type);

            var stockIds = stocks.Select(e => e.Id).ToList();
            var specImageByStock = new Dictionary<long, string>();
            if (!priceAndStockOnly && stockIds.Count > 0)
            {
                var specImageRows = await (
                    from bind in db.FileBinds.AsNoTracking()
                    join file in db.FileUploads.AsNoTracking()
                        on bind.FK_FileUploadId equals file.Id
                    where stockIds.Contains(bind.Sid)
                        && bind.type == (int)FileBindTypeEnum.產品規格圖
                        && !bind.IsDeleted
                        && !file.IsDeleted
                        && file.FK_WebsiteId == websiteId
                        && file.ContentType != null
                        && file.ContentType.StartsWith("image/")
                    orderby bind.SerNo, bind.Id
                    select new { StockId = bind.Sid, file.DownloadFileName }
                ).ToListAsync();
                specImageByStock = specImageRows
                    .Where(e => !string.IsNullOrWhiteSpace(e.DownloadFileName))
                    .GroupBy(e => e.StockId)
                    .ToDictionary(e => e.Key, e => e.First().DownloadFileName!);
            }
            var allPrices = await db.Prod_Prices
                .AsNoTracking()
                .Where(e => stockIds.Contains(e.FK_PSId) && !e.IsDeleted)
                .OrderBy(e => e.Id)
                .ToListAsync();
            var pricesByStock = allPrices
                .GroupBy(e => e.FK_PSId)
                .ToDictionary(e => e.Key, e => e.ToList());
            var roleNameMap = await db.Roles
                .AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId && e.Type == RoleTypeEnum.前台 && !e.IsDeleted)
                .ToDictionaryAsync(e => e.Id, e => e.Name ?? "");

            var productTagMap = new Dictionary<long, string[]>();
            if (!priceAndStockOnly)
            {
                reportProgress?.Invoke(22, "正在讀取商品標籤");
                var tagRows = await (
                    from associate in db.Tag_Associates.AsNoTracking()
                    join tag in db.Tags.AsNoTracking() on associate.FK_TId equals tag.Id
                    where associate.Type == TagAssociateTypeEnum.商品
                        && !associate.IsDeleted
                        && !tag.IsDeleted
                        && tag.FK_WebsiteId == websiteId
                        && productIds.Contains(associate.FK_AId)
                    orderby tag.Title
                    select new { ProductId = associate.FK_AId, tag.Title }
                ).ToListAsync();
                productTagMap = tagRows
                    .GroupBy(e => e.ProductId)
                    .ToDictionary(e => e.Key, e => e.Select(x => x.Title).Distinct().Take(6).ToArray());
            }

            var productRows = new List<ProductExportRow>();
            for (var productIndex = 0; productIndex < products.Count; productIndex++)
            {
                var product = products[productIndex];
                var productProgress = products.Count == 0
                    ? 78
                    : 28 + (int)Math.Floor((productIndex + 1) * 50d / products.Count);
                reportProgress?.Invoke(
                    productProgress,
                    $"正在整理商品資料（{productIndex + 1}/{products.Count}）");

                var multimedia = priceAndStockOnly
                    ? new List<FileGetProdDisplayDto>()
                    : (await fileUploadAppService.getProdMultimedia(product.Id, 1))
                        .OrderBy(e => e.SerNo)
                        .ThenBy(e => e.Id)
                        .Take(7)
                        .ToList();
                var files = priceAndStockOnly
                    ? new List<FileGetProdDisplayDto>()
                    : (await fileUploadAppService.getProdFiles(product.Id))
                        .OrderBy(e => e.SerNo)
                        .ThenBy(e => e.Id)
                        .Take(7)
                        .ToList();
                productTagMap.TryGetValue(product.Id, out var tags);

                var productStocks = stocks.Where(e => e.FK_Pid == product.Id).ToList();
                if (productStocks.Count == 0)
                    productStocks.Add(null!);

                foreach (var stock in productStocks)
                {
                    Prod_Spec? spec1 = null;
                    Prod_Spec? spec2 = null;
                    if (stock?.FK_S1id is long spec1Id) specs.TryGetValue(spec1Id, out spec1);
                    if (stock?.FK_S2id is long spec2Id) specs.TryGetValue(spec2Id, out spec2);
                    var specImage = stock != null
                        ? specImageByStock.GetValueOrDefault(stock.Id, "")
                        : "";

                    var exportPrices = new List<Prod_Price?>();
                    if (stock != null && !stock.IsTimePrice && pricesByStock.TryGetValue(stock.Id, out var stockPrices))
                        exportPrices.AddRange(stockPrices);
                    if (exportPrices.Count == 0)
                        exportPrices.Add(null);

                    foreach (var rolePrice in exportPrices)
                    {
                        productRows.Add(new ProductExportRow
                        {
                            ProductId = product.Id,
                            ItemNo = product.ItemNo ?? "",
                            SubItemNo = stock?.SubItemNo ?? "",
                            ProdName = product.Title,
                            Status = product.Status.ToString(),
                        Introduction = product.Introduction ?? "",
                        Description = product.Description ?? "",
                        SaveHtml = GetExportHtml(product.SaveHtml, product.Html, orgName),
                        SaveCss = GetExportCss(product.SaveCss, product.Css, orgName),
                        Image1 = GetExportLink(multimedia, 0, orgName),
                        Image2 = GetExportLink(multimedia, 1, orgName),
                        Image3 = GetExportLink(multimedia, 2, orgName),
                        Image4 = GetExportLink(multimedia, 3, orgName),
                        Image5 = GetExportLink(multimedia, 4, orgName),
                        Image6 = GetExportLink(multimedia, 5, orgName),
                        Image7 = GetExportLink(multimedia, 6, orgName),
                        FileName1 = GetName(files, 0),
                        File1 = GetExportLink(files, 0, orgName),
                        FileName2 = GetName(files, 1),
                        File2 = GetExportLink(files, 1, orgName),
                        FileName3 = GetName(files, 2),
                        File3 = GetExportLink(files, 2, orgName),
                        FileName4 = GetName(files, 3),
                        File4 = GetExportLink(files, 3, orgName),
                        FileName5 = GetName(files, 4),
                        File5 = GetExportLink(files, 4, orgName),
                        FileName6 = GetName(files, 5),
                        File6 = GetExportLink(files, 5, orgName),
                        FileName7 = GetName(files, 6),
                        File7 = GetExportLink(files, 6, orgName),
                        StartTime = product.permanent ? "" : product.StartTime?.ToString("yyyy-MM-dd") ?? "",
                        EndTime = product.permanent ? "" : product.EndTime?.ToString("yyyy-MM-dd") ?? "",
                        Visible = product.Visible ? "是" : "否",
                        OnShelf = product.RemovedFromShelves ? "否" : "是",
                        Spec1Name = spec1 != null && specTypes.TryGetValue(spec1.FK_Tid, out var spec1Name) ? spec1Name : "",
                        Spec1 = spec1?.Title ?? "",
                        Spec2Name = spec2 != null && specTypes.TryGetValue(spec2.FK_Tid, out var spec2Name) ? spec2Name : "",
                        Spec2 = spec2?.Title ?? "",
                        SpecImage = stringHandler.ResolveFrontUploadPath(specImage, orgName),
                        SpecDescription = stock?.SpecDescription ?? "",
                        SpecVisible = stock == null ? "" : (stock.Visible ? "是" : "否"),
                        Stock = stock?.Stock ?? 0,
                        Min_Qty = stock?.Min_Qty ?? 1,
                        Alert_Qty = stock?.Alert_Qty ?? 0,
                        SuggestPrice = stock?.Price ?? 0,
                        RoleName = stock?.IsTimePrice == true
                            ? ""
                            : rolePrice == null || rolePrice.FK_RId is 0 or 1
                                ? "非會員"
                                : roleNameMap.GetValueOrDefault(rolePrice.FK_RId, $"角色ID:{rolePrice.FK_RId}"),
                        Price = stock == null
                            ? "0"
                            : stock.IsTimePrice
                                ? "時價"
                                : (rolePrice?.Price ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Bonus = rolePrice?.Bonus ?? 0,
                        Tag1 = GetValue(tags, 0),
                        Tag2 = GetValue(tags, 1),
                        Tag3 = GetValue(tags, 2),
                        Tag4 = GetValue(tags, 3),
                        Tag5 = GetValue(tags, 4),
                        Tag6 = GetValue(tags, 5),
                        });
                    }
                }
            }

            reportProgress?.Invoke(82, priceAndStockOnly ? "正在整理價格與庫存資料" : "正在整理技術證照");
            var techRows = priceAndStockOnly
                ? new List<TechCertExportRow>()
                : await BuildTechCertExportRows(websiteId, products);
            reportProgress?.Invoke(88, priceAndStockOnly ? "正在準備簡易匯出範本" : "正在整理目錄分類");
            var directoryRows = priceAndStockOnly
                ? new List<DirectoryExportRow>()
                : await BuildDirectoryExportRows(websiteId);
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources",
                "ProductExportTemplates",
                priceAndStockOnly ? "ProductPriceData.xlsx" : "ProductData.xlsx"
            );
            if (!System.IO.File.Exists(templatePath))
                throw new FileNotFoundException("找不到商品匯出範本。", templatePath);

            reportProgress?.Invoke(94, "正在產生 Excel 檔案");
            using var stream = new MemoryStream();
            MiniExcel.SaveAsByTemplate(stream, templatePath, new
            {
                Products = productRows,
                Directories = directoryRows,
                TechCerts = techRows,
            });
            reportProgress?.Invoke(98, "Excel 檔案製作完成");
            return stream.ToArray();
        }

        private async Task<List<TechCertExportRow>> BuildTechCertExportRows(long websiteId, List<Prod> products)
        {
            var productMap = products.ToDictionary(e => e.Id);
            var productIds = productMap.Keys.ToList();
            var associations = await (
                from associate in db.Prod_TechCerts.AsNoTracking()
                join tech in db.TechnicalCertificates.AsNoTracking() on associate.FK_TCId equals tech.Id
                where productIds.Contains(associate.FK_PId)
                    && !associate.IsDeleted
                    && !tech.IsDeleted
                    && tech.FK_WebsiteId == websiteId
                orderby tech.Ser_no, tech.Id
                select new { associate.FK_PId, Tech = tech }
            ).ToListAsync();

            var imageMap = new Dictionary<long, string>();
            foreach (var techId in associations.Select(e => e.Tech.Id).Distinct())
            {
                var images = await fileUploadAppService.getImgFiles(new FileGetImgInputDto
                {
                    Sid = techId,
                    Type = (int)FileBindTypeEnum.技術證照,
                    Size = 1,
                });
                imageMap[techId] = images.OrderBy(e => e.Id).FirstOrDefault()?.Link ?? "";
            }

            return associations.Select(e => new TechCertExportRow
            {
                ItemNo = productMap[e.FK_PId].ItemNo ?? "",
                ProdName = productMap[e.FK_PId].Title,
                Title = e.Tech.Title ?? "",
                Image1 = imageMap.GetValueOrDefault(e.Tech.Id, e.Tech.Img ?? ""),
                Description = e.Tech.Description ?? "",
            }).ToList();
        }

        private async Task<List<DirectoryExportRow>> BuildDirectoryExportRows(long websiteId)
        {
            var directories = await db.Directory
                .AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId && !e.IsDeleted && e.Type == (int)DirectoryTypeEnum.商品)
                .OrderBy(e => e.Title)
                .ToListAsync();
            var menus = await db.WebMenus
                .AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId && !e.IsDeleted)
                .ToListAsync();
            var menuById = menus.ToDictionary(e => e.Id);
            var directoryIds = directories.Select(e => e.Id).ToList();
            var tagRows = await (
                from associate in db.Tag_Associates.AsNoTracking()
                join tag in db.Tags.AsNoTracking() on associate.FK_TId equals tag.Id
                where associate.Type == TagAssociateTypeEnum.目錄
                    && !associate.IsDeleted
                    && !tag.IsDeleted
                    && tag.FK_WebsiteId == websiteId
                    && directoryIds.Contains(associate.FK_AId)
                orderby tag.Title
                select new { DirectoryId = associate.FK_AId, tag.Title }
            ).ToListAsync();
            var tagMap = tagRows
                .GroupBy(e => e.DirectoryId)
                .ToDictionary(e => e.Key, e => e.Select(x => x.Title).Distinct().Take(3).ToArray());

            var output = new List<DirectoryExportRow>();
            foreach (var directory in directories)
            {
                var menu = menus.FirstOrDefault(e => e.Title == directory.Title)
                    ?? menus.FirstOrDefault(e => MenuContainsDirectory(e, directory.Id));

                // 沒有實際選單承載的孤立目錄不匯出，避免再次匯入時誤建網站選單。
                if (menu == null)
                    continue;

                var levels = new List<WebMenu>();
                var visited = new HashSet<long>();
                while (menu != null && visited.Add(menu.Id) && levels.Count < 3)
                {
                    levels.Add(menu);
                    menu = menu.FK_TopNodeId.HasValue && menuById.TryGetValue(menu.FK_TopNodeId.Value, out var parent)
                        ? parent
                        : null;
                }
                levels.Reverse();
                tagMap.TryGetValue(directory.Id, out var tags);
                output.Add(new DirectoryExportRow
                {
                    Level1 = levels[0].Title ?? "",
                    Level1RouterName = levels[0].RouterName ?? "",
                    Level2 = levels.Count > 1 ? levels[1].Title ?? "" : "",
                    Level2RouterName = levels.Count > 1 ? levels[1].RouterName ?? "" : "",
                    Level3 = levels.Count > 2 ? levels[2].Title ?? "" : "",
                    Level3RouterName = levels.Count > 2 ? levels[2].RouterName ?? "" : "",
                    Tag1 = GetValue(tags, 0),
                    Tag2 = GetValue(tags, 1),
                    Tag3 = GetValue(tags, 2),
                });
            }
            return output;
        }

        private static bool MenuContainsDirectory(WebMenu menu, long directoryId)
        {
            var rawMarker = $"data-dirid=\"{directoryId}\"";
            var encodedMarker = $"data-dirid=&quot;{directoryId}&quot;";
            return (menu.Html?.Contains(rawMarker, StringComparison.OrdinalIgnoreCase) ?? false)
                || (menu.SaveHtml?.Contains(rawMarker, StringComparison.OrdinalIgnoreCase) ?? false)
                || (menu.Html?.Contains(encodedMarker, StringComparison.OrdinalIgnoreCase) ?? false)
                || (menu.SaveHtml?.Contains(encodedMarker, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static string GetLink(IReadOnlyList<FileGetProdDisplayDto> files, int index)
            => index < files.Count ? files[index].Link.FirstOrDefault() ?? "" : "";

        private string GetExportLink(IReadOnlyList<FileGetProdDisplayDto> files, int index, string orgName)
            => stringHandler.ResolveFrontUploadPath(GetLink(files, index), orgName);

        private string GetExportHtml(string? savedContent, string? publishedContent, string orgName)
        {
            var content = stringHandler.HtmlDecode(
                string.IsNullOrWhiteSpace(savedContent) ? publishedContent ?? "" : savedContent);
            return stringHandler.ResolveFrontUploadPath(content, orgName);
        }

        private string GetExportCss(string? savedContent, string? publishedContent, string orgName)
            => stringHandler.ResolveFrontUploadPath(
                string.IsNullOrWhiteSpace(savedContent) ? publishedContent ?? "" : savedContent,
                orgName);

        private static string GetName(IReadOnlyList<FileGetProdDisplayDto> files, int index)
            => index < files.Count ? files[index].Name ?? "" : "";

        private static string GetValue(IReadOnlyList<string>? values, int index)
            => values != null && index < values.Count ? values[index] ?? "" : "";

        public async Task<ResponseMessageDto> HasAnyItemNo()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var webid = await loginUserData.GetWebsiteId();
                bool hasAnyItemNo = await db.Prods
                    .AsNoTracking()
                    .Where(p => p.FK_WebsiteId == webid && !p.IsDeleted)
                    .AnyAsync(p => !string.IsNullOrWhiteSpace(p.ItemNo));

                response.Success = hasAnyItemNo;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
        /* Get Data */
        public async Task<JsonResult> GetAllList(
            DataSourceLoadOptions loadOptions,
            string? pids,
            string? tagIds,
            bool excludeUnavailable = false)
        {
            try
            {
                long webid = await loginUserData.GetWebsiteId();
                var selectedIds = stringHandler.ParseCsvIds(pids);
                var selectedTagIds = stringHandler.ParseCsvIds(tagIds);
                // 只取必要欄位，避免撈太肥
                var productQuery = db.Prods
                    .Where(p => p.FK_WebsiteId == webid && !p.IsDeleted);

                if (excludeUnavailable)
                {
                    productQuery = productQuery.Where(p =>
                        selectedIds.Contains(p.Id) ||
                        (p.Status != ProdStatusEnum.售完 &&
                         !p.RemovedFromShelves &&
                         (p.NoStockManagement || p.Prod_Stocks.Any(s => !s.IsDeleted && (s.Stock ?? 0) > 0))));
                }

                var baseQuery = productQuery
                    .Select(p => new ProductListBase
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Visible = p.Visible,
                        RemovedFromShelves = p.RemovedFromShelves,
                        NoStockManagement = p.NoStockManagement,
                        ProductStatus = p.Status,
                        Ser_No = p.Ser_No,
                        ItemNo = p.ItemNo,
                        StartTime = p.StartTime,
                        EndTime = p.EndTime,
                        permanent = p.permanent,
                        LastModificationTime = p.LastModificationTime ?? p.CreationTime,
                        CreationTime = p.CreationTime,
                        IsSelected = selectedIds.Contains(p.Id)
                    });

                if (selectedTagIds.Count > 0)
                {
                    var matchedProductIds =
                        from ta in db.Tag_Associates.AsNoTracking()
                        where ta.Type == TagAssociateTypeEnum.商品
                           && !ta.IsDeleted
                           && selectedTagIds.Contains(ta.FK_TId)
                        group ta by ta.FK_AId into g
                        where g.Select(x => x.FK_TId).Distinct().Count() == selectedTagIds.Count
                        select g.Key;

                    baseQuery = baseQuery.Where(p => matchedProductIds.Contains(p.Id));
                }

                var baseResult = await DataSourceLoader.LoadAsync(baseQuery, loadOptions);

                var pageRows = ((IEnumerable<object>)baseResult.data).Cast<ProductListBase>().ToList();
                var pageIds = pageRows.Select(r => r.Id).ToList();
                if (pageIds.Count == 0)
                    return new JsonResult(baseResult, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });

                //抓標籤
                var tagRows = await (
                    from ta in db.Tag_Associates.AsNoTracking()
                    join t in db.Tags.AsNoTracking() on ta.FK_TId equals t.Id
                    where ta.Type == TagAssociateTypeEnum.商品
                       && !ta.IsDeleted && !t.IsDeleted
                       && pageIds.Contains(ta.FK_AId)
                    select new { ProdId = ta.FK_AId, TagName = t.Title }
                ).ToListAsync();

                var tagMap = tagRows
                    .GroupBy(x => x.ProdId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join("、", g.Select(x => x.TagName).Distinct().OrderBy(n => n))
                    );
                // 最小圖的路徑/檔名
                var imageMap = await fileUploadAppService.GetMinImageMapAsync(pageIds);

                //抓商品價格
                var priceAgg = await
                    (from s in db.Prod_Stocks
                     where !s.IsDeleted && pageIds.Contains(s.FK_Pid)
                     join pp in db.Prod_Prices.Where(pp => !pp.IsDeleted) on s.Id equals pp.FK_PSId into ppj
                     from pp in ppj.DefaultIfEmpty()
                     group new { s, pp } by s.FK_Pid into g
                     select new
                     {
                         ProdId = g.Key,
                         HasAnyStock = g.Any(), // 有無任何庫存
                         HasTimePrice = g.Any(x => x.s.IsTimePrice),
                         MinPrice = g.Where(x => !x.s.IsTimePrice && x.pp != null)
                                     .Select(x => (int?)(x.pp.Price ?? 0))
                                     .Min(),
                         MaxPrice = g.Where(x => !x.s.IsTimePrice && x.pp != null)
                                     .Select(x => (int?)(x.pp.Price ?? 0))
                                     .Max(),
                         HasNormalStock = g.Any(x => !x.s.IsTimePrice) // 有無非時價庫存
                     })
                    .ToListAsync();



                var priceMap = priceAgg.ToDictionary(x => x.ProdId);

                var stockMap = await db.Prod_Stocks
                    .AsNoTracking()
                    .Where(s => !s.IsDeleted && pageIds.Contains(s.FK_Pid))
                    .GroupBy(s => s.FK_Pid)
                    .Select(g => new
                    {
                        ProdId = g.Key,
                        StockQuantity = g.Sum(s => s.Stock ?? 0),
                        AlertQuantity = g.Sum(s => s.Alert_Qty ?? 0)
                    })
                    .ToDictionaryAsync(x => x.ProdId);

                string L_MarketPrice = L.get("MarketPrice");

                var finalRows = pageRows.Select(p =>
                {
                    string priceText = "";
                    if (priceMap.TryGetValue(p.Id, out var agg))
                    {
                        if (!agg.HasAnyStock) priceText = "";
                        else if (!agg.HasNormalStock) priceText = L_MarketPrice;
                        else if (agg.MinPrice == null || agg.MaxPrice == null)
                            priceText = agg.HasTimePrice ? L_MarketPrice : "";
                        else
                        {
                            var minPrice = agg.MinPrice.GetValueOrDefault();
                            var maxPrice = agg.MaxPrice.GetValueOrDefault();
                            priceText = agg.HasTimePrice
                                ? $"{minPrice:###,###}~{L_MarketPrice}"
                                : (minPrice == maxPrice ? $"{maxPrice:###,###}" : $"{minPrice:###,###}~{maxPrice:###,###}");
                        }
                    }

                    imageMap.TryGetValue(p.Id, out var imgPath);
                    tagMap.TryGetValue(p.Id, out var tagsText);
                    stockMap.TryGetValue(p.Id, out var stockInfo);
                    var stockQuantity = stockInfo?.StockQuantity ?? 0;
                    var alertQuantity = stockInfo?.AlertQuantity ?? 0;

                    var saleStateName = p.RemovedFromShelves
                        ? "下架"
                        : p.ProductStatus == ProdStatusEnum.售完
                            ? "售完"
                            : !p.Visible
                                ? "隱藏"
                                : !p.NoStockManagement && stockQuantity <= 0
                                    ? "庫存為 0"
                                    : "可銷售";

                    return new ProductSelectGetAllListDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Visible = p.Visible,
                        Available = !p.RemovedFromShelves,
                        RemovedFromShelves = p.RemovedFromShelves,
                        NoStockManagement = p.NoStockManagement,
                        ProductStatus = (int)p.ProductStatus,
                        ProductStatusName = p.ProductStatus.ToString(),
                        SaleStateName = saleStateName,
                        StockQuantity = p.NoStockManagement ? null : stockQuantity,
                        AlertQuantity = p.NoStockManagement ? null : alertQuantity,
                        StockDisplay = p.NoStockManagement ? "不限庫存" : stockQuantity.ToString("N0"),
                        Ser_No = p.Ser_No,
                        ItemNo = p.ItemNo ?? "",
                        Price = priceText,
                        StartTime = p.StartTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.StartTime),
                        EndTime = p.EndTime == null ? "-" : string.Format("{0:yyyy-MM-dd hh:mm}", p.EndTime),
                        Permanent = p.permanent,
                        LastModificationTime = p.LastModificationTime ?? p.CreationTime,
                        MinsizeImage = imgPath ?? "/images/noImg.jpg",
                        TagNames = tagsText ?? "",
                        IsSelected = p.IsSelected
                    };
                }).ToList();

                baseResult.data = finalRows;

                return new JsonResult(baseResult, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver()
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // TODO: 記錄 log（包含 webid、loadOptions 摘要、e.Message/e.StackTrace）
            }

            return new JsonResult(new List<ProductGetAllListDto>(), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver()
            });
        }
        public async Task<List<TagGetSelectedDto>> GetProductListTags()
        {
            long webid = await loginUserData.GetWebsiteId();

            var tags = await (
                from ta in db.Tag_Associates.AsNoTracking()
                join t in db.Tags.AsNoTracking() on ta.FK_TId equals t.Id
                join p in db.Prods.AsNoTracking() on ta.FK_AId equals p.Id
                where ta.Type == TagAssociateTypeEnum.商品
                   && !ta.IsDeleted
                   && !t.IsDeleted
                   && !p.IsDeleted
                   && p.FK_WebsiteId == webid
                group t by new { t.Id, t.Title } into g
                orderby g.Key.Title
                select new TagGetSelectedDto
                {
                    FK_TId = g.Key.Id,
                    Tag_Name = g.Key.Title
                }
            ).ToListAsync();

            return tags;
        }
        public async Task<ProdGetDataDto> GetProdDataOne(long Id)
        {
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();
                var db_p = await db.Prods
                    .Where(e => e.Id == Id && e.FK_WebsiteId == websiteId && !e.IsDeleted)
                    .OrderBy(e => e.Ser_No)
                    .FirstOrDefaultAsync();

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
                        Type = TagAssociateTypeEnum.商品,
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
                var websiteId = await loginUserData.GetCommonWebsiteId();
                var output = await (from ps in db.Prod_Stocks
                                    join p in db.Prods on ps.FK_Pid equals p.Id
                                    where !ps.IsDeleted && !p.IsDeleted && ps.FK_Pid == PId &&
                                          p.FK_WebsiteId == websiteId
                                    orderby ps.Ser_No, ps.Id
                                    select new ProductStockDto
                                    {
                                        Pid = PId,
                                        Id = ps.Id,
                                        FK_S1id = ps.FK_S1id,
                                        S1_Title = "",
                                        FK_S2id = ps.FK_S2id,
                                        TimePrice = ps.IsTimePrice,
                                        S2_Title = "",
                                        Price = ps.Price,
                                        Min_Qty = ps.Min_Qty,
                                        Stock = ps.Stock,
                                        PackingPoint = ps.PackingPoint,
                                        Alert_Qty = ps.Alert_Qty,
                                        SubItemNo = ps.SubItemNo ?? "",
                                        SpecDescription = ps.SpecDescription,
                                        Ser_No = ps.Ser_No,
                                        SuggestPrice = ps.Price,
                                        Prices = new List<ProductPriceDto>(),
                                        Visible = ps.Visible,
                                    }).ToListAsync();


                var db_sp = await db.Prod_Specs
                    .Include(e => e.Prod_Spec_Type)
                    .Where(e => !e.IsDeleted && e.Prod_Spec_Type != null &&
                                e.Prod_Spec_Type.FK_WebsiteId == websiteId)
                    .ToListAsync();

                foreach (var item in output)
                {
                    if (db_sp.Count > 0)                // ← guard 縮小到只包「規格名稱查找」
                    {
                        var spec1 = item.FK_S1id is > 0 ? db_sp.Find(spec => spec.Id == item.FK_S1id) : null;
                        var spec2 = item.FK_S2id is > 0 ? db_sp.Find(spec => spec.Id == item.FK_S2id) : null;
                        item.FK_ST1id = spec1?.FK_Tid ?? 0;
                        item.S1_Name = spec1?.Prod_Spec_Type?.Type ?? "";
                        item.S1_Title = spec1?.Title ?? "";
                        item.FK_ST2id = spec2?.FK_Tid ?? 0;
                        item.S2_Name = spec2?.Prod_Spec_Type?.Type ?? "";
                        item.S2_Title = spec2?.Title ?? "";
                    }

                    item.Prices = await this.GetPriceDataAll(item.Id);                       // ← 一定會執行
                    item.Multimedia = await fileUploadAppService.getSpecMultimedia(item.Id, 1);  // ← A-4 新增的規格圖
                }

                return output;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public async Task<JsonResult> GetRolesAll()
        {
            List<AddRoleDto> output = new List<AddRoleDto>();
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var roles = await db.Roles.Where(e => e.Id == 1 || (e.Type == RoleTypeEnum.前台 && e.FK_WebsiteId == WebsiteID)).OrderBy(e => e.Ser_No).ToListAsync();
                if (roles.Any())
                {
                    foreach (var role in roles)
                    {
                        var tmep_output = mapper.Map<AddRoleDto>(role);
                        output.Add(tmep_output);

                        if (!output.Any()) throw new Exception("查無角色資料");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"Product=>GetRolesAll回傳資料：{ex.Message}");
            }
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<ProductPriceDto>> GetPriceDataAll(long PSId)
        {
            List<ProductPriceDto> output = new List<ProductPriceDto>();
            try
            {
                var websiteId = await loginUserData.GetCommonWebsiteId();
                output = await (from pp in db.Prod_Prices
                                join ps in db.Prod_Stocks on pp.FK_PSId equals ps.Id
                                join p in db.Prods on ps.FK_Pid equals p.Id
                                join r in db.Roles on pp.FK_RId equals r.Id
                                where pp.FK_PSId == PSId && !p.IsDeleted &&
                                      p.FK_WebsiteId == websiteId
                                orderby r.Ser_No, pp.Price descending
                                select new ProductPriceDto
                                 {
                                     Id = pp.Id,
                                     FK_PSId = pp.FK_PSId,
                                     FK_RId = pp.FK_RId,
                                     Price = pp.Price,
                                     Bonus = pp.Bonus ?? 0,
                                     RoleName = r.Name,
                                 }).ToListAsync();
            }
            catch (Exception e)
            {

            }
            return output;
        }
        public async Task<List<ProductPriceDto>> GetPriceByStock(List<long> PSIds)
        {
            var output = new List<ProductPriceDto>();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                Guid UUID = await tokenAppService.GetUUID();

                var role_level = await db.Roles.Where(e => e.Type == RoleTypeEnum.前台 && e.FK_WebsiteId == WebsiteId).OrderBy(e => e.Ser_No).Select(e => e.Id).ToListAsync();
                role_level.Insert(0, 1);
                var roleid = await frontRoleContextService.GetRoleIdAsync(UUID, WebsiteId);
                var role_index = role_level.IndexOf(roleid);

                foreach (var stockid in PSIds)
                {
                    var cash = await db.Prod_Prices.Where(e => e.FK_PSId == stockid).Where(e => e.Bonus == 0).ToListAsync();
                    if (cash.Any())
                    {
                        var tempori = mapper.Map<ProductPriceDto>(cash.Find(e => e.FK_RId == 1));
                        if (tempori != null) tempori.OriPrice = tempori.Price;

                        if (role_index >= 0)
                        {
                            for (var index = role_index; index >= 0; index--)
                            {
                                if (cash.Where(e => e.FK_RId == role_level[index]).Any())
                                {
                                    var temp = mapper.Map<ProductPriceDto>(cash.Where(e => e.FK_RId == role_level[index]).FirstOrDefault());
                                    temp.OriPrice = tempori?.OriPrice ?? 0;
                                    output.Add(temp);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (tempori != null) output.Add(mapper.Map<ProductPriceDto>(tempori));
                        }
                    }
                }

                var temp_bonus = await db.Prod_Prices.Where(e => PSIds.Contains(e.FK_PSId)).Where(e => e.Bonus > 0).Where(e => role_level.Take(role_index + 1).Contains(e.FK_RId)).OrderBy(e => e.Price).ThenBy(e => e.Bonus).ToListAsync();
                var bonus = new List<Prod_Price>();
                foreach (var temp in temp_bonus)
                {
                    if (bonus.Find(e => e.Bonus == temp.Bonus || e.Price == temp.Price) == null)
                    {
                        var output_this = output.FindAll(e => e.FK_PSId == temp.FK_PSId && e.Bonus == 0);
                        if (!output_this.Any() || output_this.Find(e => e.Price > temp.Price) != null) bonus.Add(temp);
                    }
                }
                output.AddRange(mapper.Map<List<ProductPriceDto>>(bonus.OrderByDescending(e => e.Price)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"Product=>GetPriceByStock回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<ProdGetMainDisplayDto> GetMainDisplayOne(long Id)
        {
            ProdGetMainDisplayDto output = new ProdGetMainDisplayDto();

            try
            {
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var purchaseCheckTime = DateTime.Now;
                var db_p = db.Prods
                    .Where(e => e.Id == Id && e.FK_WebsiteId == websiteId && !e.IsDeleted)
                    .OrderBy(e => e.Ser_No)
                    .FirstOrDefault();

                if (db_p != null)
                {
                    var sanitized = await EnsureProductDisplayContentSanitizedAsync(db_p);
                    output = new ProdGetMainDisplayDto()
                    {
                        Id = db_p.Id,
                        Title = db_p.Title,
                        Introduction = db_p.Introduction,
                        Description = db_p.Description,
                        // 前端只能接收此處已清洗、已 Decode 的 HTML，不再自行 htmlDecode。
                        Html = sanitized.Html,
                        Css = sanitized.Css,
                        ItemNo = db_p.ItemNo,
                        Status = (int)db_p.Status,
                        NoStockManagement = db_p.NoStockManagement,
                        StatusName = db_p.Status.ToString(),
                        CanPurchase = ProductPurchasePolicy.CanPurchaseProduct(db_p, purchaseCheckTime),
                        PurchaseUnavailableReason = ProductPurchasePolicy.GetProductUnavailableReason(db_p, purchaseCheckTime),
                        TagDatas = new List<TagGetSelectedDto>(),
                        TechCertDatas = new List<TechCertDisplayDto>(),
                        Stocks = new List<ProductStockDto>(),
                        Files = new List<FileGetImgDto>(),
                        Img_Original = new List<FileGetProdDisplayDto>(),
                        Img_Medium = new List<FileGetProdDisplayDto>(),
                        Img_Small = new List<FileGetProdDisplayDto>(),

                        // 商品主顯示價格（你已補到 DTO 的欄位）
                        Price = null,
                        Bonus = null,
                        OriPrice = null,
                        SuggestPrice = null,
                        IsTimePrice = false,
                        PriceDisplayText = null,
                        BaseRoleName = null,
                        CurrentRoleName = null
                    };

                    var tagDatas = await tagAppService.GetTagAssociate(new TagAssociateGetDto()
                    {
                        Fk_Aid = output.Id,
                        Type = TagAssociateTypeEnum.商品,
                    });

                    if (tagDatas != null)
                    {
                        output.TagDatas = tagDatas;
                    }

                    var techcertDatas = await technicalCertificateAppService.GetDisplayData(db_p.Id);

                    if (techcertDatas != null)
                    {
                        output.TechCertDatas = techcertDatas;
                    }

                    // ===== 商品主顯示價格：改走共用顯示價格 service =====
                    string orgName = await loginUserData.GetWebsiteOrgName();

                    var priceOrder = await storeSetAppService.getValues(new Shared.Dto.StoreSet.StoreSetGetValueInput
                    {
                        key = "priceOrder",
                        SiteId = websiteId
                    });

                    var orderLowToHigh =
                        priceOrder.Success &&
                        priceOrder.detailItem != null &&
                        priceOrder.detailItem.key == "priceOrder" &&
                        priceOrder.detailItem.value != null &&
                        priceOrder.detailItem.value.Contains("LtoH");

                    var roleContext = await frontRoleContextService.GetCurrentContextAsync(orgName);

                    var displayPrice = await productDisplayPriceService.GetProductDisplayPriceAsync(
                        output.Id,
                        roleContext,
                        orderLowToHigh);

                    if (displayPrice != null)
                    {
                        output.Price = displayPrice.Price;
                        output.Bonus = displayPrice.Bonus;
                        output.OriPrice = displayPrice.OriPrice;
                        output.SuggestPrice = displayPrice.SuggestPrice;
                        output.IsTimePrice = displayPrice.IsTimePrice;
                        output.PriceDisplayText = displayPrice.PriceDisplayText;
                        output.BaseRoleName = displayPrice.BaseRoleName;
                        output.CurrentRoleName = displayPrice.CurrentRoleName;
                    }
                    // ===== 主顯示價格結束 =====

                    var stockDatas = await this.GetStockDataAll(output.Id);
                    if (stockDatas != null)
                    {
                        stockDatas = stockDatas.Where(e => e.Visible).ToList();
                        var prices = await productDisplayPriceService.GetDisplayPricesByStockAsync(stockDatas.Select(e => e.Id).ToList(), roleContext);
                        var stockIds = stockDatas.Select(x => x.Id).ToList();
                        var stockEntities = await db.Prod_Stocks
                            .Where(x => stockIds.Contains(x.Id))
                            .ToDictionaryAsync(x => x.Id);

                        foreach (var stock in stockDatas)
                        {
                            stock.Prices = prices.Where(e => e.FK_PSId == stock.Id).ToList();
                            stock.SpecDescription = storeSetAppService.RenderMarkdownToHtml(stock.SpecDescription);
                            if (stockEntities.TryGetValue(stock.Id, out var stockEntity))
                            {
                                var hasPrice = stock.Prices.Any();
                                stock.MaxPurchaseQuantity = ProductPurchasePolicy.GetMaxPurchaseQuantity(db_p, stockEntity);
                                stock.CanPurchase = ProductPurchasePolicy.CanPurchaseStock(db_p, stockEntity, hasPrice, purchaseCheckTime);
                                stock.PurchaseUnavailableReason = ProductPurchasePolicy.GetStockUnavailableReason(db_p, stockEntity, hasPrice, purchaseCheckTime);
                            }
                        }

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
                }
                else throw new Exception("查無商品資料");
            }
            catch (Exception e)
            {
            }

            return output;
        }
        public async Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(DirectoryReleInfoInputDto dto)
        {
            try
            {
                var uuid = await tokenAppService.GetUUID();
                var token = await db.Tokens
                    .Where(e => e.UUID == uuid)
                    .FirstOrDefaultAsync();

                bool isFront = loginUserData.IsisFront();

                long websiteId = dto.SiteId == 0
                    ? await loginUserData.GetWebsiteId()
                    : (long)dto.SiteId;

                var priceOrder = await storeSetAppService.getValues(new Shared.Dto.StoreSet.StoreSetGetValueInput
                {
                    key = "priceOrder",
                    SiteId = websiteId
                });
                var storeBuyState = await storeSetAppService.getValues(new Shared.Dto.StoreSet.StoreSetGetValueInput
                {
                    key = "storeBuyState",
                    SiteId = websiteId
                });
                var showProductPrice = false;

                if (storeBuyState.Success && storeBuyState.detailItem != null && storeBuyState.detailItem.value != null){
                    showProductPrice = !storeBuyState.detailItem.value.Contains("noPayNoShow");
                }

                var orderLowToHigh =
                    priceOrder.Success &&
                    priceOrder.detailItem != null &&
                    priceOrder.detailItem.key == "priceOrder" &&
                    priceOrder.detailItem.value != null &&
                    priceOrder.detailItem.value.Contains("LtoH");

                string orgName = await loginUserData.GetWebsiteOrgName(websiteId);

                var output = new List<DirectoryReleInfoDto>();

                var result = await db.Prods
                    .Where(e => dto.Ids.Contains(e.Id) && !e.IsDeleted && e.FK_WebsiteId == websiteId)
                    .OrderBy(e => e.Ser_No)
                    .ThenByDescending(e => e.Status == ProdStatusEnum.新品)
                    .ThenByDescending(e => e.Status != ProdStatusEnum.售完)
                    .ThenByDescending(e => e.Status != ProdStatusEnum.停產)
                    .ThenBy(e => e.ItemNo)
                    .ThenBy(e => e.Title)
                    .ThenByDescending(e => e.Id)
                    .ToListAsync();

                if (result == null || !result.Any())
                    throw new Exception("查無商品資料");

                var productData = mapper.Map<List<ProdGetDataDto>>(result);

                output = (from p in productData
                          select new DirectoryReleInfoDto
                          {
                              Id = p.Id,
                              FId = null,
                              Title = p.Title,
                              ItemNo = p.ItemNo,
                              OrgName = orgName,
                              Link = $"/product/{p.Id}",
                              type = DirectoryTypeEnum.商品,
                              Introduction = p.Introduction,
                              Description = p.Description,
                              SerNo = p.Ser_No,
                              Status = p.Status,
                              StatusName = Enum.GetName(typeof(ProdStatusEnum), (ProdStatusEnum)p.Status) ?? string.Empty,
                              tags = new List<TagGetSelectedDto>(),
                              MainImage = "/images/noImg.jpg",
                          }).ToList();

                var outputIds = output.Select(x => x.Id).ToList();
                var tagRows = await (
                    from associate in db.Tag_Associates.AsNoTracking()
                    join tag in db.Tags.AsNoTracking() on associate.FK_TId equals tag.Id
                    where !associate.IsDeleted
                        && associate.Type == TagAssociateTypeEnum.商品
                        && outputIds.Contains(associate.FK_AId)
                        && !tag.IsDeleted
                        && tag.FK_WebsiteId == websiteId
                    select new { associate.FK_AId, TagId = tag.Id, tag.Title }
                ).ToListAsync();
                var tagMap = tagRows
                    .GroupBy(x => x.FK_AId)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .GroupBy(x => new { x.TagId, x.Title })
                            .Select(x => new TagGetSelectedDto
                            {
                                FK_TId = x.Key.TagId,
                                Tag_Name = x.Key.Title
                            }).ToList());

                var imageRows = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto
                {
                    Sid = outputIds,
                    Type = (int)FileBindTypeEnum.產品,
                    Size = 1
                });
                var imageMap = imageRows
                    .Where(x => !string.IsNullOrWhiteSpace(x.Link))
                    .GroupBy(x => x.Sid)
                    // getImgsFiles 已依商品、圖片 SerNo、Upload Id 排序；沿用第一張，
                    // 避免改以 Upload Id 取圖而與商品內頁的後台排序不一致。
                    .ToDictionary(x => x.Key, x => x.First().Link);

                var favoriteMap = await db.Favorites.AsNoTracking()
                    .Where(x => x.UUID == uuid
                        && outputIds.Contains(x.FK_AssocId)
                        && x.Type == (int)FavoritesTypeEnum.商品)
                    .GroupBy(x => x.FK_AssocId)
                    .Select(x => new { ProductId = x.Key, FavoriteId = x.Min(y => y.Id) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.FavoriteId);

                foreach (var item in output)
                {
                    if (tagMap.TryGetValue(item.Id, out var tags)) item.tags = tags;
                    if (imageMap.TryGetValue(item.Id, out var image)) item.MainImage = image;
                    if (favoriteMap.TryGetValue(item.Id, out var favoriteId)) item.FId = favoriteId;
                }

                // 一次取得所有商品的目錄價格
                Dictionary<long, DirectoryPriceResultDto> priceMap = new();

                if (showProductPrice)
                {
                    var roleContext = await frontRoleContextService.GetCurrentContextAsync(orgName);

                    priceMap = await productDisplayPriceService.GetDirectoryPriceMapAsync(
                        output.Select(e => e.Id).ToList(),
                        roleContext,
                        orderLowToHigh);
                }

                for (int i = 0; i < output.Count; i++)
                {
                    var data = output[i];

                    if (!showProductPrice)
                    {
                        data.MarketingLabels = new List<ProductMarketingLabelDto>();
                        continue;
                    }

                    if (priceMap.TryGetValue(data.Id, out var priceInfo))
                    {
                        mapper.Map(priceInfo, data);
                    }
                    else
                    {
                        data.IsTimePrice = false;
                        data.Price = null;
                        data.Bonus = null;
                        data.OriPrice = null;
                        data.SuggestPrice = null;
                        data.PriceDisplayText = null;
                        data.BaseRoleName = null;
                        data.CurrentRoleName = null;
                        data.HasBonusPrice = false;
                    }

                    // 時價時，沿用你原本的顯示邏輯
                    if (data.IsTimePrice)
                    {
                        data.PriceDisplayText = L.get("MarketPrice");
                        data.SuggestPrice = null;
                        data.OriPrice = null;
                        data.BaseRoleName = null;
                        data.CurrentRoleName = null;
                    }

                    // 商品行銷標籤：一定要放在價格 mapping 後面
                    data.MarketingLabels = BuildMarketingLabels(data);

                    // 如果你仍想保留「訪客且非會員價時才顯示 SuggestPrice」的舊規則，
                    // 就把這段改回條件判斷；目前這版以 GetDirectoryPriceMapAsync 的結果為準。
                }

                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static bool HasDisplayNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var text = value.Replace(",", "").Trim();

            return decimal.TryParse(text, out var number) && number > 0;
        }

        private static ProductMarketingLabelDto CreateMarketingLabel(ProductMarketingLabelTypeEnum type)
        {
            return type switch
            {
                ProductMarketingLabelTypeEnum.紅利 => new ProductMarketingLabelDto
                {
                    Type = ProductMarketingLabelTypeEnum.紅利,
                    Text = "紅利",
                    CssClass = "marketing-label-bonus"
                },

                ProductMarketingLabelTypeEnum.加價購 => new ProductMarketingLabelDto
                {
                    Type = ProductMarketingLabelTypeEnum.加價購,
                    Text = "加價購",
                    CssClass = "marketing-label-addon"
                },

                ProductMarketingLabelTypeEnum.滿額贈 => new ProductMarketingLabelDto
                {
                    Type = ProductMarketingLabelTypeEnum.滿額贈,
                    Text = "滿額贈",
                    CssClass = "marketing-label-gift"
                },

                ProductMarketingLabelTypeEnum.限時優惠 => new ProductMarketingLabelDto
                {
                    Type = ProductMarketingLabelTypeEnum.限時優惠,
                    Text = "限時優惠",
                    CssClass = "marketing-label-limited"
                },

                _ => new ProductMarketingLabelDto
                {
                    Type = ProductMarketingLabelTypeEnum.自訂,
                    Text = "優惠",
                    CssClass = "marketing-label-custom"
                }
            };
        }

        private static List<ProductMarketingLabelDto> BuildMarketingLabels(DirectoryReleInfoDto data)
        {
            var labels = new List<ProductMarketingLabelDto>();

            if (data == null)
                return labels;

            /*
             * 目前第一階段只有紅利：
             * 只要最後實際目錄價格有 Bonus，就顯示「紅利」行銷標籤。
             * 是否啟用紅利、是否可見，已由 ProductDisplayPriceService 決定。
             */
            if (data.HasBonusPrice)
            {
                labels.Add(CreateMarketingLabel(ProductMarketingLabelTypeEnum.紅利));
            }

            /*
             * 未來加價購、滿額贈、限時優惠可以在這裡追加：
             *
             * if (data.HasAddOnPurchase)
             *     labels.Add(CreateMarketingLabel(ProductMarketingLabelTypeEnum.加價購));
             *
             * if (data.HasGiftCampaign)
             *     labels.Add(CreateMarketingLabel(ProductMarketingLabelTypeEnum.滿額贈));
             */

            return labels;
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
                long websiteId = await loginUserData.GetWebsiteId();
                var db_p = db.Prods
                    .Where(e => e.Id == Id && e.FK_WebsiteId == websiteId && !e.IsDeleted)
                    .FirstOrDefault();

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

                    var tagids = await db.Tag_Associates.Where(e => e.FK_AId == Id && e.Type == TagAssociateTypeEnum.商品 && !e.IsDeleted).ToListAsync();

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
                else
                {
                    output.Success = false;
                    output.Error = "商品不屬於目前網站，已停止刪除";
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
                long websiteId = await loginUserData.GetWebsiteId();
                var db_ps = db.Prod_Stocks
                    .Include(e => e.Prod)
                    .Where(e => e.Id == Id && e.Prod != null &&
                                e.Prod.FK_WebsiteId == websiteId && !e.Prod.IsDeleted)
                    .FirstOrDefault();
                if (db_ps != null)
                {
                    db_ps.IsDeleted = true;
                    db_ps.DeletionTime = DateTime.Now;
                    db_ps.DeleterUserId = usetId;
                    db.SaveChanges();
                    output.Success = true;

                    var db_pp = db.Prod_Prices.Where(e => e.FK_PSId == Id);
                    foreach (var item in db_pp)
                    {
                        item.IsDeleted = true;
                        item.DeleterUserId = usetId;
                        item.DeletionTime = DateTime.Now;
                    }

                    await fileUploadAppService.deleteFileById(new FileDeleteDto()
                    {
                        Sid = Id,
                        Type = (int)FileBindTypeEnum.產品規格圖,
                    });
                }
                else output.Error = "商品規格不屬於目前網站，已停止刪除";
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
                long websiteId = await loginUserData.GetWebsiteId();
                var db_pp = (from price in db.Prod_Prices
                             join stock in db.Prod_Stocks on price.FK_PSId equals stock.Id
                             join prod in db.Prods on stock.FK_Pid equals prod.Id
                             where price.Id == Id && prod.FK_WebsiteId == websiteId && !prod.IsDeleted
                             select price).FirstOrDefault();
                if (db_pp != null)
                {
                    db_pp.IsDeleted = true;
                    db_pp.DeletionTime = DateTime.Now;
                    db_pp.DeleterUserId = usetId;
                    db.SaveChanges();
                    output.Success = true;
                }
                else output.Error = "商品價格不屬於目前網站，已停止刪除";
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
                var token = await tokenAppService.CheckToken(null);
                Guid UUID = await tokenAppService.GetUUID();

                var prod = db.Prods.Where(e => e.Id == FK_Pid).FirstOrDefault();
                if (prod != null)
                {
                    prod.Clicks = prod.Clicks == null ? 1 : prod.Clicks + 1;
                    await loginUserData.SaveChanges(prod);

                    var userid = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e => e.FK_User).FirstOrDefaultAsync();

                    Prod_Log prod_log = new Prod_Log
                    {
                        FK_Pid = FK_Pid,
                        Action = LogActionEnum.點擊,
                        UUID = UUID,
                        FK_UserId = userid
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
                var db_ps = db.Prod_Stocks.Where(e => e.FK_Pid == db_p.Id && !e.IsDeleted)
                    .OrderBy(e => e.Price)
                    .FirstOrDefault();

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
                                    where ps.FK_Pid == id && !ps.IsDeleted && ps.Visible
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

                var s1Title = db_spt.ElementAtOrDefault(0)?.Type ?? "";
                var s2Title = db_spt.ElementAtOrDefault(1)?.Type ?? "";

                foreach (var item in output)
                {
                    item.S1_Title = s1Title;
                    item.S1_Name = item.FK_S1id is > 0 ? db_sp.FirstOrDefault(s => s.Id == item.FK_S1id)?.Title ?? "" : "";
                    item.S2_Title = s2Title;
                    item.S2_Name = item.FK_S2id is > 0 ? db_sp.FirstOrDefault(s => s.Id == item.FK_S2id)?.Title ?? "" : "";
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
                if (page <= 0) page = 1;

                Guid uuid = await tokenAppService.GetUUID();
                long websiteId = configuration.GetValue<long>("WebConfig:SiteId");

                var prodLogs = await db.Prod_Logs
                    .Where(e => e.UUID == uuid)
                    .Where(e => e.Action == LogActionEnum.點擊)
                    .Where(e => DateTime.Now.AddMonths(-3) < e.CreationTime)
                    .OrderByDescending(e => e.CreationTime)
                    .Select(e => e.FK_Pid)
                    .ToListAsync();

                var pids = new List<long>();
                foreach (var pid in prodLogs)
                {
                    if (!pids.Contains(pid))
                    {
                        pids.Add(pid);
                    }
                }

                if (!pids.Any())
                {
                    output.Success = true;
                    output.Data = new List<DirectoryReleInfoDto>();
                    output.Page_Total = 0;
                    return output;
                }

                const int pageSize = 8;
                output.Page_Total = (int)Math.Ceiling(pids.Count / (double)pageSize);

                var pageIds = pids
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var directoryData = await GetDirectoryReleInfo(new DirectoryReleInfoInputDto
                {
                    Ids = pageIds,
                    SiteId = websiteId
                });

                if (directoryData == null)
                {
                    output.Success = false;
                    output.Error = "查無商品資料";
                    output.Data = new List<DirectoryReleInfoDto>();
                    return output;
                }

                output.Data = pageIds
                    .Join(directoryData,
                        pid => pid,
                        item => item.Id,
                        (pid, item) => item)
                    .ToList();

                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                output.Success = false;
                output.Error = ex.Message;
                output.Data = new List<DirectoryReleInfoDto>();
                return output;
            }
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
                var s = await SaveContenInternal(dto, writeAuditLog: false);
                if (!s.Success)
                    throw new Exception(s.Error ?? "商品內容儲存失敗");
                var user = await loginUserData.GetUser();
                var websiteId = await loginUserData.GetWebsiteId();
                var prod = await db.Prods.FirstOrDefaultAsync(e => e.Id == dto.Id &&
                                                                   e.FK_WebsiteId == websiteId &&
                                                                   !e.IsDeleted);
                if (prod != null)
                {
                    string Orgname = await loginUserData.GetWebsiteOrgName();
                    importDto.Html = stringHandler.HtmlDecode(importDto.Html);
                    importDto.Html = htmlProcessor.RemoveNode(importDto.Html ?? "", ".backstageType");
                    importDto.Html = htmlProcessor.SetAttr(importDto.Html ?? "", "[target='_blank'] ", "rel", "noopener noreferrer");
                    importDto.Html = (importDto.Html ?? "").Replace($"/upload/{Orgname}/", "/upload/");
                    importDto.Css = (importDto.Css ?? "").Replace($"/upload/{Orgname}/", "/upload/");

                    var sanitized = await SanitizeProductPublishedContentAsync(
                        prod.FK_WebsiteId,
                        prod.Id,
                        importDto.Html ?? "",
                        importDto.Css ?? "",
                        true
                    );

                    prod.PageText = htmlProcessor.text(sanitized.Html);
                    prod.Html = stringHandler.HtmlEncode(sanitized.Html);
                    prod.Css = sanitized.Css;
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
            finally
            {
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
            }
            return response;
        }
        public Task<ResponseMessageDto> SaveConten(ProdSaveContenDto dto)
        {
            return SaveContenInternal(dto, writeAuditLog: true);
        }
        private async Task<ResponseMessageDto> SaveContenInternal(ProdSaveContenDto dto, bool writeAuditLog)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                dto.SaveHtml = HttpUtility.HtmlEncode(dto.SaveHtml);
                var user = await loginUserData.GetUser();
                var websiteId = await loginUserData.GetWebsiteId();
                var prod = await db.Prods.FirstOrDefaultAsync(e => e.Id == dto.Id &&
                                                                   e.FK_WebsiteId == websiteId &&
                                                                   !e.IsDeleted);

                if (prod == null)
                    throw new Exception("商品不屬於目前網站，已停止儲存");

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
            finally
            {
                if (writeAuditLog)
                {
                    await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
                }
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
                        var sanitized = await EnsureProductDisplayContentSanitizedAsync(prod);
                        result.Id = (int)prod.Id;
                        result.Title = prod.Title;
                        result.Description = BuildProductSeoDescription(prod);
                        var images = await fileUploadAppService.getImgFiles(new FileGetImgInputDto { Sid = prod.Id, Type = (int)FileBindTypeEnum.產品, Size = 1 });
                        if (images.Count > 0)
                        {
                            result.ImageUrl = images[0].Link;
                        }
                        result.Html = stringHandler.HtmlEncode(sanitized.Html);
                        result.Css = sanitized.Css;
                        result.Html = result.Html == null ? "" : result.Html.Replace("&lt;body&gt;", "").Replace("&lt;/body&gt;", "");
                        result.PopularVisible = prod.PopularVisible;
                        result.Popular = prod.PopularVisible ? prod.Popular : null;
                    }
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public async Task<ProductSeoDataDto?> GetSeoData(
            ProdGetFrontContenInputDto dto,
            bool orderLowToHigh)
        {
            var websiteId = dto.siteId != 0
                ? dto.siteId
                : configuration.GetValue<long>("WebConfig:SiteId");
            var product = await db.Prods
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == dto.prodId &&
                    e.FK_WebsiteId == websiteId &&
                    !e.IsDeleted &&
                    !e.RemovedFromShelves);

            if (product == null)
            {
                return null;
            }

            var now = DateTime.Now;
            var stocks = await db.Prod_Stocks
                .AsNoTracking()
                .Where(e => e.FK_Pid == product.Id && !e.IsDeleted && !e.IsTimePrice && e.Visible)
                .ToListAsync();

            var stockIds = stocks.Select(e => e.Id).ToList();
            var publicPrices = stockIds.Count == 0
                ? new List<Prod_Price>()
                : await db.Prod_Prices
                    .AsNoTracking()
                    .Where(e =>
                        stockIds.Contains(e.FK_PSId) &&
                        !e.IsDeleted &&
                        (e.FK_RId == 1 || e.FK_RId == 0) &&
                        (e.Bonus ?? 0) == 0 &&
                        (e.Price ?? 0) > 0)
                    .ToListAsync();

            var candidates = stocks
                .Select(stock => new
                {
                    Stock = stock,
                    Price = publicPrices
                        .Where(e => e.FK_PSId == stock.Id)
                        .OrderBy(e => e.FK_RId == 1 ? 0 : 1)
                        .ThenBy(e => e.Price)
                        .ThenBy(e => e.Id)
                        .FirstOrDefault()
                })
                .Where(e => e.Price?.Price != null)
                .Select(e => new
                {
                    e.Stock,
                    Price = e.Price!.Price!.Value,
                    IsAvailable = ProductPurchasePolicy.CanPurchaseStock(
                        product,
                        e.Stock,
                        hasPrice: true,
                        now)
                })
                .ToList();

            var specIds = stocks
                .SelectMany(e => new long?[] { e.FK_S1id, e.FK_S2id })
                .Where(e => e.HasValue && e.Value > 0)
                .Select(e => e!.Value)
                .Distinct()
                .ToList();
            var specOptionMap = new Dictionary<long, ProductSeoVariantOptionDto>();
            if (specIds.Count > 0)
            {
                var specOptionRows = await (
                    from spec in db.Prod_Specs.AsNoTracking()
                    join specType in db.Prod_Spec_Types.AsNoTracking()
                        on spec.FK_Tid equals specType.Id
                    where specIds.Contains(spec.Id)
                        && !spec.IsDeleted
                        && !specType.IsDeleted
                        && specType.FK_WebsiteId == websiteId
                    select new
                    {
                        spec.Id,
                        TypeName = specType.Type,
                        Value = spec.Title,
                        specType.SeoVariantProperty
                    })
                    .ToListAsync();
                specOptionMap = specOptionRows.ToDictionary(
                    e => e.Id,
                    e => new ProductSeoVariantOptionDto
                    {
                        TypeName = e.TypeName,
                        Value = e.Value,
                        SeoVariantProperty = e.SeoVariantProperty
                    });
            }

            var specImageMap = new Dictionary<long, string>();
            if (stockIds.Count > 0)
            {
                var imageRows = await (
                    from bind in db.FileBinds.AsNoTracking()
                    join file in db.FileUploads.AsNoTracking()
                        on bind.FK_FileUploadId equals file.Id
                    where stockIds.Contains(bind.Sid)
                        && bind.type == (int)FileBindTypeEnum.產品規格圖
                        && bind.IsVisible
                        && !bind.IsDeleted
                        && !file.IsDeleted
                        && file.FK_WebsiteId == websiteId
                        && file.ContentType != null
                        && file.ContentType.StartsWith("image/")
                    orderby bind.SerNo, bind.Id
                    select new
                    {
                        StockId = bind.Sid,
                        bind.MediaLink,
                        file.DownloadFileName
                    })
                    .ToListAsync();
                var orgName = await loginUserData.GetWebsiteOrgName();
                specImageMap = imageRows
                    .Select(e => new
                    {
                        e.StockId,
                        Path = stringHandler.ResolveFrontUploadPath(
                            string.IsNullOrWhiteSpace(e.MediaLink)
                                ? e.DownloadFileName ?? string.Empty
                                : e.MediaLink,
                            orgName)
                    })
                    .Where(e => !string.IsNullOrWhiteSpace(e.Path))
                    .GroupBy(e => e.StockId)
                    .ToDictionary(e => e.Key, e => e.First().Path);
            }

            var variants = new List<ProductSeoVariantDto>();
            foreach (var candidate in candidates
                .OrderBy(e => e.Stock.Ser_No)
                .ThenBy(e => e.Stock.Id))
            {
                var options = new List<ProductSeoVariantOptionDto>();
                if (candidate.Stock.FK_S1id is long spec1Id &&
                    specOptionMap.TryGetValue(spec1Id, out var spec1Option))
                {
                    options.Add(spec1Option);
                }
                if (candidate.Stock.FK_S2id is long spec2Id &&
                    specOptionMap.TryGetValue(spec2Id, out var spec2Option))
                {
                    options.Add(spec2Option);
                }

                variants.Add(new ProductSeoVariantDto
                {
                    StockId = candidate.Stock.Id,
                    SubItemNo = candidate.Stock.SubItemNo,
                    PublicPrice = candidate.Price,
                    IsAvailable = candidate.IsAvailable,
                    ImageUrl = specImageMap.GetValueOrDefault(candidate.Stock.Id),
                    Options = options
                });
            }

            // 有可購買規格時，SEO 價格只從可購買規格中選；全部售完時仍保留公開價格並標為 OutOfStock。
            var availableCandidates = candidates.Where(e => e.IsAvailable).ToList();
            var priceCandidates = availableCandidates.Count > 0
                ? availableCandidates
                : candidates;
            var selected = orderLowToHigh
                ? priceCandidates
                    .OrderBy(e => e.Price)
                    .ThenBy(e => e.Stock.Ser_No)
                    .ThenBy(e => e.Stock.Id)
                    .FirstOrDefault()
                : priceCandidates
                    .OrderByDescending(e => e.Price)
                    .ThenBy(e => e.Stock.Ser_No)
                    .ThenBy(e => e.Stock.Id)
                    .FirstOrDefault();

            return new ProductSeoDataDto
            {
                Id = product.Id,
                Title = product.Title ?? string.Empty,
                ItemNo = product.ItemNo,
                PublicPrice = selected?.Price,
                IsAvailable = selected?.IsAvailable == true,
                Variants = variants
            };
        }

        private string BuildProductSeoDescription(Prod prod)
        {
            var title = NormalizeSeoText(htmlProcessor.text(stringHandler.HtmlDecode(prod.Title ?? "")));
            var description = NormalizeSeoText(htmlProcessor.text(stringHandler.HtmlDecode(prod.Description ?? "")));

            if (!IsWeakSeoDescription(description, title))
            {
                return description;
            }

            var introduction = NormalizeSeoText(htmlProcessor.text(stringHandler.HtmlDecode(prod.Introduction ?? "")));
            var parts = new List<string>();
            AddDistinctSeoPart(parts, title);
            AddDistinctSeoPart(parts, description);
            AddDistinctSeoPart(parts, introduction);

            var separator = parts.Any(ContainsCjkText) ? "。" : ". ";
            var generated = string.Join(separator, parts);

            if (IsWeakSeoDescription(generated, title))
            {
                var pageText = NormalizeSeoText(htmlProcessor.text(stringHandler.HtmlDecode(prod.Html ?? "")));
                AddDistinctSeoPart(parts, pageText);
                generated = string.Join(separator, parts);
            }

            return TruncateSeoDescription(generated, 200);
        }

        private static bool IsWeakSeoDescription(string description, string title)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return true;
            }

            if (NormalizeSeoComparisonText(description) == NormalizeSeoComparisonText(title))
            {
                return true;
            }

            if (ContainsCjkText(description))
            {
                return description.Count(char.IsLetterOrDigit) < 20;
            }

            return Regex.Matches(
                description,
                @"[\p{L}\p{N}]+(?:['’\-][\p{L}\p{N}]+)*"
            ).Count < 8;
        }

        private static bool ContainsCjkText(string value)
        {
            return Regex.IsMatch(
                value ?? "",
                @"[\u3400-\u4DBF\u4E00-\u9FFF\u3040-\u30FF\uAC00-\uD7AF]"
            );
        }

        private static string NormalizeSeoText(string value)
        {
            return Regex.Replace(value ?? "", @"\s+", " ").Trim();
        }

        private static string NormalizeSeoComparisonText(string value)
        {
            return new string((value ?? "")
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private static void AddDistinctSeoPart(List<string> parts, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var normalizedValue = NormalizeSeoComparisonText(value);
            if (parts.Any(part => NormalizeSeoComparisonText(part) == normalizedValue))
            {
                return;
            }

            parts.Add(value.Trim().TrimEnd('。', '.', '，', ',', '；', ';'));
        }

        private static string TruncateSeoDescription(string value, int maximumLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maximumLength)
            {
                return value;
            }

            var truncated = value.Substring(0, maximumLength).TrimEnd();
            var lastBoundary = truncated.LastIndexOfAny(new[] { ' ', '。', '，', ',', '；', ';' });
            if (lastBoundary >= maximumLength / 2)
            {
                truncated = truncated.Substring(0, lastBoundary).TrimEnd();
            }

            return truncated;
        }

        private Task<HtmlSanitizeResult> SanitizeProductPublishedContentAsync(
            long websiteId,
            long productId,
            string html,
            string css,
            bool force = false)
        {
            return htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
            {
                WebsiteId = websiteId,
                SourceType = HtmlSanitizeSourceType.商品,
                SourceId = productId,
                ContentKey = "Published",
                SanitizePolicy = "PublicHtml",
                Html = html ?? "",
                Css = css ?? "",
                Force = force
            });
        }

        private async Task<(string Html, string Css)> EnsureProductDisplayContentSanitizedAsync(Prod product)
        {
            var publishedHtml = stringHandler.HtmlDecode(product.Html ?? "");
            var restoredHtml = htmlSanitizeService.RepairLegacyPublishedHtml(
                publishedHtml,
                stringHandler.HtmlDecode(product.SaveHtml ?? "")
            );
            var repairedLegacyHtml = !string.Equals(
                publishedHtml,
                restoredHtml,
                StringComparison.Ordinal
            );

            var sanitized = await SanitizeProductPublishedContentAsync(
                product.FK_WebsiteId,
                product.Id,
                restoredHtml,
                product.Css ?? "",
                repairedLegacyHtml
            );

            if (sanitized.WasSanitized)
            {
                product.Html = stringHandler.HtmlEncode(sanitized.Html);
                product.Css = sanitized.Css;
                product.PageText = htmlProcessor.text(sanitized.Html);
                await loginUserData.SaveChanges(product);
            }

            return (sanitized.Html, sanitized.Css);
        }
    }
}
