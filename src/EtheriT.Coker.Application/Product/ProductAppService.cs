using AutoMapper;
using DevExpress.XtraCharts;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Import;
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
        private readonly ImportAppService importAppService;
        private readonly IFrontRoleContextService frontRoleContextService;
        private readonly IProductDisplayPriceService productDisplayPriceService;
        private readonly IWebsiteCacheStateAppService websiteCacheStateAppService;
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
            ImportAppService importAppService,
            IFrontRoleContextService frontRoleContextService,
            IProductDisplayPriceService productDisplayPriceService,
            IWebsiteCacheStateAppService websiteCacheStateAppService,
            IHtmlSanitizeService htmlSanitizeService
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
            this.storeSetAppService = storeSetAppService;
            this.webMenuApplication = webMenuApplication;
            this.tokenAppService = tokenAppService;
            this.stringHandler = stringHandler;
            this.htmlProcessor = htmlProcessor;
            this.frontRoleContextService = frontRoleContextService;
            this.productDisplayPriceService = productDisplayPriceService;
            this.websiteCacheStateAppService = websiteCacheStateAppService;
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
                    Id = s.Id,
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
                Id = x.Id,
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

                    var exportPrices = new List<Prod_Price?>();
                    if (stock != null && !stock.IsTimePrice && pricesByStock.TryGetValue(stock.Id, out var stockPrices))
                        exportPrices.AddRange(stockPrices);
                    if (exportPrices.Count == 0)
                        exportPrices.Add(null);

                    foreach (var rolePrice in exportPrices)
                    {
                        productRows.Add(new ProductExportRow
                        {
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
                        SpecDescription = stock?.SpecDescription ?? "",
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
                                : (rolePrice?.Price ?? stock.Price).ToString(System.Globalization.CultureInfo.InvariantCulture),
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
                var s = await SaveConten(dto);
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
            return response;
        }
        public async Task<ResponseMessageDto> SaveConten(ProdSaveContenDto dto)
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
                        result.Description = !string.IsNullOrEmpty(prod.Description) ? prod.Description :
                                                !string.IsNullOrEmpty(prod.Introduction) ? prod.Introduction : htmlProcessor.text(stringHandler.HtmlDecode(prod.Html));
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

        /* Product Import */
        public async Task<ImportOutputDto> ProdReplace(
            IList<IFormFile> files,
            long templateId,
            bool overwriteExisting)
        {
            return await ProdReplace(files, templateId, overwriteExisting, false);
        }

        public async Task<ImportOutputDto> ProdReplace(
            IList<IFormFile> files,
            long templateId,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles)
        {
            ProdImportAllDto fileData = await importAppService.ProdReplace(files);
            return await ImportProductData(
                fileData,
                templateId,
                overwriteExisting,
                allowDuplicateMenuTitles,
                true,
                true,
                true,
                true,
                true,
                null);
        }

        public async Task<ImportOutputDto> ProdReplace(
            string filePath,
            long templateId,
            bool overwriteExisting,
            Action<int, string>? reportProgress)
        {
            return await ProdReplace(
                filePath,
                templateId,
                overwriteExisting,
                false,
                true,
                true,
                true,
                true,
                true,
                new List<ProductImportIgnoredRowDto>(),
                reportProgress);
        }

        public async Task<ImportOutputDto> ProdReplace(
            string filePath,
            long templateId,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles,
            Action<int, string>? reportProgress)
        {
            return await ProdReplace(
                filePath,
                templateId,
                overwriteExisting,
                allowDuplicateMenuTitles,
                true,
                true,
                true,
                true,
                true,
                new List<ProductImportIgnoredRowDto>(),
                reportProgress);
        }

        public async Task<ImportOutputDto> ProdReplace(
            string filePath,
            long templateId,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles,
            bool overwriteExistingMenuParents,
            bool overwriteExistingProductNames,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices,
            bool overwriteExistingTechnicalCertificates,
            List<ProductImportIgnoredRowDto> ignoredRows,
            Action<int, string>? reportProgress)
        {
            reportProgress?.Invoke(5, "正在讀取商品匯入檔案");
            var strategy = db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();
                var fileData = await importAppService.ProdReplace(filePath);
                ApplyIgnoredProductImportRows(fileData, ignoredRows);
                var response = await ImportProductData(
                    fileData,
                    templateId,
                    overwriteExisting,
                    allowDuplicateMenuTitles,
                    overwriteExistingMenuParents,
                    overwriteExistingProductNames,
                    overwriteExistingSpecs,
                    overwriteExistingPrices,
                    overwriteExistingTechnicalCertificates,
                    reportProgress);
                if (response.Success)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();
                return response;
            });
        }

        private static void ApplyIgnoredProductImportRows(
            ProdImportAllDto fileData,
            List<ProductImportIgnoredRowDto>? ignoredRows)
        {
            if (ignoredRows == null || ignoredRows.Count == 0) return;
            var ignored = ignoredRows
                .GroupBy(e => e.Sheet, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    e => e.Key,
                    e => e.Select(row => row.RowNumber).ToHashSet(),
                    StringComparer.OrdinalIgnoreCase);

            if (ignored.TryGetValue("商品", out var productRows))
                fileData.Products = fileData.Products.Where(e => !productRows.Contains(e.SourceRowNumber)).ToList();
            if (ignored.TryGetValue("目錄分類", out var directoryRows))
                fileData.Directories = fileData.Directories.Where(e => !directoryRows.Contains(e.SourceRowNumber)).ToList();
            if (ignored.TryGetValue("技術證照", out var techRows))
            {
                fileData.TechnicalCertificates = fileData.TechnicalCertificates
                    .Where(e => !techRows.Contains(e.SourceRowNumber)).ToList();
                var retainedTechKeys = fileData.TechnicalCertificates
                    .Select(e => $"{Norm(e.ItemNo)}|{Norm(e.ProdName)}|{Norm(e.Title)}")
                    .ToHashSet();
                foreach (var product in fileData.Products)
                    product.Techs = product.Techs?.Where(tech => retainedTechKeys.Contains(
                        $"{Norm(product.ItemNo)}|{Norm(product.ProdName)}|{Norm(tech.Title)}")).ToList();
            }
        }

        public async Task<ProductImportAnalysisDto> AnalyzeProductImport(
            string filePath,
            long templateId,
            List<ProductImportIgnoredRowDto> ignoredRows,
            Action<int, string>? reportProgress)
        {
            reportProgress?.Invoke(5, "正在讀取三個商品匯入工作表");
            var fileData = await importAppService.ProdReplace(filePath);
            ApplyIgnoredProductImportRows(fileData, ignoredRows);
            var websiteId = await loginUserData.GetWebsiteId();
            _ = await GetProductImportTemplate(templateId, websiteId);
            var result = new ProductImportAnalysisDto();
            result.Summary.DetectedUpdateScopes = GetDetectedProductImportScopes(fileData);
            result.Summary.ProductRowCount = fileData.Products.Count(e => !string.IsNullOrWhiteSpace(e.ProdName));
            result.Summary.DirectoryRowCount = fileData.Directories.Count;
            result.Summary.MenuCount = CountDirectoryMenuRequests(fileData.Directories);
            result.Summary.ProductBeforeCount = await db.Prods.AsNoTracking()
                .CountAsync(e => !e.IsDeleted && e.FK_WebsiteId == websiteId);
            result.Summary.MenuBeforeCount = await db.WebMenus.AsNoTracking()
                .CountAsync(e => !e.IsDeleted && e.FK_WebsiteId == websiteId);

            reportProgress?.Invoke(25, "正在檢查商品工作表");
            var productRows = fileData.Products
                .Where(e => !string.IsNullOrWhiteSpace(e.ProdName))
                .ToList();
            result.Summary.ProductCount = productRows
                .GroupBy(e => string.IsNullOrWhiteSpace(e.ItemNo)
                    ? $"title:{Norm(e.ProdName)}"
                    : $"item:{Norm(e.ItemNo)}")
                .Count();
            var productNameConflictItemNos = productRows
                .Where(e => !string.IsNullOrWhiteSpace(e.ItemNo))
                .GroupBy(e => Norm(e.ItemNo))
                .Where(group => group
                    .Select(e => Norm(e.ProdName))
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct()
                    .Count() > 1)
                .Select(group => group.Key)
                .ToHashSet();
            result.Errors.AddRange(ValidateProductImportConflicts(productRows));
            var frontRoles = await db.Roles.AsNoTracking()
                .Where(e => e.FK_WebsiteId == websiteId
                    && e.Type == RoleTypeEnum.前台
                    && !e.IsDeleted)
                .Select(e => new { e.Id, e.Name })
                .ToListAsync();
            var frontRoleKeys = frontRoles.Select(e => Norm(e.Name)).ToHashSet();
            var frontRoleMap = frontRoles
                .GroupBy(e => Norm(e.Name))
                .ToDictionary(e => e.Key, e => e.Last().Id);
            foreach (var invalidRoleGroup in productRows
                .Where(e => !string.IsNullOrWhiteSpace(e.RoleName)
                    && Norm(e.RoleName) != Norm("非會員")
                    && !frontRoleKeys.Contains(Norm(e.RoleName)))
                .GroupBy(e => Norm(e.RoleName)))
            {
                var invalidRole = invalidRoleGroup.First();
                result.Errors.Add(new ImportMassageItem
                {
                    Name = $"商品會員價格：{invalidRole.RoleName}",
                    Description = "找不到對應的前台會員身分，請修正 Excel 或先建立會員身分。",
                    Sheet = "商品",
                    RowNumbers = invalidRoleGroup
                        .Select(e => e.SourceRowNumber)
                        .Distinct()
                        .OrderBy(e => e)
                        .ToList(),
                    CanIgnore = true
                });
            }

            var itemNos = productRows
                .Where(e => !string.IsNullOrWhiteSpace(e.ItemNo))
                .Select(e => e.ItemNo)
                .Distinct()
                .ToList();
            var productTitlesWithoutItemNo = productRows
                .Where(e => string.IsNullOrWhiteSpace(e.ItemNo))
                .Select(e => e.ProdName)
                .Distinct()
                .ToList();
            var existingProducts = await db.Prods.AsNoTracking()
                .Where(e => !e.IsDeleted
                    && e.FK_WebsiteId == websiteId
                    && ((!string.IsNullOrEmpty(e.ItemNo) && itemNos.Contains(e.ItemNo))
                        || (string.IsNullOrEmpty(e.ItemNo) && productTitlesWithoutItemNo.Contains(e.Title))))
                .Select(e => new { e.Id, e.ItemNo, e.Title })
                .ToListAsync();
            result.Summary.ProductUpdatedCount = productRows
                .Where(e => existingProducts.Any(p => string.IsNullOrWhiteSpace(e.ItemNo)
                    ? string.IsNullOrWhiteSpace(p.ItemNo) && Norm(p.Title) == Norm(e.ProdName)
                    : Norm(p.ItemNo) == Norm(e.ItemNo)))
                .GroupBy(e => string.IsNullOrWhiteSpace(e.ItemNo)
                    ? $"title:{Norm(e.ProdName)}"
                    : $"item:{Norm(e.ItemNo)}")
                .Count();
            result.Summary.ProductAddedCount = Math.Max(
                0,
                result.Summary.ProductCount - result.Summary.ProductUpdatedCount);
            foreach (var product in productRows
                .Where(e => !string.IsNullOrWhiteSpace(e.ItemNo)
                    && !productNameConflictItemNos.Contains(Norm(e.ItemNo)))
                .GroupBy(e => Norm(e.ItemNo))
                .Select(e => e.First()))
            {
                var existing = existingProducts.FirstOrDefault(e => Norm(e.ItemNo) == Norm(product.ItemNo));
                if (existing != null && Norm(existing.Title) != Norm(product.ProdName))
                {
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.ProductName,
                        Sheet = "商品",
                        Name = $"ItemNo：{product.ItemNo}",
                        ExistingValue = existing.Title ?? string.Empty,
                        ExcelValue = product.ProdName,
                        Description = "商品名稱不同，可保留現有名稱或授權以 Excel 更名。"
                    });
                }
            }

            var productIds = existingProducts.Select(e => e.Id).ToList();
            var existingStocks = await db.Prod_Stocks.AsNoTracking()
                .Where(e => !e.IsDeleted
                    && productIds.Contains(e.FK_Pid))
                .Select(e => new { e.Id, e.FK_Pid, e.SubItemNo, e.FK_S1id, e.FK_S2id, e.Price, e.IsTimePrice })
                .ToListAsync();
            var specIds = existingStocks
                .SelectMany(e => new[] { e.FK_S1id, e.FK_S2id })
                .Where(e => e.HasValue)
                .Select(e => e!.Value)
                .Distinct()
                .ToList();
            var existingSpecs = await db.Prod_Specs.AsNoTracking()
                .Include(e => e.Prod_Spec_Type)
                .Where(e => specIds.Contains(e.Id))
                .ToDictionaryAsync(
                    e => e.Id,
                    e => new { Type = e.Prod_Spec_Type != null ? e.Prod_Spec_Type.Type : string.Empty, e.Title });
            string DescribeExistingSpecLocal(long? spec1Id, long? spec2Id)
            {
                var values = new List<string>();
                if (spec1Id.HasValue && existingSpecs.TryGetValue(spec1Id.Value, out var spec1))
                    values.Add(string.IsNullOrWhiteSpace(spec1.Type) ? spec1.Title : $"{spec1.Type}：{spec1.Title}");
                if (spec2Id.HasValue && existingSpecs.TryGetValue(spec2Id.Value, out var spec2))
                    values.Add(string.IsNullOrWhiteSpace(spec2.Type) ? spec2.Title : $"{spec2.Type}：{spec2.Title}");
                return values.Count == 0 ? "無規格" : string.Join("／", values);
            }
            foreach (var row in productRows.Where(e => !string.IsNullOrWhiteSpace(e.SubItemNo)))
            {
                var existingProduct = existingProducts.FirstOrDefault(e => Norm(e.ItemNo) == Norm(row.ItemNo));
                if (existingProduct == null) continue;
                var stock = existingStocks.FirstOrDefault(e => e.FK_Pid == existingProduct.Id
                    && Norm(e.SubItemNo) == Norm(row.SubItemNo));
                if (stock == null) continue;

                var existingSpec = DescribeExistingSpecLocal(stock.FK_S1id, stock.FK_S2id);
                var excelSpec = DescribeProductImportSpec(row);
                if (Norm(existingSpec) != Norm(excelSpec))
                {
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.ProductSpec,
                        Sheet = "商品",
                        Name = $"ItemNo：{row.ItemNo}／SubItemNo：{row.SubItemNo}",
                        ExistingValue = existingSpec,
                        ExcelValue = excelSpec,
                        Description = "同一規格編號的規格不同，可保留現有規格或授權以 Excel 更新。"
                    });
                }
            }

            var existingStockIds = existingStocks.Select(e => e.Id).ToList();
            var existingPrices = await db.Prod_Prices.AsNoTracking()
                .Where(e => !e.IsDeleted && existingStockIds.Contains(e.FK_PSId))
                .Select(e => new { e.Id, e.FK_PSId, e.FK_RId, e.Bonus, e.Price })
                .ToListAsync();
            foreach (var row in productRows.Where(e =>
                HasImportedColumn(e, nameof(e.Price))
                || HasImportedColumn(e, nameof(e.SuggestPrice))))
            {
                var existingProduct = existingProducts.FirstOrDefault(e => string.IsNullOrWhiteSpace(row.ItemNo)
                    ? string.IsNullOrWhiteSpace(e.ItemNo) && Norm(e.Title) == Norm(row.ProdName)
                    : Norm(e.ItemNo) == Norm(row.ItemNo));
                if (existingProduct == null) continue;

                var stock = !string.IsNullOrWhiteSpace(row.SubItemNo)
                    ? existingStocks.FirstOrDefault(e => e.FK_Pid == existingProduct.Id
                        && Norm(e.SubItemNo) == Norm(row.SubItemNo))
                    : existingStocks.FirstOrDefault(e => e.FK_Pid == existingProduct.Id
                        && Norm(DescribeExistingSpecLocal(e.FK_S1id, e.FK_S2id)) == Norm(DescribeProductImportSpec(row)));
                if (stock == null) continue;

                var existingValues = new List<string>();
                var excelValues = new List<string>();
                if (HasImportedColumn(row, nameof(row.Price)))
                {
                    var roleName = string.IsNullOrWhiteSpace(row.RoleName) ? "非會員" : row.RoleName.Trim();
                    var roleId = Norm(roleName) == Norm("非會員")
                        ? 1
                        : frontRoleMap.GetValueOrDefault(Norm(roleName));
                    if (roleId > 0)
                    {
                        var currentPrice = existingPrices
                            .Where(e => e.FK_PSId == stock.Id
                                && e.FK_RId == roleId
                                && (int)(e.Bonus ?? 0) == row.Bonus)
                            .OrderByDescending(e => e.Id)
                            .FirstOrDefault();
                        var excelIsTimePrice = row.Price < 0;
                        var priceChanged = excelIsTimePrice != stock.IsTimePrice
                            || (!excelIsTimePrice && (currentPrice == null || currentPrice.Price != row.Price));
                        if (priceChanged)
                        {
                            existingValues.Add(stock.IsTimePrice
                                ? $"{roleName}：時價"
                                : $"{roleName}／紅利 {row.Bonus}：{(currentPrice == null ? "尚無價格" : $"${currentPrice.Price:N0}")}");
                            excelValues.Add(excelIsTimePrice
                                ? $"{roleName}：時價"
                                : $"{roleName}／紅利 {row.Bonus}：${row.Price:N0}");
                        }
                    }
                }
                if (HasImportedColumn(row, nameof(row.SuggestPrice))
                    && stock.Price != row.SuggestPrice)
                {
                    existingValues.Add($"建議售價：${stock.Price:N0}");
                    excelValues.Add($"建議售價：${row.SuggestPrice:N0}");
                }
                if (existingValues.Count == 0) continue;

                result.Differences.Add(new ProductImportDifferenceDto
                {
                    Code = ProductImportDifferenceCodes.ProductPrice,
                    Sheet = "商品",
                    Name = $"Excel 第 {row.SourceRowNumber} 列／ItemNo：{row.ItemNo}／SubItemNo：{row.SubItemNo}",
                    ExistingValue = string.Join("；", existingValues),
                    ExcelValue = string.Join("；", excelValues),
                    Description = "價格不同；未授權時保留資料庫價格，授權後才以 Excel 更新。"
                });
            }

            reportProgress?.Invoke(50, "正在檢查目錄分類工作表");
            var directoryValidation = await ValidateDirectoryImportStructureAsync(
                fileData.Directories,
                websiteId,
                true);
            result.Errors.AddRange(directoryValidation.Errors);
            foreach (var titleGroup in GetDirectoryMenuRequests(fileData.Directories)
                .GroupBy(e => Norm(e.Title)))
            {
                var routers = titleGroup
                    .Select(e => Norm(e.RouterName))
                    .Distinct()
                    .ToList();
                if (routers.Count > 1)
                {
                    result.Errors.Add(new ImportMassageItem
                    {
                        Name = $"目錄分類：{titleGroup.First().Title}",
                        Description = $"Excel 內相同選單名稱使用不同 RouterName：{string.Join("、", titleGroup.Select(e => string.IsNullOrWhiteSpace(e.RouterName) ? "(空白)" : e.RouterName).Distinct())}。",
                        Sheet = "目錄分類",
                        RowNumbers = fileData.Directories
                            .Where(row => new[] { row.Level1, row.Level2, row.Level3 }.Any(title => Norm(title) == titleGroup.Key))
                            .Select(row => row.SourceRowNumber).Distinct().OrderBy(e => e).ToList(),
                        CanIgnore = true
                    });
                }
            }
            var menus = await db.WebMenus.AsNoTracking()
                .Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                .ToListAsync();
            result.Summary.MenuExistingCount = GetDirectoryMenuRequests(fileData.Directories)
                .Count(request => FindMenuByRouterOrTitle(
                    menus,
                    request.Title,
                    request.RouterName,
                    true) != null);
            result.Summary.MenuAddedCount = Math.Max(
                0,
                result.Summary.MenuCount - result.Summary.MenuExistingCount);
            result.Summary.ProductAfterCount = result.Summary.ProductBeforeCount + result.Summary.ProductAddedCount;
            result.Summary.MenuAfterCount = result.Summary.MenuBeforeCount + result.Summary.MenuAddedCount;
            foreach (var row in fileData.Directories)
            {
                var level1 = FindMenuByRouterOrTitle(menus, row.Level1, row.Level1RouterName, true);
                var level2 = FindMenuByRouterOrTitle(menus, row.Level2, row.Level2RouterName, true);
                var level3 = FindMenuByRouterOrTitle(menus, row.Level3, row.Level3RouterName, true);
                if (level1 != null && level2 != null
                    && level2.FK_TopNodeId != level1.Id)
                {
                    var currentParent = menus.FirstOrDefault(e => e.Id == level2.FK_TopNodeId);
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.MenuParent,
                        Sheet = "目錄分類",
                        Name = level2.Title ?? row.Level2 ?? string.Empty,
                        ExistingValue = currentParent?.Title ?? "無父層",
                        ExcelValue = level1.Title ?? row.Level1,
                        Description = "選單父層不同，可保留現有位置或授權依 Excel 搬移。"
                    });
                }
                if (level2 != null && level3 != null
                    && level3.FK_TopNodeId != level2.Id)
                {
                    var currentParent = menus.FirstOrDefault(e => e.Id == level3.FK_TopNodeId);
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.MenuParent,
                        Sheet = "目錄分類",
                        Name = level3.Title ?? row.Level3 ?? string.Empty,
                        ExistingValue = currentParent?.Title ?? "無父層",
                        ExcelValue = level2.Title ?? row.Level2 ?? string.Empty,
                        Description = "選單父層不同，可保留現有位置或授權依 Excel 搬移。"
                    });
                }
            }
            foreach (var request in GetDirectoryMenuRequests(fileData.Directories))
            {
                if (string.IsNullOrWhiteSpace(request.RouterName)) continue;
                var sameTitle = menus.FirstOrDefault(e => Norm(e.Title) == Norm(request.Title));
                if (sameTitle != null && Norm(sameTitle.RouterName) != Norm(request.RouterName))
                {
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.DuplicateMenuTitle,
                        Sheet = "目錄分類",
                        Name = request.Title,
                        ExistingValue = sameTitle.RouterName,
                        ExcelValue = request.RouterName,
                        Description = "選單名稱相同但 RouterName 不同，可沿用既有選單或授權建立同名選單。"
                    });
                }
            }

            var leafRequests = GetLeafDirectoryMenuRequests(fileData.Directories);
            foreach (var request in leafRequests)
            {
                var menu = FindMenuByRouterOrTitle(
                    menus,
                    request.Title,
                    request.RouterName,
                    useTitleFallback: string.IsNullOrWhiteSpace(request.RouterName));
                if (menu != null && !string.IsNullOrWhiteSpace(menu.SaveHtml))
                {
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.DirectoryPage,
                        Sheet = "目錄分類",
                        Name = string.IsNullOrWhiteSpace(request.RouterName)
                            ? request.Title
                            : $"{request.Title}（{request.RouterName}）",
                        ExistingValue = "已有頁面內容",
                        ExcelValue = "套用本次選擇的版型",
                        Description = "差異原因：此目錄已經有頁面內容。這不是名稱、RouterName 或父層位置衝突；可保留現有目錄頁，或授權以本次選擇的版型重新產生頁面。"
                    });
                }
            }

            reportProgress?.Invoke(72, "正在檢查技術證照工作表");
            var productKeys = productRows
                .Select(e => $"{Norm(e.ItemNo)}|{Norm(e.ProdName)}")
                .ToHashSet();
            foreach (var tech in fileData.TechnicalCertificates)
            {
                if (!productKeys.Contains($"{Norm(tech.ItemNo)}|{Norm(tech.ProdName)}"))
                {
                    result.Errors.Add(new ImportMassageItem
                    {
                        Name = $"技術證照：{tech.Title}",
                        Description = $"找不到對應商品 ItemNo「{tech.ItemNo}」／商品名稱「{tech.ProdName}」。",
                        Sheet = "技術證照",
                        RowNumbers = new List<int> { tech.SourceRowNumber },
                        CanIgnore = true
                    });
                }
            }
            foreach (var techGroup in fileData.TechnicalCertificates
                .Where(e => !string.IsNullOrWhiteSpace(e.Title))
                .GroupBy(e => Norm(e.Title)))
            {
                var values = techGroup
                    .GroupBy(e => $"{Norm(e.Description)}|{Norm(e.Image1)}|{e.Ser_no}")
                    .ToList();
                if (values.Count > 1)
                {
                    result.Errors.Add(new ImportMassageItem
                    {
                        Name = $"技術證照：{techGroup.First().Title}",
                        Description = "Excel 內相同證照名稱對應到不同圖片、說明或排序。",
                        Sheet = "技術證照",
                        RowNumbers = techGroup.Select(e => e.SourceRowNumber).Distinct().OrderBy(e => e).ToList(),
                        CanIgnore = true
                    });
                }
            }
            var techTitles = fileData.TechnicalCertificates
                .Select(e => e.Title)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();
            var currentTechs = await db.TechnicalCertificates.AsNoTracking()
                .Where(e => !e.IsDeleted
                    && e.FK_WebsiteId == websiteId
                    && techTitles.Contains(e.Title))
                .ToListAsync();
            foreach (var tech in fileData.TechnicalCertificates.GroupBy(e => Norm(e.Title)).Select(e => e.First()))
            {
                var current = currentTechs.FirstOrDefault(e => Norm(e.Title) == Norm(tech.Title));
                if (current == null) continue;
                if (Norm(current.Description) != Norm(tech.Description)
                    || current.Ser_no != tech.Ser_no
                    || (!string.IsNullOrWhiteSpace(tech.Image1) && Norm(current.Img) != Norm(tech.Image1)))
                {
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.TechnicalCertificate,
                        Sheet = "技術證照",
                        Name = tech.Title,
                        ExistingValue = $"說明：{current.Description}／排序：{current.Ser_no}",
                        ExcelValue = $"說明：{tech.Description}／排序：{tech.Ser_no}",
                        Description = "證照內容不同，可保留現有內容或授權以 Excel 覆蓋。"
                    });
                }
            }

            result.Errors = result.Errors
                .GroupBy(e => $"{e.Name}|{e.Description}")
                .Select(group =>
                {
                    var first = group.First();
                    first.RowNumbers = group
                        .SelectMany(e => e.RowNumbers ?? new List<int>())
                        .Distinct()
                        .OrderBy(e => e)
                        .ToList();
                    first.ComparisonValues = group
                        .SelectMany(e => e.ComparisonValues ?? new List<ImportMassageComparisonValue>())
                        .GroupBy(e => new { e.RowNumber, e.Label, e.Value })
                        .Select(e => e.First())
                        .OrderBy(e => e.RowNumber)
                        .ToList();
                    first.CanIgnore = group.All(e => e.CanIgnore);
                    return first;
                })
                .ToList();
            result.Differences = result.Differences
                .GroupBy(e => $"{e.Code}|{e.Name}|{e.ExistingValue}|{e.ExcelValue}")
                .Select(e => e.First())
                .ToList();
            reportProgress?.Invoke(95, "商品匯入檔掃描完成");
            return result;
        }

        private async Task<ImportOutputDto> ImportProductData(
            ProdImportAllDto fileData,
            long templateId,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles,
            bool overwriteExistingMenuParents,
            bool overwriteExistingProductNames,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices,
            bool overwriteExistingTechnicalCertificates,
            Action<int, string>? reportProgress)
        {
            ImportOutputDto response = new ImportOutputDto { ErrorList = new List<ImportMassageItem>() };
            bool productImportFailed = false;
            long WebsiteID = await loginUserData.GetWebsiteId();
            response.Summary.DetectedUpdateScopes = GetDetectedProductImportScopes(fileData);
            response.Summary.ProductBeforeCount = await db.Prods.AsNoTracking()
                .CountAsync(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID);
            response.Summary.MenuBeforeCount = await db.WebMenus.AsNoTracking()
                .CountAsync(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID);
            response.Summary.ProductRowCount = fileData.Products.Count(e => !string.IsNullOrWhiteSpace(e.ProdName));
            response.Summary.DirectoryRowCount = fileData.Directories.Count;
            if (fileData.Directories.Any())
            {
                var directoryValidation = await ValidateDirectoryImportStructureAsync(
                    fileData.Directories,
                    WebsiteID,
                    allowDuplicateMenuTitles);
                response.ErrorList.AddRange(directoryValidation.Errors);
                fileData.Directories = fileData.Directories
                    .Where((_, index) => !directoryValidation.InvalidRowIndexes.Contains(index))
                    .ToList();
            }
            response.Summary.DirectoryRowCount = fileData.Directories.Count;
            response.Summary.MenuCount = CountDirectoryMenuRequests(fileData.Directories);
            var importTemplate = await GetProductImportTemplate(templateId, WebsiteID);
            reportProgress?.Invoke(15, "正在驗證商品與會員價格資料");
            if (fileData.Products.Any())
            {
                List<ProductImportDto> allData = fileData.Products.FindAll(e => !string.IsNullOrEmpty(e.ProdName));
                response.ErrorList.AddRange(ValidateProductImportConflicts(allData));
                var frontRoles = await db.Roles
                    .AsNoTracking()
                    .Where(e => e.FK_WebsiteId == WebsiteID && e.Type == RoleTypeEnum.前台 && !e.IsDeleted)
                    .Select(e => new { e.Id, e.Name })
                    .ToListAsync();
                var frontRoleMap = frontRoles
                    .GroupBy(e => Norm(e.Name))
                    .ToDictionary(e => e.Key, e => e.Last().Id);

                foreach (var row in allData)
                {
                    var roleName = (row.RoleName ?? "").Trim();
                    if (string.IsNullOrEmpty(roleName) || roleName == "非會員")
                    {
                        row.RoleId = 1;
                        row.Bonus = 0;
                    }
                    else if (frontRoleMap.TryGetValue(Norm(roleName), out var roleId))
                    {
                        row.RoleId = roleId;
                    }
                    else
                    {
                        row.RoleId = 0;
                        response.ErrorList.Add(new ImportMassageItem
                        {
                            Name = row.ProdName,
                            Description = $"找不到會員身分「{roleName}」，該列價格已略過。"
                        });
                    }
                }
                List<ProductImportDto> prods = new List<ProductImportDto>();
                List<string> allTitles = allData.Select(p => p.ProdName).ToList();
                List<string> allItemNos = allData.Select(p => p.ItemNo).ToList();
                var updateItems = db.Prods.Where(e => !e.IsDeleted)
                    .Where(e => e.FK_WebsiteId == WebsiteID)
                    .Where(p => string.IsNullOrEmpty(p.ItemNo)
                        ? allTitles.Contains((p.Title ?? "").Trim())
                        : allItemNos.Contains((p.ItemNo ?? "").Trim()))
                    .Select(s => new { s.Id, s.ItemNo, s.Title }).ToList();
                ProductImportDto dto = null;
                for (int i = 0; i < allData.Count; i++)
                {
                    var el = allData[i];
                    var item = updateItems.Find(e => string.IsNullOrEmpty(el.ItemNo)
                        ? Norm(e.Title) == Norm(el.ProdName)
                        : Norm(e.ItemNo) == Norm(el.ItemNo));
                    el.FK_WebsiteId = WebsiteID;
                    if (item != null) el.Id = item.Id;
                    var preProds = prods.Find(e => string.IsNullOrEmpty(el.ItemNo)
                        ? Norm(e.ProdName) == Norm(el.ProdName)
                        : Norm(e.ItemNo) == Norm(el.ItemNo));
                    if (preProds == null)
                    {
                        dto = el;
                        dto.stocks = new List<ProductStockDto>();
                        prods.Add(el);
                    }
                    else dto = preProds;
                    if (dto != null && dto.stocks != null) dto.stocks.Add(mapper.Map<ProductStockDto>(el));
                }
                response.Summary.ProductCount = prods.Count;
                response.Summary.ProductAddedCount = prods.Count(e => e.Id == 0);
                response.Summary.ProductUpdatedCount = prods.Count(e => e.Id != 0);
                try
                {
                    reportProgress?.Invoke(30, "正在匯入商品、規格與價格");
                    var productCounts = await importProds(
                        prods,
                        response.ErrorList,
                        overwriteExistingProductNames,
                        overwriteExistingSpecs,
                        overwriteExistingPrices,
                        overwriteExistingTechnicalCertificates,
                        fileData.TechnicalCertificates.Count > 0,
                        reportProgress);
                    response.Summary.ProductAddedCount = productCounts.AddedCount;
                    response.Summary.ProductUpdatedCount = productCounts.UpdatedCount;
                    if (prods.Count > 0)
                    {
                        // 商品本身或商品標籤異動都會改變目錄可顯示的商品集合。
                        // 整批匯入完成後只更新一次版本，讓既有目錄內容快照失效。
                        await websiteCacheStateAppService.TouchByWebsiteIdAsync(
                            WebsiteID,
                            WebsiteCacheKeys.DirectoryContent);
                    }
                    response.Success = true;
                }
                catch (Exception ex)
                {
                    productImportFailed = true;
                    response.ErrorList.Add(new ImportMassageItem { Name = "error", Description = ex.Message });
                }
            }
            if (fileData.Directories.Any())
            {
                reportProgress?.Invoke(88, "正在匯入商品目錄與標籤");
                await imporDirectories(
                    fileData.Directories,
                    importTemplate,
                    overwriteExisting,
                    allowDuplicateMenuTitles,
                    overwriteExistingMenuParents);
                if (!productImportFailed)
                    response.Success = true;
            }

            response.Summary.ProductAfterCount = await db.Prods.AsNoTracking()
                .CountAsync(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID);
            response.Summary.MenuAfterCount = await db.WebMenus.AsNoTracking()
                .CountAsync(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID);
            response.Summary.MenuAddedCount = Math.Max(
                0,
                response.Summary.MenuAfterCount - response.Summary.MenuBeforeCount);
            response.Summary.MenuExistingCount = Math.Max(
                0,
                response.Summary.MenuCount - response.Summary.MenuAddedCount);
            reportProgress?.Invoke(98, "商品匯入處理完成");
            return response;
        }

        private static List<string> GetDetectedProductImportScopes(ProdImportAllDto fileData)
        {
            var columns = fileData.Products
                .SelectMany(e => e.ImportedColumns)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var scopes = new List<string>();

            bool HasAny(params string[] names) => names.Any(columns.Contains);

            if (HasAny(
                nameof(ProductImportDto.ItemNo), nameof(ProductImportDto.ProdName),
                nameof(ProductImportDto.Status), nameof(ProductImportDto.Introduction),
                nameof(ProductImportDto.Description), nameof(ProductImportDto.SaveHtml),
                nameof(ProductImportDto.Html), nameof(ProductImportDto.SaveCss),
                nameof(ProductImportDto.StartTime), nameof(ProductImportDto.EndTime),
                nameof(ProductImportDto.Visible), nameof(ProductImportDto.OnShelf),
                nameof(ProductImportDto.Tag1), nameof(ProductImportDto.Tag2),
                nameof(ProductImportDto.Tag3), nameof(ProductImportDto.Tag4),
                nameof(ProductImportDto.Tag5), nameof(ProductImportDto.Tag6)))
                scopes.Add("商品基本資料");

            if (HasAny(
                nameof(ProductImportDto.SubItemNo), nameof(ProductImportDto.Spec1Name),
                nameof(ProductImportDto.Spec1), nameof(ProductImportDto.Spec2Name),
                nameof(ProductImportDto.Spec2), nameof(ProductImportDto.SpecDescription)))
                scopes.Add("規格");

            if (HasAny(
                nameof(ProductImportDto.Stock), nameof(ProductImportDto.Min_Qty),
                nameof(ProductImportDto.Alert_Qty)))
                scopes.Add("庫存");

            if (HasAny(
                nameof(ProductImportDto.RoleName), nameof(ProductImportDto.Price),
                nameof(ProductImportDto.Bonus), nameof(ProductImportDto.SuggestPrice)))
                scopes.Add("價格");

            if (fileData.Directories.Count > 0)
                scopes.Add("目錄／選單");
            if (fileData.TechnicalCertificates.Count > 0)
                scopes.Add("技術證照");

            return scopes;
        }

        private static List<ImportMassageItem> ValidateProductImportConflicts(
            List<ProductImportDto> products)
        {
            var warnings = new List<ImportMassageItem>();
            var itemNoGroups = products
                .Where(e => !string.IsNullOrWhiteSpace(e.ItemNo))
                .GroupBy(e => Norm(e.ItemNo));

            foreach (var group in itemNoGroups)
            {
                var names = group
                    .Select(e => e.ProdName)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .GroupBy(Norm)
                    .Select(e => e.First())
                    .ToList();
                var itemNo = group.First().ItemNo;
                if (names.Count > 1)
                {
                    var importDetails = group
                        .Select(e => $"匯入商品名稱「{e.ProdName}」")
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    warnings.Add(new ImportMassageItem
                    {
                        Name = $"商品衝突－ItemNo：{itemNo}",
                        Description = $"相同 ItemNo 對應到不同商品名稱：{string.Join("；", importDetails)}。請確認 ItemNo 或商品名稱是否正確。",
                        Sheet = "商品",
                        RowNumbers = group.Select(e => e.SourceRowNumber).Distinct().OrderBy(e => e).ToList(),
                        ComparisonValues = group
                            .GroupBy(e => new { e.SourceRowNumber, e.ProdName })
                            .Select(e => new ImportMassageComparisonValue
                            {
                                RowNumber = e.Key.SourceRowNumber,
                                Label = "商品名稱",
                                Value = e.Key.ProdName ?? string.Empty
                            })
                            .OrderBy(e => e.RowNumber)
                            .ToList(),
                        CanIgnore = true
                    });
                }

                var subItemGroups = group
                    .Where(e => !string.IsNullOrWhiteSpace(e.SubItemNo))
                    .GroupBy(e => Norm(e.SubItemNo));
                foreach (var subItemGroup in subItemGroups)
                {
                    var specs = subItemGroup
                        .GroupBy(ProductImportSpecKey)
                        .Select(e => DescribeProductImportSpec(e.First()))
                        .ToList();
                    if (specs.Count <= 1)
                        continue;

                    warnings.Add(new ImportMassageItem
                    {
                        Name = $"規格編號衝突－ItemNo：{itemNo}／SubItemNo：{subItemGroup.First().SubItemNo}",
                        Description = $"相同 SubItemNo 對應到不同規格：{string.Join("；", specs)}。請確認規格內容或 SubItemNo 是否正確。",
                        Sheet = "商品",
                        RowNumbers = subItemGroup.Select(e => e.SourceRowNumber).Distinct().OrderBy(e => e).ToList(),
                        ComparisonValues = subItemGroup
                            .Select(e => new ImportMassageComparisonValue
                            {
                                RowNumber = e.SourceRowNumber,
                                Label = "規格",
                                Value = DescribeProductImportSpec(e)
                            })
                            .OrderBy(e => e.RowNumber)
                            .ToList(),
                        CanIgnore = true
                    });
                }

                var duplicateSpecs = group
                    .GroupBy(ProductImportSpecKey)
                    .Select(specGroup => new
                    {
                        Rows = specGroup,
                        DuplicatePriceKeys = specGroup
                            .GroupBy(ProductImportPriceKey)
                            .Where(priceGroup => priceGroup.Count() > 1)
                            .ToList()
                    })
                    .Where(e => e.DuplicatePriceKeys.Count > 0);
                foreach (var duplicateSpec in duplicateSpecs)
                {
                    var first = duplicateSpec.Rows.First();
                    var duplicateCount = duplicateSpec.DuplicatePriceKeys.Sum(e => e.Count() - 1);
                    warnings.Add(new ImportMassageItem
                    {
                        Name = $"規格重複－ItemNo：{itemNo}",
                        Description = $"規格「{DescribeProductImportSpec(first)}」有 {duplicateCount} 筆重複資料（相同會員身分與紅利條件）。請保留一筆或確認價格資料是否正確。",
                        Sheet = "商品",
                        RowNumbers = duplicateSpec.DuplicatePriceKeys
                            .SelectMany(e => e.Select(row => row.SourceRowNumber))
                            .Distinct().OrderBy(e => e).ToList(),
                        CanIgnore = true
                    });
                }
            }

            return warnings;
        }

        private static string ProductImportSpecKey(ProductImportDto product)
            => string.Join("|", new[]
            {
                Norm(product.Spec1Name),
                Norm(product.Spec1),
                Norm(product.Spec2Name),
                Norm(product.Spec2)
            });

        private static string ProductImportPriceKey(ProductImportDto product)
            => $"{Norm(product.RoleName)}|{product.Bonus}";

        private static string DescribeProductImportSpec(ProductImportDto product)
        {
            var specs = new[]
            {
                (Name: product.Spec1Name, Value: product.Spec1),
                (Name: product.Spec2Name, Value: product.Spec2)
            }
            .Where(e => !string.IsNullOrWhiteSpace(e.Name) || !string.IsNullOrWhiteSpace(e.Value))
            .Select(e => string.IsNullOrWhiteSpace(e.Name)
                ? (e.Value ?? string.Empty)
                : $"{e.Name}：{e.Value}")
            .ToList();

            return specs.Count == 0 ? "無規格" : string.Join("／", specs);
        }

        private static int CountDirectoryMenuRequests(List<DirectoryImportDto> directories)
            => GetDirectoryMenuRequests(directories).Count;

        private static List<(string Title, string RouterName)> GetDirectoryMenuRequests(
            List<DirectoryImportDto> directories)
        {
            var requests = new Dictionary<string, (string Title, string RouterName)>();

            void Add(string? title, string? routerName)
            {
                if (string.IsNullOrWhiteSpace(title))
                    return;

                var key = string.IsNullOrWhiteSpace(routerName)
                    ? $"title:{Norm(title)}"
                    : $"router:{Norm(routerName)}";
                requests[key] = (title.Trim(), (routerName ?? string.Empty).Trim());
            }

            foreach (var directory in directories)
            {
                Add(directory.Level1, directory.Level1RouterName);
                Add(directory.Level2, directory.Level2RouterName);
                Add(directory.Level3, directory.Level3RouterName);
            }

            return requests.Values.ToList();
        }

        private static List<(string Title, string RouterName)> GetLeafDirectoryMenuRequests(
            List<DirectoryImportDto> directories)
        {
            var requests = new Dictionary<string, (string Title, string RouterName)>();
            foreach (var row in directories)
            {
                var title = !string.IsNullOrWhiteSpace(row.Level3)
                    ? row.Level3
                    : !string.IsNullOrWhiteSpace(row.Level2) ? row.Level2 : row.Level1;
                var routerName = !string.IsNullOrWhiteSpace(row.Level3)
                    ? row.Level3RouterName
                    : !string.IsNullOrWhiteSpace(row.Level2) ? row.Level2RouterName : row.Level1RouterName;
                if (string.IsNullOrWhiteSpace(title)) continue;
                var key = string.IsNullOrWhiteSpace(routerName)
                    ? $"title:{Norm(title)}"
                    : $"router:{Norm(routerName)}";
                requests[key] = (title.Trim(), (routerName ?? string.Empty).Trim());
            }
            return requests.Values.ToList();
        }

        private async Task<(List<ImportMassageItem> Errors, HashSet<int> InvalidRowIndexes)> ValidateDirectoryImportStructureAsync(
            List<DirectoryImportDto> directories,
            long websiteId,
            bool allowDuplicateMenuTitles)
        {
            var errors = new List<ImportMassageItem>();
            var invalidRowIndexes = new HashSet<int>();
            var errorKeys = new HashSet<string>();
            var existingMenus = await db.WebMenus.AsNoTracking()
                .Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                .ToListAsync();
            var routerTitles = new Dictionary<string, string>();
            var parentByChild = new Dictionary<string, (
                string ParentKey,
                int RowIndex,
                int Level,
                string Title,
                string Router,
                string ExistingTitle,
                string ExistingRouter
            )>();
            var edges = new Dictionary<string, HashSet<string>>();
            var labels = new Dictionary<string, string>();

            void AddError(int rowIndex, string message)
            {
                var key = $"{rowIndex}:{message}";
                if (!errorKeys.Add(key))
                    return;

                if (rowIndex >= 0)
                    invalidRowIndexes.Add(rowIndex);

                // 目錄分頁的第一筆資料位於 Excel 第 4 列：
                // rowIndex = 0 代表 Excel 第 4 列。
                const int directoryFirstDataRow = 4;

                var excelRowNumber = rowIndex >= 0
                    ? rowIndex + directoryFirstDataRow
                    : 0;

                errors.Add(new ImportMassageItem
                {
                    Name = rowIndex >= 0
                        ? $"目錄分頁第 {excelRowNumber} 列"
                        : "目錄分類結構",

                    Description = rowIndex >= 0
                    ? $"{message} 這一列的目錄尚未建立。"
                    : message,
                    Sheet = "目錄分類",
                    RowNumbers = rowIndex >= 0
                        ? new List<int> { excelRowNumber }
                        : new List<int>(),
                    CanIgnore = rowIndex >= 0
                });
            }

            static bool IsRouterCharacterAllowed(char value)
                => char.IsLetterOrDigit(value) || value is '-' or '.' or '_' or '~';

            static string GetTitleColumn(int level)
            {
                return level switch
                {
                    1 => "A",
                    2 => "C",
                    3 => "E",
                    _ => ""
                };
            }

            static string GetRouterColumn(int level)
            {
                return level switch
                {
                    1 => "B",
                    2 => "D",
                    3 => "F",
                    _ => ""
                };
            }

            static int GetExcelRowNumber(int rowIndex)
            {
                const int directoryFirstDataRow = 4;
                return rowIndex + directoryFirstDataRow;
            }

            for (var rowIndex = 0; rowIndex < directories.Count; rowIndex++)
            {
                var row = directories[rowIndex];
                var levels = new[]
                {
                    (Level: 1, Title: (row.Level1 ?? string.Empty).Trim(), Router: (row.Level1RouterName ?? string.Empty).Trim()),
                    (Level: 2, Title: (row.Level2 ?? string.Empty).Trim(), Router: (row.Level2RouterName ?? string.Empty).Trim()),
                    (Level: 3, Title: (row.Level3 ?? string.Empty).Trim(), Router: (row.Level3RouterName ?? string.Empty).Trim())
                };

                if (levels[1].Title.Length > 0 && levels[0].Title.Length == 0)
                    AddError(rowIndex, "第二層選單有資料，但第一層選單為空白。");
                if (levels[2].Title.Length > 0 && levels[1].Title.Length == 0)
                    AddError(rowIndex, "第三層選單有資料，但第二層選單為空白。");

                var pathNodes = new List<(
                    string Key,
                    string Label,
                    int Level,
                    string Title,
                    string Router,
                    string ExistingTitle,
                    string ExistingRouter
                )>();
                var rowRouterTitles = new Dictionary<string, string>();
                foreach (var level in levels.Where(e => e.Title.Length > 0))
                {
                    if (level.Router.Length > 0)
                    {
                        var invalidCharacters = level.Router
                            .Where(value => !IsRouterCharacterAllowed(value))
                            .Distinct()
                            .Select(value => char.IsWhiteSpace(value) ? "空白" : $"「{value}」")
                            .ToList();
                        if (invalidCharacters.Count > 0)
                        {
                            AddError(rowIndex,
                                $"第 {level.Level} 層 RouterName「{level.Router}」包含網址路徑不允許的字元：{string.Join("、", invalidCharacters)}。只允許中英文字母、數字及 - . _ ~。");
                        }

                        var normalizedRouter = Norm(level.Router);
                        if ((routerTitles.TryGetValue(normalizedRouter, out var previousTitle)
                                || rowRouterTitles.TryGetValue(normalizedRouter, out previousTitle))
                            && Norm(previousTitle) != Norm(level.Title))
                        {
                            AddError(rowIndex,
                                $"第 {level.Level} 層 RouterName「{level.Router}」已被選單「{previousTitle}」使用，不能再指定給「{level.Title}」。");
                        }
                        else
                        {
                            rowRouterTitles[normalizedRouter] = level.Title;
                        }
                    }

                    var existing = FindMenuByRouterOrTitle(
                        existingMenus,
                        level.Title,
                        level.Router,
                        useTitleFallback: !allowDuplicateMenuTitles);
                    var key = existing != null
                        ? $"id:{existing.Id}"
                        : level.Router.Length > 0 ? $"router:{Norm(level.Router)}" : $"title:{Norm(level.Title)}";
                    var label = level.Router.Length > 0 ? $"{level.Title} ({level.Router})" : level.Title;
                    labels[key] = label;
                    pathNodes.Add((
                        Key: key,
                        Label: label,
                        Level: level.Level,
                        Title: level.Title,
                        Router: level.Router,
                        ExistingTitle: existing?.Title ?? string.Empty,
                        ExistingRouter: existing?.RouterName ?? string.Empty
                    ));
                }

                var duplicateNode = pathNodes.GroupBy(e => e.Key).FirstOrDefault(e => e.Count() > 1);
                if (duplicateNode != null)
                    AddError(rowIndex, $"選單「{duplicateNode.First().Label}」不能放在自己底下，請檢查各層 RouterName 是否重複。");

                var rowEdges = pathNodes.Skip(1)
                    .Select((child, index) => (
                        Parent: pathNodes[index],
                        Child: child
                    ))
                    .ToList();

                foreach (var edge in rowEdges)
                {
                    if (parentByChild.TryGetValue(
                            edge.Child.Key,
                            out var previousLocation)
                        && previousLocation.ParentKey != edge.Parent.Key)
                    {
                        var previousExcelRow =
                            GetExcelRowNumber(previousLocation.RowIndex);

                        var currentExcelRow =
                            GetExcelRowNumber(rowIndex);

                        var previousTitleCell =
                            $"{GetTitleColumn(previousLocation.Level)}{previousExcelRow}";

                        var currentTitleCell =
                            $"{GetTitleColumn(edge.Child.Level)}{currentExcelRow}";

                        var previousRouterCell =
                            $"{GetRouterColumn(previousLocation.Level)}{previousExcelRow}";

                        var currentRouterCell =
                            $"{GetRouterColumn(edge.Child.Level)}{currentExcelRow}";

                        if (edge.Child.Key.StartsWith("id:", StringComparison.Ordinal))
                        {
                            var existingTitle = string.IsNullOrEmpty(edge.Child.ExistingTitle)
                                ? previousLocation.ExistingTitle
                                : edge.Child.ExistingTitle;
                            var existingRouter = string.IsNullOrEmpty(edge.Child.ExistingRouter)
                                ? previousLocation.ExistingRouter
                                : edge.Child.ExistingRouter;
                            var existingMenu = string.IsNullOrEmpty(existingRouter)
                                ? $"「{existingTitle}」"
                                : $"「{existingTitle}」（RouterName：{existingRouter}）";
                            var previousParent = labels[previousLocation.ParentKey];
                            var currentParent = edge.Parent.Label;

                            AddError(
                                rowIndex,
                                $"「目錄分類」工作表第 {previousExcelRow} 列的 "
                                + $"{previousTitleCell}「{previousLocation.Title}」、"
                                + $"{previousRouterCell}「{previousLocation.Router}」，與第 {currentExcelRow} 列的 "
                                + $"{currentTitleCell}「{edge.Child.Title}」、"
                                + $"{currentRouterCell}「{edge.Child.Router}」內容雖不相同，"
                                + $"但匯入時都對應到後台現有選單{existingMenu}。"
                                + $"同一個選單不能同時放在「{previousParent}」與「{currentParent}」兩個分類中。"
                                + $"若要保留在「{previousParent}」，請刪除第 {currentExcelRow} 列；"
                                + $"若要搬移到「{currentParent}」，請刪除第 {previousExcelRow} 列；"
                                + $"若兩個分類都要保留，請將 {currentTitleCell} 的選單名稱及 "
                                + $"{currentRouterCell} 的 RouterName 都改成後台尚未使用的新值。");
                        }
                        else if (edge.Child.Key.StartsWith("router:", StringComparison.Ordinal))
                        {
                            AddError(
                                rowIndex,
                                $"「目錄分類」工作表的 {currentRouterCell} 與 "
                                + $"{previousRouterCell} 使用了相同的 RouterName"
                                + $"「{edge.Child.Router}」，但兩筆資料分別位於"
                                + $"「{labels[previousLocation.ParentKey]}」與"
                                + $"「{edge.Parent.Label}」兩個不同分類中。"
                                + $"若要保留前一個分類，請刪除第 {currentExcelRow} 列；"
                                + $"若要搬移到目前分類，請刪除第 {previousExcelRow} 列；"
                                + $"若兩個分類都要保留，請將 {currentTitleCell} 的選單名稱及 "
                                + $"{currentRouterCell} 的 RouterName 改成不重複的內容。");
                        }
                        else
                        {
                            AddError(
                                rowIndex,
                                $"「目錄分類」工作表的 {currentTitleCell} 與 "
                                + $"{previousTitleCell} 使用了相同的選單名稱"
                                + $"「{edge.Child.Title}」，但兩筆資料分別位於"
                                + $"「{labels[previousLocation.ParentKey]}」與"
                                + $"「{edge.Parent.Label}」兩個不同分類中。"
                                + $"若要保留前一個分類，請刪除第 {currentExcelRow} 列；"
                                + $"若要搬移到目前分類，請刪除第 {previousExcelRow} 列；"
                                + $"若兩個分類都要保留，請將 {currentTitleCell} 的選單名稱及 "
                                + $"{currentRouterCell} 的 RouterName 改成不重複的內容。");
                        }
                    }
                }

                if (invalidRowIndexes.Contains(rowIndex))
                    continue;

                foreach (var routerTitle in rowRouterTitles)
                    routerTitles[routerTitle.Key] = routerTitle.Value;
                for (var index = 1; index < pathNodes.Count; index++)
                {
                    var parent = pathNodes[index - 1];
                    var child = pathNodes[index];
                    parentByChild[child.Key] = (
                        ParentKey: parent.Key,
                        RowIndex: rowIndex,
                        Level: child.Level,
                        Title: child.Title,
                        Router: child.Router,
                        ExistingTitle: child.ExistingTitle,
                        ExistingRouter: child.ExistingRouter
                    );

                    if (!edges.TryGetValue(parent.Key, out var children))
                        edges[parent.Key] = children = new HashSet<string>();
                    children.Add(child.Key);
                }
            }

            var states = new Dictionary<string, int>();
            bool HasCycle(string node)
            {
                if (states.TryGetValue(node, out var state)) return state == 1;
                states[node] = 1;
                if (edges.TryGetValue(node, out var children) && children.Any(HasCycle)) return true;
                states[node] = 2;
                return false;
            }

            foreach (var node in edges.Keys)
            {
                if (HasCycle(node))
                {
                    invalidRowIndexes.UnionWith(Enumerable.Range(0, directories.Count));
                    AddError(-1, "多筆選單結構共同形成循環，為避免寫入錯誤父層，本次目錄分類已全部略過；商品資料仍會繼續匯入。");
                    break;
                }
            }

            return (errors, invalidRowIndexes);
        }

        private async Task<Html_Content> GetProductImportTemplate(long templateId, long websiteId)
        {
            if (templateId <= 0)
                throw new InvalidOperationException("請先選擇商品匯入版型。");

            var isSystemUser = await loginUserData.isSystemUser();
            var template = await db.Html_Contents
                .Include(e => e.HtmlContentPurposes)
                    .ThenInclude(e => e.ComponentPurpose)
                .FirstOrDefaultAsync(e =>
                    e.Id == templateId
                    && e.Disp_opt
                    && (isSystemUser || e.Type != (int)ObjectTypeEnum.自訂 || e.FK_WebsiteId == websiteId)
                    && e.HtmlContentPurposes.Any(p =>
                        p.ComponentPurpose.Visible
                        && p.ComponentPurpose.Code == "product-import-directory"));

            if (template == null)
                throw new InvalidOperationException("找不到可使用的商品匯入版型，請重新選擇。");
            if (string.IsNullOrWhiteSpace(template.Html))
                throw new InvalidOperationException("選擇的商品匯入版型沒有 HTML 內容。");

            return template;
        }
        private async Task<(int AddedCount, int UpdatedCount)> importProds(
            List<ProductImportDto> prods,
            List<ImportMassageItem> erroes,
            bool overwriteExistingProductNames,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices,
            bool overwriteExistingTechnicalCertificates,
            bool hasTechnicalCertificateRows,
            Action<int, string>? reportProgress)
        {
            reportProgress?.Invoke(35, "正在寫入商品、規格與價格");
            var counts = await InsertOrUpdateProd(
                prods,
                erroes,
                overwriteExistingProductNames,
                overwriteExistingSpecs,
                overwriteExistingPrices);
            var hasMediaColumns = HasAnyImportedProductColumn(
                prods,
                nameof(ProductImportDto.Image1), nameof(ProductImportDto.Image2),
                nameof(ProductImportDto.Image3), nameof(ProductImportDto.Image4),
                nameof(ProductImportDto.Image5), nameof(ProductImportDto.Image6),
                nameof(ProductImportDto.Image7), nameof(ProductImportDto.File1),
                nameof(ProductImportDto.File2), nameof(ProductImportDto.File3),
                nameof(ProductImportDto.File4), nameof(ProductImportDto.File5),
                nameof(ProductImportDto.File6), nameof(ProductImportDto.File7));
            reportProgress?.Invoke(55, hasMediaColumns
                ? "正在整理商品圖片與附件"
                : "Excel 未包含圖片與附件，已略過");
            if (hasMediaColumns)
                await ImportProdMediaLinks(prods, erroes);

            var hasTagColumns = HasAnyImportedProductColumn(
                prods,
                nameof(ProductImportDto.Tag1), nameof(ProductImportDto.Tag2),
                nameof(ProductImportDto.Tag3), nameof(ProductImportDto.Tag4),
                nameof(ProductImportDto.Tag5), nameof(ProductImportDto.Tag6));
            reportProgress?.Invoke(68, hasTagColumns
                ? "正在整理商品標籤"
                : "Excel 未包含商品標籤，已略過");
            if (hasTagColumns)
                await ImportProdTags(prods, erroes);

            reportProgress?.Invoke(78, hasTechnicalCertificateRows
                ? "正在整理商品技術證照"
                : "Excel 未包含技術證照資料，已略過");
            if (hasTechnicalCertificateRows)
                await importTechs(prods, erroes, overwriteExistingTechnicalCertificates);
            return counts;
        }

        private static bool HasAnyImportedProductColumn(
            IEnumerable<ProductImportDto> products,
            params string[] columnNames)
            => products.Any(product => product.ImportedColumns.Count == 0
                || columnNames.Any(product.ImportedColumns.Contains));
        private async Task imporDirectories(
            List<DirectoryImportDto> directories,
            Html_Content importTemplate,
            bool overwriteExisting,
            bool allowDuplicateMenuTitles,
            bool overwriteExistingMenuParents)
        {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                List<string> tagNames = new List<string>();
                tagNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Tag1)).Select(e => (e.Tag1 ?? "").Trim()).ToList());
                tagNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Tag2)).Select(e => (e.Tag2 ?? "").Trim()).ToList());
                tagNames.AddRange(directories.Where(e => !string.IsNullOrEmpty(e.Tag3)).Select(e => (e.Tag3 ?? "").Trim()).ToList());

                var menuRequests = new List<(string Title, string RouterName)>();
                void AddMenuRequest(string? title, string? routerName)
                {
                    var normalizedTitle = (title ?? "").Trim();
                    var normalizedRouter = (routerName ?? "").Trim();
                    if (string.IsNullOrEmpty(normalizedTitle)) return;
                    if (!menuRequests.Any(e =>
                        (!string.IsNullOrEmpty(normalizedRouter) && Norm(e.RouterName) == Norm(normalizedRouter))
                        || (string.IsNullOrEmpty(normalizedRouter) && Norm(e.Title) == Norm(normalizedTitle))))
                    {
                        menuRequests.Add((normalizedTitle, normalizedRouter));
                    }
                }

                foreach (var directory in directories)
                {
                    AddMenuRequest(directory.Level1, directory.Level1RouterName);
                    AddMenuRequest(directory.Level2, directory.Level2RouterName);
                    AddMenuRequest(directory.Level3, directory.Level3RouterName);
                }

                var existingMenus = await db.WebMenus
                    .Where(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID)
                    .ToListAsync();
                var existingMenuIds = existingMenus.Select(e => e.Id).ToHashSet();
                var missingMenuRequests = menuRequests
                    .Where(e => FindMenuByRouterOrTitle(
                        existingMenus,
                        e.Title,
                        e.RouterName,
                        useTitleFallback: !allowDuplicateMenuTitles) == null)
                    .ToList();

                await importMenus(WebsiteID, missingMenuRequests);
                await importTags(WebsiteID, tagNames);

                var menus = await db.WebMenus
                    .Where(e => !e.IsDeleted && e.FK_WebsiteId == WebsiteID)
                    .ToListAsync();

                foreach (var request in menuRequests.Where(e => !string.IsNullOrEmpty(e.RouterName)))
                {
                    var menu = FindMenuByRouterOrTitle(
                        menus,
                        request.Title,
                        request.RouterName,
                        useTitleFallback: !allowDuplicateMenuTitles);
                    var routerIsUsed = menus.Any(e => e.Id != menu?.Id && Norm(e.RouterName) == Norm(request.RouterName));
                    if (menu != null && !routerIsUsed)
                        menu.RouterName = request.RouterName;
                }
                await db.SaveChangesAsync();

                var Tags = await db.Tags.Where(e => !e.IsDeleted)
                               .Where(e => !string.IsNullOrEmpty(e.Title) && tagNames.Contains(e.Title))
                               .Where(e => e.FK_WebsiteId == WebsiteID).ToListAsync();

                List<DirectoryArrangeImportDto> menuMap = new List<DirectoryArrangeImportDto>();
                for (int i = 0; i < directories.Count; i++)
                {
                    var directory = directories[i];
                    DirectoryArrangeImportDto? item = menuMap.Find(e =>
                        !string.IsNullOrEmpty(directory.Level1RouterName)
                            ? Norm(e.RouterName) == Norm(directory.Level1RouterName)
                            : Norm(e.Name) == Norm(directory.Level1));
                    if (string.IsNullOrEmpty(directory.Level1)) break;

                    var menu = FindMenuByRouterOrTitle(
                        menus,
                        directory.Level1,
                        directory.Level1RouterName,
                        useTitleFallback: !allowDuplicateMenuTitles);
                    if (menu == null) break;

                    if (item == null)
                    {
                        item = new DirectoryArrangeImportDto
                        {
                            Id = menu.Id,
                            Name = menu.Title ?? directory.Level1,
                            RouterName = menu.RouterName
                        };
                        menuMap.Add(item);
                    }
                    else item.Id = menu.Id;

                    if (string.IsNullOrEmpty(directory.Level2)) continue;
                    var menu2 = FindMenuByRouterOrTitle(
                        menus,
                        directory.Level2,
                        directory.Level2RouterName,
                        useTitleFallback: !allowDuplicateMenuTitles);
                    if (menu2 != null)
                    {
                        if (overwriteExistingMenuParents || !existingMenuIds.Contains(menu2.Id))
                        {
                            menu2.FK_TopNodeId = menu.Id;
                            menu2.FK_RootNodeId = menu.Id;
                        }
                        DirectoryArrangeImportDto? item2 = item.Child.Find(e =>
                            !string.IsNullOrEmpty(directory.Level2RouterName)
                                ? Norm(e.RouterName) == Norm(directory.Level2RouterName)
                                : Norm(e.Name) == Norm(directory.Level2));
                        if (item2 == null)
                        {
                            item2 = new DirectoryArrangeImportDto
                            {
                                Id = menu2.Id,
                                Name = menu2.Title ?? directory.Level2,
                                RouterName = menu2.RouterName
                            };
                            item.Child.Add(item2);

                            if (string.IsNullOrEmpty(directory.Level3))
                            {
                                await addDirectoryToTags(directory, item2, Tags);
                            }
                            else
                            {
                                var menu3 = FindMenuByRouterOrTitle(
                                    menus,
                                    directory.Level3,
                                    directory.Level3RouterName,
                                    useTitleFallback: !allowDuplicateMenuTitles);
                                if (menu3 != null)
                                {
                                    if (overwriteExistingMenuParents || !existingMenuIds.Contains(menu3.Id))
                                    {
                                        menu3.FK_TopNodeId = menu2.Id;
                                        menu3.FK_RootNodeId = menu.Id;
                                    }
                                    DirectoryArrangeImportDto? item3 = item2.Child.Find(e =>
                                        !string.IsNullOrEmpty(directory.Level3RouterName)
                                            ? Norm(e.RouterName) == Norm(directory.Level3RouterName)
                                            : Norm(e.Name) == Norm(directory.Level3));
                                    if (item3 == null)
                                    {
                                        item3 = new DirectoryArrangeImportDto
                                        {
                                            Id = menu3.Id,
                                            Name = menu3.Title ?? directory.Level3,
                                            RouterName = menu3.RouterName
                                        };
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
                                var menu3 = FindMenuByRouterOrTitle(
                                    menus,
                                    directory.Level3,
                                    directory.Level3RouterName,
                                    useTitleFallback: !allowDuplicateMenuTitles);
                                if (menu3 != null)
                                {
                                    if (overwriteExistingMenuParents || !existingMenuIds.Contains(menu3.Id))
                                    {
                                        menu3.FK_TopNodeId = menu2.Id;
                                        menu3.FK_RootNodeId = menu.Id;
                                    }
                                    DirectoryArrangeImportDto? item3 = item2.Child.Find(e =>
                                        !string.IsNullOrEmpty(directory.Level3RouterName)
                                            ? Norm(e.RouterName) == Norm(directory.Level3RouterName)
                                            : Norm(e.Name) == Norm(directory.Level3));
                                    if (item3 == null)
                                    {
                                        item3 = new DirectoryArrangeImportDto
                                        {
                                            Id = menu3.Id,
                                            Name = menu3.Title ?? directory.Level3,
                                            RouterName = menu3.RouterName
                                        };
                                    }
                                    item2.Child.Add(item3);
                                    await addDirectoryToTags(directory, item3, Tags);
                                }
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();
                await createDirectory(menuMap, importTemplate, overwriteExisting);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"商品目錄匯入失敗：{e.Message}", e);
            }
        }
        private bool TemplateHasMenuDirectory(Html_Content importTemplate)
        {
            var html = stringHandler.HtmlDecode(importTemplate.Html ?? string.Empty);
            var document = htmlProcessor.LoadHtml(html);
            return document.DocumentNode.SelectSingleNode("//*[@data-import-role='menu-directory']") != null
                || document.DocumentNode.SelectSingleNode("//*[contains(concat(' ', normalize-space(@class), ' '), ' menu_directory ')]") != null;
        }

        private async Task<Core.Models.Directory> GetOrCreateMenuDirectory(
            long websiteId,
            DirectoryArrangeImportDto rootMenu)
        {
            var directory = await db.Directory.FirstOrDefaultAsync(e =>
                !e.IsDeleted
                && e.FK_WebsiteId == websiteId
                && e.Type == (int)DirectoryTypeEnum.選單
                && (e.FK_Mid == rootMenu.Id || (e.FK_Mid == null && e.Title == rootMenu.Name)));

            if (directory != null)
            {
                if (directory.FK_Mid == null)
                {
                    directory.FK_Mid = rootMenu.Id;
                    await loginUserData.setOptionParameter(directory);
                }
                return directory;
            }

            directory = new Core.Models.Directory
            {
                FK_WebsiteId = websiteId,
                FK_Mid = rootMenu.Id,
                Title = rootMenu.Name,
                Type = (int)DirectoryTypeEnum.選單,
                Visible = true
            };
            db.Directory.Add(directory);
            await loginUserData.SaveChanges(directory);
            return directory;
        }

        private string BuildProductImportMenuHtml(
            Html_Content importTemplate,
            Core.Models.Directory? menuDirectory,
            Core.Models.Directory productDirectory,
            string pageTitle)
        {
            var html = stringHandler.HtmlDecode(importTemplate.Html ?? string.Empty);
            var document = htmlProcessor.LoadHtml(html);

            var menuNode = document.DocumentNode.SelectSingleNode("//*[@data-import-role='menu-directory']")
                ?? document.DocumentNode.SelectSingleNode("//*[contains(concat(' ', normalize-space(@class), ' '), ' menu_directory ')]");
            var productNode = document.DocumentNode.SelectSingleNode("//*[@data-import-role='product-directory']")
                ?? document.DocumentNode.SelectSingleNode("//*[contains(concat(' ', normalize-space(@class), ' '), ' catalog_frame ')]");
            var titleNode = document.DocumentNode.SelectSingleNode("//*[@data-edit-key='pageTitle']")
                ?? document.DocumentNode.SelectSingleNode("//*[@data-import-role='page-title']");

            if (productNode == null)
                throw new InvalidOperationException("版型缺少商品目錄標記 data-import-role=\"product-directory\"。");
            if (titleNode == null)
                throw new InvalidOperationException("版型缺少標題標記 data-edit-key=\"pageTitle\"。");
            if (menuNode != null && menuDirectory == null)
                throw new InvalidOperationException("版型包含選單目錄，但無法建立對應的選單目錄。");

            if (menuNode != null && menuDirectory != null)
            {
                menuNode.SetAttributeValue("data-dirid", menuDirectory.Id.ToString());
                menuNode.SetAttributeValue("data-diridname", menuDirectory.Title ?? string.Empty);
            }

            productNode.SetAttributeValue("data-dirid", productDirectory.Id.ToString());
            productNode.SetAttributeValue("data-diridname", productDirectory.Title ?? string.Empty);

            var titleValueNode = titleNode.SelectSingleNode(".//span") ?? titleNode;
            titleValueNode.InnerHtml = HttpUtility.HtmlEncode(pageTitle);

            return stringHandler.HtmlEncode(document.DocumentNode.OuterHtml);
        }

        private string UpdateProductImportDirectoryBindings(
            string? storedHtml,
            Core.Models.Directory? menuDirectory,
            Core.Models.Directory productDirectory)
        {
            if (string.IsNullOrWhiteSpace(storedHtml))
                return storedHtml ?? string.Empty;

            var html = stringHandler.HtmlDecode(storedHtml);
            var document = htmlProcessor.LoadHtml(html);
            var changed = false;

            var menuNode = document.DocumentNode.SelectSingleNode("//*[@data-import-role='menu-directory']")
                ?? document.DocumentNode.SelectSingleNode("//*[contains(concat(' ', normalize-space(@class), ' '), ' menu_directory ')]");
            var productNode = document.DocumentNode.SelectSingleNode("//*[@data-import-role='product-directory']")
                ?? document.DocumentNode.SelectSingleNode("//*[contains(concat(' ', normalize-space(@class), ' '), ' catalog_frame ')]");

            if (menuNode != null && menuDirectory != null)
                changed |= SetDirectoryBinding(menuNode, menuDirectory);
            if (productNode != null)
                changed |= SetDirectoryBinding(productNode, productDirectory);

            return changed
                ? stringHandler.HtmlEncode(document.DocumentNode.OuterHtml)
                : storedHtml;

            static bool SetDirectoryBinding(HtmlAgilityPack.HtmlNode node, Core.Models.Directory directory)
            {
                var directoryId = directory.Id.ToString();
                var directoryName = directory.Title ?? string.Empty;
                var idChanged = !string.Equals(
                    node.GetAttributeValue("data-dirid", string.Empty),
                    directoryId,
                    StringComparison.Ordinal);
                var nameChanged = !string.Equals(
                    node.GetAttributeValue("data-diridname", string.Empty),
                    directoryName,
                    StringComparison.Ordinal);

                if (idChanged)
                    node.SetAttributeValue("data-dirid", directoryId);
                if (nameChanged)
                    node.SetAttributeValue("data-diridname", directoryName);

                return idChanged || nameChanged;
            }
        }

        private async Task createDirectory(
            List<DirectoryArrangeImportDto> menuMap,
            Html_Content importTemplate,
            bool overwriteExisting,
            Core.Models.Directory? menuDirectory = null,
            bool? hasMenuDirectory = null)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            long UserID = await loginUserData.GetUserId();
            hasMenuDirectory ??= TemplateHasMenuDirectory(importTemplate);
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
                    .Where(e => e.Type == TagAssociateTypeEnum.目錄)
                    .Where(t => t.Tag != null && t.Tag.FK_WebsiteId == WebsiteID).ToListAsync();
            for (int i = 0; i < menuMap.Count; i++)
            {
                var menu = menuMap[i];
                var currentMenuDirectory = menuDirectory;
                if (hasMenuDirectory == true && currentMenuDirectory == null)
                    currentMenuDirectory = await GetOrCreateMenuDirectory(WebsiteID, menu);

                if (menu.Child.Any())
                    await createDirectory(menu.Child, importTemplate, overwriteExisting, currentMenuDirectory, hasMenuDirectory);
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
                                    Type = TagAssociateTypeEnum.目錄
                                };
                                loginUserData.setOptionParameter(associate, UserID);
                                associates.Add(associate);
                            }
                        });
                        var oldTagBind = TagAssociate.FindAll(e => e.FK_AId == dir.Id && !tagIds.Contains(e.FK_TId)).ToList();
                        for (int j = 0; j < oldTagBind.Count(); j++)
                        {
                            oldTagBind[j].IsDeleted = true;
                            await loginUserData.setOptionParameter(oldTagBind[j]);
                        }
                    }
                    var myMenu = webMenu.FirstOrDefault(e => e.Id == menu.Id);
                    if (myMenu != null && !string.IsNullOrEmpty(dir.Title))
                    {
                        if (overwriteExisting || string.IsNullOrEmpty(myMenu.SaveHtml))
                        {
                            myMenu.Html = BuildProductImportMenuHtml(
                                importTemplate,
                                currentMenuDirectory,
                                dir,
                                myMenu.Title ?? dir.Title);
                            myMenu.PageText = htmlProcessor.text(stringHandler.HtmlDecode(myMenu.Html));
                            myMenu.SaveHtml = myMenu.Html;
                            myMenu.Css = importTemplate.Css ?? string.Empty;
                            myMenu.SaveCss = myMenu.Css;
                        }
                        else
                        {
                            myMenu.Html = UpdateProductImportDirectoryBindings(
                                myMenu.Html,
                                currentMenuDirectory,
                                dir);
                            myMenu.SaveHtml = UpdateProductImportDirectoryBindings(
                                myMenu.SaveHtml,
                                currentMenuDirectory,
                                dir);
                        }

                        var sanitizedMenu = await htmlSanitizeService.EnsurePublicContentAsync(new HtmlSanitizeInput
                        {
                            WebsiteId = myMenu.FK_WebsiteId,
                            SourceType = HtmlSanitizeSourceType.選單,
                            SourceId = myMenu.Id,
                            ContentKey = "Published",
                            SanitizePolicy = "PublicHtml",
                            Html = stringHandler.HtmlDecode(myMenu.Html ?? ""),
                            Css = myMenu.Css ?? "",
                            Force = true
                        });

                        myMenu.Html = stringHandler.HtmlEncode(sanitizedMenu.Html);
                        myMenu.Css = sanitizedMenu.Css;
                        myMenu.PageText = htmlProcessor.text(sanitizedMenu.Html);
                    }
                }
            }
            ;
            db.Tag_Associates.AddRange(associates);
            await db.SaveChangesAsync();
            if (associates.Count > 0 || TagAssociate.Any(x => x.IsDeleted))
            {
                await websiteCacheStateAppService.TouchByWebsiteIdAsync(
                    WebsiteID,
                    WebsiteCacheKeys.DirectoryContent);
            }
        }
        private async Task addDirectoryToTags(DirectoryImportDto directory, DirectoryArrangeImportDto item, List<Core.Models.Tag> Tags)
        {
            item.Tags = new List<TagGetSelectedDto>();
            if (!string.IsNullOrEmpty(directory.Tag1))
            {
                var tag1 = Tags.FirstOrDefault(e => Norm(e.Title) == Norm(directory.Tag1));
                if (tag1 != null) item.Tags.Add(new TagGetSelectedDto { Id = tag1.Id, Tag_Name = tag1.Title });
            }
            if (!string.IsNullOrEmpty(directory.Tag2))
            {
                var tag2 = Tags.FirstOrDefault(e => Norm(e.Title) == Norm(directory.Tag2));
                if (tag2 != null) item.Tags.Add(new TagGetSelectedDto { Id = tag2.Id, Tag_Name = tag2.Title });
            }
            if (!string.IsNullOrEmpty(directory.Tag3))
            {
                var tag3 = Tags.FirstOrDefault(e => Norm(e.Title) == Norm(directory.Tag3));
                if (tag3 != null) item.Tags.Add(new TagGetSelectedDto { Id = tag3.Id, Tag_Name = tag3.Title });
            }
        }
        private async Task importMenus(
            long WebsiteID,
            List<(string Title, string RouterName)> menuRequests)
        {
            if (menuRequests.Count == 0) return;

            var userId = await loginUserData.GetUserId();
            var newMenus = menuRequests.Select(request => new WebMenu
            {
                Title = request.Title,
                RouterName = string.IsNullOrEmpty(request.RouterName)
                    ? request.Title
                    : request.RouterName,
                Visible = true,
                SerNO = 500,
                Popular = 0,
                PageType = PageTypeEnum.一般頁面,
                icon = "empty",
                PopularVisible = false,
                LanBar = false,
                FK_WebsiteId = WebsiteID,
                CreationTime = DateTime.Now,
                CreatorUserId = userId,
                IsDeleted = false,
                VisibleFooter = true,
                VisibleHeader = true,
                VisibleTitle = true,
                ShowToMenu = true,
                RemovedFromShelves = false
            }).ToList();

            db.WebMenus.AddRange(newMenus);
            await db.SaveChangesAsync();
            await websiteCacheStateAppService.TouchAsync(WebsiteCacheKeys.Menu);
        }

        private static WebMenu? FindMenuByRouterOrTitle(
            IReadOnlyList<WebMenu> menus,
            string? title,
            string? routerName,
            bool useTitleFallback = true)
        {
            var normalizedRouter = Norm(routerName);
            if (!string.IsNullOrEmpty(normalizedRouter))
            {
                var byRouter = menus.FirstOrDefault(e => Norm(e.RouterName) == normalizedRouter);
                if (byRouter != null) return byRouter;
                if (!useTitleFallback) return null;
            }

            var normalizedTitle = Norm(title);
            return string.IsNullOrEmpty(normalizedTitle)
                ? null
                : menus.FirstOrDefault(e => Norm(e.Title) == normalizedTitle);
        }

        private async Task importTags(long WebsiteID, List<string> tagNames)
        {
            long userId = await loginUserData.GetUserId();
            tagNames = tagNames
                .Select(CustomDtoMapper.Normalize)
                .Where(e => !string.IsNullOrEmpty(e))
                .GroupBy(Norm)
                .Select(e => e.First())
                .ToList();
            var tags = await db.Tags.Where(e => !e.IsDeleted)
               .Where(e => e.FK_WebsiteId == WebsiteID)
               .Where(e => !string.IsNullOrEmpty(e.Title))
               .ToListAsync();
            var hasTagsTitle = tags.Select(e => Norm(e.Title)).ToHashSet();
            var needAddTagss = tagNames.Where(e => !hasTagsTitle.Contains(Norm(e))).ToList();
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
        private async Task importTechs(
            List<ProductImportDto> prods,
            List<ImportMassageItem> errors,
            bool overwriteExistingTechnicalCertificates)
        {
            List<TechCertDto> allTech = new List<TechCertDto>();
            for (int i = 0; i < prods.Count; i++)
            {
                var prod = prods[i];
                if (prod.Techs != null) allTech.AddRange(prod.Techs);
            }
            if (!overwriteExistingTechnicalCertificates && allTech.Count > 0)
            {
                var existingTitles = await db.TechnicalCertificates.AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .Select(e => e.Title)
                    .ToListAsync();
                var existingTitleKeys = existingTitles.Select(Norm).ToHashSet();
                allTech = allTech
                    .Where(e => !existingTitleKeys.Contains(Norm(e.Title)))
                    .ToList();
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
            TagStr = TagStr
                .Select(CustomDtoMapper.Normalize)
                .Where(e => !string.IsNullOrEmpty(e))
                .GroupBy(Norm)
                .Select(e => (string?)e.First())
                .ToList();

            HashSet<string> nowTags = db.Tags.Where(e => e.FK_WebsiteId == WebsiteId)
                                    .Where(e => !e.IsDeleted)
                                    .Select(e => e.Title).ToList()
                                    .Select(Norm).ToHashSet();

            TagStr = TagStr.FindAll(e => !nowTags.Contains(Norm(e)));
            List<Core.Models.Tag> addTads = new List<Core.Models.Tag>();
            for (int i = 0; i < TagStr.Count; i++)
            {
                string? title = CustomDtoMapper.Normalize(TagStr[i]);
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
                var el = allProd.Find(e => item.Id != 0 && e.Id == item.Id)
                    ?? allProd.Find(e => Norm(e.Title) == Norm(item.ProdName) && Norm(e.ItemNo) == Norm(item.ItemNo));
                if (el == null)
                {
                    errors.Add(new ImportMassageItem
                    {
                        Name = item.ProdName,
                        Description = "商品標籤榜定失敗。"
                    });
                    continue;
                }
                item.Id = el.Id;
                var tag = nowTags.FindAll(e =>
                    !string.IsNullOrEmpty(e.Title) &&
                    new List<string?> { item.Tag1, item.Tag2, item.Tag3, item.Tag4, item.Tag5, item.Tag6 }
                        .Any(tagTitle => Norm(tagTitle) == Norm(e.Title))
                );
                if (tag != null)
                {
                    for (int j = 0; j < tag.Count; j++)
                    {
                        TagAssociates.Add(new TagAssociateDto
                        {
                            Type = TagAssociateTypeEnum.商品,
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
                var myProd = prod.Id != 0
                    ? db.Prods.Local.FirstOrDefault(e => e.Id == prod.Id)
                        ?? db.Prods.FirstOrDefault(e => e.Id == prod.Id)
                    : fileProds.FirstOrDefault(e => e.Title == prod.ProdName && e.ItemNo == prod.ItemNo);
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
            List<string?> ProdStr = prods.Where(e => !string.IsNullOrEmpty(e.ProdName)).Select(e => e.ProdName).ToList();
            List<FileImageImportDto> importDtos = new List<FileImageImportDto>();
            var fileProds = db.Prods.Where(e => !e.IsDeleted).Where(e => ProdStr.Contains(e.Title)).ToList();
            foreach (var prod in prods)
            {
                var myProd = prod.Id != 0
                    ? db.Prods.Local.FirstOrDefault(e => e.Id == prod.Id)
                        ?? db.Prods.FirstOrDefault(e => e.Id == prod.Id)
                    : fileProds.FirstOrDefault(e => e.Title == prod.ProdName && e.ItemNo == prod.ItemNo);
                if (myProd != null)
                {
                    string?[] fileLinks = { prod.File1, prod.File2, prod.File3, prod.File4, prod.File5, prod.File6, prod.File7 };
                    string?[] fileNames = { prod.FileName1, prod.FileName2, prod.FileName3, prod.FileName4, prod.FileName5, prod.FileName6, prod.FileName7 };
                    for (int i = 0; i < fileLinks.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(fileLinks[i]))
                        {
                            importDtos.Add(new FileImageImportDto
                            {
                                SId = myProd.Id,
                                Type = FileBindTypeEnum.產品檔案,
                                Name = fileNames[i] ?? "",
                                mediaLink = fileLinks[i] ?? "",
                                SerNo = 500
                            });
                        }
                    }
                }
            }
            await fileUploadAppService.uploadImageLink(importDtos);
        }
        private async Task<(int AddedCount, int UpdatedCount)> InsertOrUpdateProd(
            List<ProductImportDto> prods,
            List<ImportMassageItem> errors,
            bool overwriteExistingProductNames,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices)
        {
            List<ProductImportDto> AddProds = prods.FindAll(e => e.Id == 0);
            List<ProductImportDto> Prods = prods.FindAll(e => e.Id != 0);
            await InsetProdSpecTypes(prods, overwriteExistingSpecs);
            await InsetProdSpec(prods, overwriteExistingSpecs);

            var upsertResult = await UpsertProducts(prods, errors, overwriteExistingProductNames);
            var products = upsertResult.Products;
            await UpsertStocksAndPricesBatchAsync(products, prods, errors, overwriteExistingSpecs, overwriteExistingPrices);
            await db.SaveChangesAsync();

            foreach (var product in products)
            {
                var source = prods.FirstOrDefault(e => e.Id == product.Id)
                    ?? prods.FirstOrDefault(e => Norm(e.ItemNo) == Norm(product.ItemNo)
                        && Norm(e.ProdName) == Norm(product.Title));
                if (source != null
                    && source.Id != 0
                    && !HasImportedColumn(source, nameof(source.SaveHtml))
                    && !HasImportedColumn(source, nameof(source.Html))
                    && !HasImportedColumn(source, nameof(source.SaveCss)))
                    continue;

                var sanitized = await SanitizeProductPublishedContentAsync(
                    product.FK_WebsiteId,
                    product.Id,
                    stringHandler.HtmlDecode(product.Html ?? ""),
                    product.Css ?? "",
                    true
                );

                product.Html = stringHandler.HtmlEncode(sanitized.Html);
                product.Css = sanitized.Css;
                product.PageText = htmlProcessor.text(sanitized.Html);
            }

            await db.SaveChangesAsync();
            return (upsertResult.AddedCount, upsertResult.UpdatedCount);
        }
        private async Task<(List<Prod> Products, int AddedCount, int UpdatedCount)> UpsertProducts(
            List<ProductImportDto> dtos,
            List<ImportMassageItem> errors,
            bool overwriteExistingProductNames)
        {
            long userId = await loginUserData.GetUserId();
            string orgName = await loginUserData.GetWebsiteOrgName();
            var results = new List<Prod>();
            var addedCount = 0;
            var updatedCount = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    Prod prod;
                    if (dto.Id == 0) // 新增
                    {
                        prod = mapper.Map<Prod>(dto);
                        prod.CreatorUserId = userId;
                        // Excel 未填寫時，新商品預設為上架、顯示。
                        prod.RemovedFromShelves = false;
                        prod.Visible = true;
                        db.Prods.Add(prod);
                    }
                    else // 更新
                    {
                        prod = await db.Prods.FirstAsync(p => p.Id == dto.Id);
                        var originalTitle = prod.Title;
                        var originalItemNo = prod.ItemNo;
                        var originalDescription = prod.Description;
                        var originalIntroduction = prod.Introduction;
                        var originalStatus = prod.Status;
                        var originalStartTime = prod.StartTime;
                        var originalEndTime = prod.EndTime;
                        var originalPermanent = prod.permanent;
                        var originalVisible = prod.Visible;
                        var originalRemovedFromShelves = prod.RemovedFromShelves;
                        var originalHtml = prod.Html;
                        var originalSaveHtml = prod.SaveHtml;
                        var originalPageText = prod.PageText;
                        var originalCss = prod.Css;
                        var originalSaveCss = prod.SaveCss;
                        mapper.Map(dto, prod);
                        if (!overwriteExistingProductNames
                            || !HasImportedColumn(dto, nameof(dto.ProdName)))
                            prod.Title = originalTitle;
                        if (!HasImportedColumn(dto, nameof(dto.ItemNo))) prod.ItemNo = originalItemNo;
                        if (!HasImportedColumn(dto, nameof(dto.Description))) prod.Description = originalDescription;
                        if (!HasImportedColumn(dto, nameof(dto.Introduction))) prod.Introduction = originalIntroduction;
                        if (!HasImportedColumn(dto, nameof(dto.Status))) prod.Status = originalStatus;
                        if (!HasImportedColumn(dto, nameof(dto.StartTime))) prod.StartTime = originalStartTime;
                        if (!HasImportedColumn(dto, nameof(dto.EndTime))) prod.EndTime = originalEndTime;
                        if (!HasImportedColumn(dto, nameof(dto.StartTime))
                            && !HasImportedColumn(dto, nameof(dto.EndTime))) prod.permanent = originalPermanent;
                        if (!HasImportedColumn(dto, nameof(dto.Visible))) prod.Visible = originalVisible;
                        if (!HasImportedColumn(dto, nameof(dto.OnShelf))) prod.RemovedFromShelves = originalRemovedFromShelves;
                        if (!HasImportedColumn(dto, nameof(dto.SaveHtml))
                            && !HasImportedColumn(dto, nameof(dto.Html)))
                        {
                            prod.Html = originalHtml;
                            prod.SaveHtml = originalSaveHtml;
                            prod.PageText = originalPageText;
                        }
                        if (!HasImportedColumn(dto, nameof(dto.SaveCss)))
                        {
                            prod.Css = originalCss;
                            prod.SaveCss = originalSaveCss;
                        }
                        prod.LastModifierUserId = userId;
                        prod.LastModificationTime = DateTime.Now;
                    }

                    // Insert/Update 共用的邏輯
                    ApplyProductDisplaySettings(dto, prod, errors);

                    ApplyImportedProductContent(dto, prod, orgName);
                    if (dto.Id == 0 || HasImportedColumn(dto, nameof(dto.Status)))
                    {
                        if (Enum.TryParse(dto.Status, out ProdStatusEnum statusType))
                            prod.Status = statusType;
                        else
                            prod.Status = 0;
                    }

                    results.Add(prod);
                    if (dto.Id == 0)
                        addedCount++;
                    else
                        updatedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportMassageItem { Name = dto.ProdName, Description = ex.Message });
                }
            }

            return (results, addedCount, updatedCount);
        }

        private static void ApplyProductDisplaySettings(
            ProductImportDto dto,
            Prod prod,
            List<ImportMassageItem> errors)
        {
            var isNew = dto.Id == 0;
            var hasStartTime = isNew || HasImportedColumn(dto, nameof(dto.StartTime));
            var hasEndTime = isNew || HasImportedColumn(dto, nameof(dto.EndTime));
            if (hasStartTime) prod.StartTime = dto.StartTime;
            if (hasEndTime) prod.EndTime = dto.EndTime;
            if (hasStartTime || hasEndTime)
            {
                prod.permanent = !prod.StartTime.HasValue || !prod.EndTime.HasValue;
                if (prod.permanent)
                {
                    prod.StartTime = null;
                    prod.EndTime = null;
                }
            }

            if (isNew || HasImportedColumn(dto, nameof(dto.Visible)))
                ApplyImportFlag(dto.Visible, value => prod.Visible = value, dto.ProdName, "顯示", errors);

            if (isNew || HasImportedColumn(dto, nameof(dto.OnShelf)))
                ApplyImportFlag(dto.OnShelf, value => prod.RemovedFromShelves = !value, dto.ProdName, "上架", errors);
        }

        private static bool HasImportedColumn(ProductImportDto dto, string columnName)
            => dto.ImportedColumns.Count == 0 || dto.ImportedColumns.Contains(columnName);

        private static void ApplyImportFlag(
            string? rawValue,
            Action<bool> apply,
            string productName,
            string fieldName,
            List<ImportMassageItem> errors)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return;

            var value = ParseImportFlag(rawValue);
            if (value.HasValue)
            {
                apply(value.Value);
                return;
            }

            errors.Add(new ImportMassageItem
            {
                Name = productName,
                Description = $"{fieldName}欄位請輸入「是」或「否」，目前值為：{rawValue}"
            });
        }

        private static bool? ParseImportFlag(string? value)
        {
            return Norm(value) switch
            {
                "是" or "TRUE" or "1" or "YES" or "Y" or "顯示" or "上架" => true,
                "否" or "FALSE" or "0" or "NO" or "N" or "隱藏" or "下架" => false,
                _ => null
            };
        }

        private string NormalizeHtml(string? rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml))
                return "";

            string html = rawHtml.Trim();

            // 已經是 container 包起來的，就直接回傳
            if (html.StartsWith("<div class=\"container\">") && html.EndsWith("</div>"))
            {
                return html;
            }

            // 統一換行處理
            html = html.Replace(Environment.NewLine, "<br />")
                       .Replace("\n", "<br />");

            // 外層補一個 container
            return $"<div class=\"container\">{html}</div>";
        }

        private void ApplyImportedProductContent(ProductImportDto dto, Prod prod, string orgName)
        {
            var isNew = dto.Id == 0;
            var hasHtml = HasImportedColumn(dto, nameof(dto.SaveHtml))
                || HasImportedColumn(dto, nameof(dto.Html));
            var hasCss = HasImportedColumn(dto, nameof(dto.SaveCss));
            if (!isNew && !hasHtml && !hasCss) return;

            // SaveHtml 是新版欄位；Html 僅供舊版 Excel 相容。
            var hasEditorHtml = !string.IsNullOrWhiteSpace(dto.SaveHtml);
            var importedHtml = hasEditorHtml ? dto.SaveHtml! : dto.Html ?? "";
            var frontHtml = stringHandler.ResolveFrontUploadPath(
                stringHandler.HtmlDecode(importedHtml),
                orgName);
            if (!hasEditorHtml)
                frontHtml = NormalizeHtml(frontHtml);

            var frontCss = hasCss || isNew
                ? stringHandler.ResolveFrontUploadPath(dto.SaveCss ?? "", orgName)
                : prod.Css ?? "";
            var editorHtml = stringHandler.ResolveUploadPath(frontHtml, orgName);
            var editorCss = stringHandler.ResolveUploadPath(frontCss, orgName);

            if (hasHtml || isNew)
            {
                prod.PageText = htmlProcessor.text(frontHtml);
                prod.Html = stringHandler.HtmlEncode(frontHtml);
                prod.SaveHtml = stringHandler.HtmlEncode(editorHtml);
            }
            if (hasCss || isNew)
            {
                prod.Css = frontCss;
                prod.SaveCss = editorCss;
            }
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

        private async Task InsetProdSpecTypes(
            List<ProductImportDto> prods,
            bool overwriteExistingSpecs)
        {
            if (prods.Count == 0) return;
            long userId = await loginUserData.GetUserId();
            long WebsiteId = await loginUserData.GetWebsiteId();
            var ProdSpecTitleList = db.Prod_Spec_Types
                                    .Where(e => !e.IsDeleted)
                                    .Where(e => e.FK_WebsiteId == prods[0].FK_WebsiteId)
                                    .Select(e => e.Type).ToList();
            var existingProductIds = prods.Where(e => e.Id != 0).Select(e => e.Id).Distinct().ToList();
            var existingSubItemKeys = overwriteExistingSpecs
                ? new HashSet<string>()
                : (await db.Prod_Stocks.AsNoTracking()
                    .Where(e => !e.IsDeleted
                        && !string.IsNullOrEmpty(e.SubItemNo)
                        && existingProductIds.Contains(e.FK_Pid))
                    .Select(e => new { e.FK_Pid, e.SubItemNo })
                    .ToListAsync())
                    .Select(e => $"{e.FK_Pid}|{Norm(e.SubItemNo)}")
                    .ToHashSet();
            List<Prod_Spec_Type> news = new List<Prod_Spec_Type>();
            for (int i = 0; i < prods.Count; i++)
            {
                var items = prods[i];
                if (items.stocks != null)
                {
                    var importableStocks = items.stocks
                        .Where(e => !existingSubItemKeys.Contains($"{items.Id}|{Norm(e.SubItemNo)}"))
                        .ToList();
                    var Adds1 = importableStocks.FindAll(e => !ProdSpecTitleList.Contains(e.S1_Name ?? "")).Select(e => e.S1_Name).ToList();
                    var Adds2 = importableStocks.FindAll(e => !ProdSpecTitleList.Contains(e.S2_Name ?? "")).Select(e => e.S2_Name).ToList();
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
        private async Task InsetProdSpec(
            List<ProductImportDto> prods,
            bool overwriteExistingSpecs)
        {
            long userId = await loginUserData.GetUserId();
            long websiteId = await loginUserData.GetWebsiteId();
            var types = await db.Prod_Spec_Types
                .Where(e => !e.IsDeleted && e.FK_WebsiteId == websiteId)
                .OrderBy(e => e.Id)
                .ToListAsync();
            if (types.Count == 0) return;

            var typeByName = types
                .GroupBy(e => Norm(e.Type))
                .ToDictionary(e => e.Key, e => e.Last());
            var existingSpecs = await db.Prod_Specs
                .Where(e => !e.IsDeleted
                    && e.Prod_Spec_Type != null
                    && e.Prod_Spec_Type.FK_WebsiteId == websiteId)
                .Select(e => new { e.FK_Tid, e.Title, TypeName = e.Prod_Spec_Type!.Type })
                .ToListAsync();
            var existingKeys = existingSpecs
                .Select(e => SpecKey(e.TypeName, e.Title))
                .ToHashSet();

            var requestedSpecs = new List<(string? TypeName, string? Title)>();
            var existingProductIds = prods.Where(e => e.Id != 0).Select(e => e.Id).Distinct().ToList();
            var existingSubItemKeys = overwriteExistingSpecs
                ? new HashSet<string>()
                : (await db.Prod_Stocks.AsNoTracking()
                    .Where(e => !e.IsDeleted
                        && !string.IsNullOrEmpty(e.SubItemNo)
                        && existingProductIds.Contains(e.FK_Pid))
                    .Select(e => new { e.FK_Pid, e.SubItemNo })
                    .ToListAsync())
                    .Select(e => $"{e.FK_Pid}|{Norm(e.SubItemNo)}")
                    .ToHashSet();
            foreach (var product in prods)
            {
                foreach (var stock in product.stocks ?? new List<ProductStockDto>())
                {
                    if (existingSubItemKeys.Contains($"{product.Id}|{Norm(stock.SubItemNo)}"))
                        continue;
                    requestedSpecs.Add((stock.S1_Name, stock.S1_Title));
                    requestedSpecs.Add((stock.S2_Name, stock.S2_Title));
                }
            }

            var news = new List<Prod_Spec>();
            foreach (var requested in requestedSpecs
                .Where(e => !string.IsNullOrWhiteSpace(e.Title))
                .GroupBy(e => SpecKey(e.TypeName, e.Title))
                .Select(e => e.First()))
            {
                var typeName = Norm(requested.TypeName);
                Prod_Spec_Type? specType = null;
                if (!string.IsNullOrEmpty(typeName))
                    typeByName.TryGetValue(typeName, out specType);

                // 舊版 Excel 可能沒有規格類型：標題只有一種既有類型時沿用，否則使用第一個類型。
                if (specType == null)
                {
                    var existingTypeIds = existingSpecs
                        .Where(e => Norm(e.Title) == Norm(requested.Title))
                        .Select(e => e.FK_Tid)
                        .Distinct()
                        .ToList();
                    specType = existingTypeIds.Count == 1
                        ? types.FirstOrDefault(e => e.Id == existingTypeIds[0])
                        : types[0];
                }

                var key = SpecKey(specType?.Type, requested.Title);
                if (specType == null || existingKeys.Contains(key)) continue;

                news.Add(new Prod_Spec
                {
                    Title = requested.Title!.Trim(),
                    FK_Tid = specType.Id,
                    CreationTime = DateTime.Now,
                    CreatorUserId = userId
                });
                existingKeys.Add(key);
            }

            db.Prod_Specs.AddRange(news);
            await db.SaveChangesAsync();
        }
        // 唯一鍵 helper
        private static string Norm(string? s)
            => CustomDtoMapper.Normalize(s).ToUpperInvariant();

        private static (string TypeName, string Title) SpecKey(string? typeName, string? title)
            => (Norm(typeName), Norm(title));

        private static bool TryGetBonusKey(double? bonus, out int key)
        {
            if (!bonus.HasValue) { key = 0; return true; }
            var v = bonus.Value;
            var isInt = Math.Abs(v - Math.Round(v)) < 1e-9;
            key = isInt ? (int)Math.Round(v) : 0;
            return isInt;
        }

        private static (long pid, long s1, long s2) StockKey(long pid, long? s1, long? s2)
            => (pid, s1 ?? 0, s2 ?? 0);

        private async Task UpsertStocksAndPricesBatchAsync(
            List<Prod> items,                      // 追蹤中的新/舊商品（未必已 Save）
            List<ProductImportDto> prods,          // 對應的 Excel DTO
            List<ImportMassageItem> errors,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices)
        {
            if (items == null || items.Count == 0) return;

            long siteId = await loginUserData.GetWebsiteId();

            // 規格必須以「類型＋標題」辨識；不同類型可以有相同標題。
            var specRows = await db.Prod_Specs
                .Include(e => e.Prod_Spec_Type)
                .Where(e => e.Prod_Spec_Type != null && e.Prod_Spec_Type.FK_WebsiteId == siteId)
                .Select(e => new { e.Id, e.Title, TypeName = e.Prod_Spec_Type!.Type })
                .ToListAsync();
            var specByTypeAndTitle = specRows
                .GroupBy(e => SpecKey(e.TypeName, e.Title))
                .ToDictionary(e => e.Key, e => e.OrderByDescending(x => x.Id).First().Id);
            var specIdsByTitle = specRows
                .GroupBy(e => Norm(e.Title))
                .ToDictionary(
                    e => e.Key,
                    e => e.Select(x => x.Id).Distinct().ToList());

            // Excel 對照：ItemNo 優先，否則用 Title
            var dtoByItemNo = prods
                .Where(p => !string.IsNullOrWhiteSpace(p.ItemNo))
                .GroupBy(p => Norm(p.ItemNo))
                .ToDictionary(g => g.Key, g => g.Last());

            var dtoByTitle = prods
                .Where(p => string.IsNullOrWhiteSpace(p.ItemNo))
                .GroupBy(p => Norm(p.ProdName))
                .ToDictionary(g => g.Key, g => g.Last());

            // 只載入「既有商品」的 DB 規格/價格做快取；新商品用本地集合處理
            var existingIds = items.Where(x => x.Id != 0).Select(x => x.Id).Distinct().ToList();

            var dbStocks = await db.Prod_Stocks
                .Where(s => existingIds.Contains(s.FK_Pid) && !s.IsDeleted)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            var stockDictByPid = new Dictionary<(long pid, long s1, long s2), Prod_Stock>();
            foreach (var s in dbStocks)
                stockDictByPid[StockKey(s.FK_Pid, s.FK_S1id, s.FK_S2id)] = s;

            var psIds = dbStocks.Select(s => s.Id).ToList();
            var dbPrices = await db.Prod_Prices
                .Where(p => !p.IsDeleted && psIds.Contains(p.FK_PSId))
                .ToListAsync();

            var priceDict = dbPrices
                .GroupBy(p => (p.FK_PSId, p.FK_RId, (int)(p.Bonus ?? 0)))
                .ToDictionary(
                    p => p.Key,
                    p => p.OrderByDescending(x => x.Id).First());

            foreach (var prod in items)
            {
                try
                {
                    // 對應到 Excel DTO
                    ProductImportDto? dto = null;
                    if (!string.IsNullOrWhiteSpace(prod.ItemNo))
                        dtoByItemNo.TryGetValue(Norm(prod.ItemNo), out dto);
                    if (dto == null)
                        dtoByTitle.TryGetValue(Norm(prod.Title), out dto);

                    if (dto == null || dto.stocks == null || dto.stocks.Count == 0)
                        continue;

                    // 將「規格類型＋標題」轉為 Id；舊版資料僅在標題唯一時相容。
                    foreach (var s in dto.stocks)
                    {
                        if (!string.IsNullOrWhiteSpace(s.S1_Title))
                            s.FK_S1id = ResolveSpecId(s.S1_Name, s.S1_Title);
                        if (!string.IsNullOrWhiteSpace(s.S2_Title))
                            s.FK_S2id = ResolveSpecId(s.S2_Name, s.S2_Title);
                    }

                    foreach (var s in dto.stocks)
                    {
                        var s1 = s.FK_S1id ?? 0;
                        var s2 = s.FK_S2id ?? 0;

                        Prod_Stock? stockEntity = null;

                        // ① 有 SubItemNo 時優先視為穩定識別；否則才以規格組合尋找。
                        if (prod.Id != 0)
                        {
                            var key = StockKey(prod.Id, s1, s2);
                            if (!string.IsNullOrWhiteSpace(s.SubItemNo))
                            {
                                var sameSubItemNo = dbStocks.FirstOrDefault(e =>
                                    e.FK_Pid == prod.Id
                                    && Norm(e.SubItemNo) == Norm(s.SubItemNo));
                                if (sameSubItemNo != null)
                                {
                                    if (overwriteExistingSpecs)
                                    {
                                        sameSubItemNo.FK_S1id = s1;
                                        sameSubItemNo.FK_S2id = s2;
                                        stockEntity = sameSubItemNo;
                                        stockDictByPid[key] = sameSubItemNo;
                                    }
                                    else
                                    {
                                        // 未授權變更規格時，沿用該 SubItemNo 目前的既有規格。
                                        stockEntity = sameSubItemNo;
                                    }
                                }
                            }
                            if (stockEntity == null)
                                stockDictByPid.TryGetValue(key, out stockEntity);
                        }

                        // ② 追蹤中的本地集合（新商品或剛新建的規格）
                        if (stockEntity == null && prod.Prod_Stocks != null)
                        {
                            stockEntity = prod.Prod_Stocks
                                .FirstOrDefault(x => !x.IsDeleted &&
                                                     (x.FK_S1id ?? 0) == s1 &&
                                                     (x.FK_S2id ?? 0) == s2);
                        }

                        // ③ 都沒有 → 新建規格
                        if (stockEntity == null)
                        {
                            stockEntity = new Prod_Stock
                            {
                                FK_S1id = s1,
                                FK_S2id = s2,
                                Stock = s.Stock,
                                Min_Qty = s.Min_Qty,
                                Alert_Qty = s.Alert_Qty,
                                SubItemNo = s.SubItemNo,
                                SpecDescription = s.SpecDescription,
                                // ！關鍵：用導覽屬性關聯（新商品 Id==0 亦可）
                                Prod = prod
                            };
                            db.Prod_Stocks.Add(stockEntity);

                            // 若是既有商品，順手補進 (pid,s1,s2) 快取
                            if (prod.Id != 0)
                                stockDictByPid[StockKey(prod.Id, s1, s2)] = stockEntity;

                            // 維護本地集合，方便同一輪後續找到
                            prod.Prod_Stocks ??= new List<Prod_Stock>();
                            if (!prod.Prod_Stocks.Contains(stockEntity))
                                prod.Prod_Stocks.Add(stockEntity);
                        }
                        else
                        {
                            // 更新既有規格欄位
                            if (HasImportedColumn(dto, nameof(dto.Stock))) stockEntity.Stock = s.Stock;
                            if (HasImportedColumn(dto, nameof(dto.Min_Qty))) stockEntity.Min_Qty = s.Min_Qty;
                            if (HasImportedColumn(dto, nameof(dto.Alert_Qty))) stockEntity.Alert_Qty = s.Alert_Qty;
                            if (HasImportedColumn(dto, nameof(dto.SubItemNo))) stockEntity.SubItemNo = s.SubItemNo;
                            if (HasImportedColumn(dto, nameof(dto.SpecDescription))) stockEntity.SpecDescription = s.SpecDescription;
                        }

                        var isNewStock = stockEntity.Id == 0;
                        // 詢價（不刪舊價；只標記並把通用價歸零）
                        var hasPrice = HasImportedColumn(dto, nameof(dto.Price));
                        var requestedIsTimePrice = hasPrice ? s.TimePrice || s.Price < 0 : stockEntity.IsTimePrice;
                        if (!isNewStock
                            && hasPrice
                            && !overwriteExistingPrices
                            && requestedIsTimePrice != stockEntity.IsTimePrice)
                            continue;
                        var isTimePrice = !isNewStock && !overwriteExistingPrices
                            ? stockEntity.IsTimePrice
                            : requestedIsTimePrice;
                        if ((hasPrice && (isNewStock || overwriteExistingPrices)) || isNewStock)
                            stockEntity.IsTimePrice = isTimePrice;
                        if ((HasImportedColumn(dto, nameof(dto.SuggestPrice))
                                && (isNewStock || overwriteExistingPrices))
                            || isNewStock)
                            stockEntity.Price = s.SuggestPrice;

                        // 詢價就不處理角色價
                        if (isTimePrice || (!hasPrice && !isNewStock)) continue;

                        // 角色價：同 (roleId, bonusKey) 最後一筆覆蓋；Bonus 必為整數
                        var roleBonusMap = new Dictionary<(long roleId, int bonusKey), ProductPriceDto>();
                        if (s.Prices != null)
                        {
                            foreach (var p in s.Prices)
                            {
                                if (p.FK_RId <= 0)
                                    continue;

                                if (!TryGetBonusKey(p.Bonus, out var bonusKey))
                                {
                                    errors.Add(new ImportMassageItem
                                    {
                                        Name = prod.Title,
                                        Description = $"Bonus 必為整數；收到 {p.Bonus}（S1={s1}, S2={s2}, Role={p.FK_RId}）。已略過此價格。"
                                    });
                                    continue;
                                }
                                roleBonusMap[(p.FK_RId, bonusKey)] = p; // 後者覆蓋前者
                            }
                        }

                        foreach (var ((roleId, bonusKey), dtoPrice) in roleBonusMap)
                        {
                            Prod_Price? entity = null;
                            var isNewPrice = false;

                            if (stockEntity.Id != 0)
                            {
                                // ✅ 既有規格：用 (psId, roleId, bonusKey) 查 DB 快取
                                if (!priceDict.TryGetValue((stockEntity.Id, roleId, bonusKey), out entity))
                                {
                                    if (!overwriteExistingPrices)
                                        continue;
                                    entity = new Prod_Price { FK_PSId = stockEntity.Id, FK_RId = roleId };
                                    db.Prod_Prices.Add(entity);
                                    priceDict[(stockEntity.Id, roleId, bonusKey)] = entity;
                                    isNewPrice = true;
                                }
                            }
                            else
                            {
                                // ✅ 新規格（尚未有 Id）：用「本地集合」去重，而不是一律新增
                                stockEntity.Prod_Prices ??= new List<Prod_Price>();
                                entity = stockEntity.Prod_Prices
                                    .FirstOrDefault(pp => !pp.IsDeleted
                                                          && pp.FK_RId == roleId
                                                          && (int)(pp.Bonus ?? 0) == bonusKey);

                                if (entity == null)
                                {
                                    entity = new Prod_Price { Prod_Stock = stockEntity, FK_RId = roleId };
                                    db.Prod_Prices.Add(entity);
                                    // 保險起見把兩邊關聯都維護好（避免某些情況下未自動 fixup）
                                    stockEntity.Prod_Prices.Add(entity);
                                    isNewPrice = true;
                                }
                            }

                            if (!isNewPrice && !overwriteExistingPrices)
                                continue;
                            // 金額可為浮點：若實體欄位是 decimal，這裡轉型；若是 double 就直接指定
                            entity.Price = dtoPrice.Price ?? 0;
                            entity.Bonus = bonusKey;
                            entity.IsDeleted = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportMassageItem { Name = prod.Title, Description = ex.Message });
                }
            }

            long ResolveSpecId(string? typeName, string? title)
            {
                if (string.IsNullOrWhiteSpace(title)) return 0;
                if (specByTypeAndTitle.TryGetValue(SpecKey(typeName, title), out var exactId))
                    return exactId;

                return specIdsByTitle.TryGetValue(Norm(title), out var ids) && ids.Count == 1
                    ? ids[0]
                    : 0;
            }
        }
    }
}
