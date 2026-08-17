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
    public class ProductImportAppService : IProductImportAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITagAppService tagAppService;
        private readonly IMapper mapper;
        private readonly ITechnicalCertificateAppService technicalCertificateAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly IHtmlProcessor htmlProcessor;
        private readonly StringHandler stringHandler;
        private readonly ImportAppService importAppService;
        private readonly IWebsiteCacheStateAppService websiteCacheStateAppService;
        private readonly IHtmlSanitizeService htmlSanitizeService;

        public ProductImportAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITagAppService tagAppService,
            IMapper mapper,
            ITechnicalCertificateAppService technicalCertificateAppService,
            IFileUploadAppService fileUploadAppService,
            IHtmlProcessor htmlProcessor,
            StringHandler stringHandler,
            ImportAppService importAppService,
            IWebsiteCacheStateAppService websiteCacheStateAppService,
            IHtmlSanitizeService htmlSanitizeService)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tagAppService = tagAppService;
            this.mapper = mapper;
            this.technicalCertificateAppService = technicalCertificateAppService;
            this.fileUploadAppService = fileUploadAppService;
            this.htmlProcessor = htmlProcessor;
            this.stringHandler = stringHandler;
            this.importAppService = importAppService;
            this.websiteCacheStateAppService = websiteCacheStateAppService;
            this.htmlSanitizeService = htmlSanitizeService;
        }

        /* Product Import */
        private async Task<ImportOutputDto> ProdReplace(
            IList<IFormFile> files,
            long templateId,
            bool overwriteExisting)
        {
            return await ProdReplace(files, templateId, overwriteExisting, false);
        }

        private async Task<ImportOutputDto> ProdReplace(
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

        private async Task<ImportOutputDto> ProdReplace(
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

        private async Task<ImportOutputDto> ProdReplace(
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
                .GroupBy(ProductImportIdentityKey)
                .Count();
            var productNameConflictItemNos = productRows
                .Where(e => !e.ProductId.HasValue && !string.IsNullOrWhiteSpace(e.ItemNo))
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

            var invalidProductIdRows = productRows
                .Where(e => e.ProductId.HasValue && e.ProductId.Value <= 0)
                .ToList();
            foreach (var invalidRow in invalidProductIdRows)
            {
                result.Errors.Add(new ImportMassageItem
                {
                    Name = $"商品 ID：{invalidRow.ProductId}",
                    Description = "商品 ID 必須是大於 0 的整數；若要沿用商品編號／名稱判斷，請將商品 ID 留空。",
                    Sheet = "商品",
                    RowNumbers = new List<int> { invalidRow.SourceRowNumber },
                    CanIgnore = false
                });
            }

            var requestedProductIds = productRows
                .Where(e => e.ProductId > 0)
                .Select(e => e.ProductId!.Value)
                .Distinct()
                .ToList();
            var fallbackRows = productRows
                .Where(e => !e.ProductId.HasValue)
                .ToList();
            var itemNos = fallbackRows
                .Where(e => !string.IsNullOrWhiteSpace(e.ItemNo))
                .Select(e => e.ItemNo)
                .Distinct()
                .ToList();
            var productTitlesWithoutItemNo = fallbackRows
                .Where(e => string.IsNullOrWhiteSpace(e.ItemNo))
                .Select(e => e.ProdName)
                .Distinct()
                .ToList();
            var existingProducts = await db.Prods.AsNoTracking()
                .Where(e => !e.IsDeleted
                    && e.FK_WebsiteId == websiteId
                    && (requestedProductIds.Contains(e.Id)
                        || (!string.IsNullOrEmpty(e.ItemNo) && itemNos.Contains(e.ItemNo))
                        || (string.IsNullOrEmpty(e.ItemNo) && productTitlesWithoutItemNo.Contains(e.Title))))
                .Select(e => new { e.Id, e.ItemNo, e.Title })
                .ToListAsync();

            var foundProductIds = existingProducts.Select(e => e.Id).ToHashSet();
            foreach (var missingProductId in requestedProductIds.Where(e => !foundProductIds.Contains(e)))
            {
                var rows = productRows.Where(e => e.ProductId == missingProductId).ToList();
                result.Errors.Add(new ImportMassageItem
                {
                    Name = $"商品 ID：{missingProductId}",
                    Description = "找不到此商品 ID，或商品不屬於目前網站；已停止將它當成新商品匯入。",
                    Sheet = "商品",
                    RowNumbers = rows.Select(e => e.SourceRowNumber).Distinct().OrderBy(e => e).ToList(),
                    CanIgnore = false
                });
            }
            result.Summary.ProductUpdatedCount = productRows
                .Where(e => existingProducts.Any(p => MatchesImportedProduct(e, p.Id, p.ItemNo, p.Title)))
                .GroupBy(ProductImportIdentityKey)
                .Count();
            result.Summary.ProductAddedCount = productRows
                .Where(e => !e.ProductId.HasValue
                    && !existingProducts.Any(p => MatchesImportedProduct(e, p.Id, p.ItemNo, p.Title)))
                .GroupBy(ProductImportIdentityKey)
                .Count();
            foreach (var product in productRows
                .Where(e => e.ProductId.HasValue
                    || (!string.IsNullOrWhiteSpace(e.ItemNo)
                        && !productNameConflictItemNos.Contains(Norm(e.ItemNo))))
                .GroupBy(ProductImportIdentityKey)
                .Select(e => e.First()))
            {
                var existing = existingProducts.FirstOrDefault(e =>
                    MatchesImportedProduct(product, e.Id, e.ItemNo, e.Title));
                if (existing != null && Norm(existing.Title) != Norm(product.ProdName))
                {
                    result.Differences.Add(new ProductImportDifferenceDto
                    {
                        Code = ProductImportDifferenceCodes.ProductName,
                        Sheet = "商品",
                        Name = product.ProductId.HasValue
                            ? $"商品 ID：{product.ProductId}"
                            : $"ItemNo：{product.ItemNo}",
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
                var existingProduct = existingProducts.FirstOrDefault(e =>
                    MatchesImportedProduct(row, e.Id, e.ItemNo, e.Title));
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
                var existingProduct = existingProducts.FirstOrDefault(e =>
                    MatchesImportedProduct(row, e.Id, e.ItemNo, e.Title));
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
                        var matchingPrices = existingPrices
                            .Where(e => e.FK_PSId == stock.Id
                                && NormalizeProductPriceRoleId(e.FK_RId)
                                    == NormalizeProductPriceRoleId(roleId)
                                && (int)(e.Bonus ?? 0) == row.Bonus)
                            .OrderByDescending(e => e.Id)
                            .ToList();
                        var currentPrice = matchingPrices.FirstOrDefault();
                        var excelIsTimePrice = row.Price < 0;
                        var priceChanged = excelIsTimePrice != stock.IsTimePrice
                            || (!excelIsTimePrice && (currentPrice == null
                                || matchingPrices.Any(e => e.Price != row.Price)));
                        if (priceChanged)
                        {
                            var currentPriceText = matchingPrices.Count == 0
                                ? "尚無價格"
                                : string.Join("、", matchingPrices
                                    .Select(e => e.Price ?? 0)
                                    .Distinct()
                                    .OrderBy(e => e)
                                    .Select(e => $"${e:N0}"));
                            existingValues.Add(stock.IsTimePrice
                                ? $"{roleName}：時價"
                                : $"{roleName}／紅利 {row.Bonus}：{currentPriceText}");
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
                    && !IsMenuUnderImportedParent(
                        menus,
                        level2,
                        row.Level1,
                        row.Level1RouterName,
                        true))
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
                    && !IsMenuUnderImportedParent(
                        menus,
                        level3,
                        row.Level2,
                        row.Level2RouterName,
                        true))
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
                var productsByIdentity = new Dictionary<string, ProductImportDto>();
                var expectedProductCount = allData
                    .Select(ProductImportIdentityKey)
                    .Distinct()
                    .Count();
                var requestedProductIds = allData
                    .Where(e => e.ProductId > 0)
                    .Select(e => e.ProductId!.Value)
                    .Distinct()
                    .ToList();
                var fallbackData = allData.Where(e => !e.ProductId.HasValue).ToList();
                List<string> allTitles = fallbackData.Select(p => p.ProdName).ToList();
                List<string> allItemNos = fallbackData.Select(p => p.ItemNo).ToList();
                var updateItems = db.Prods.Where(e => !e.IsDeleted)
                    .Where(e => e.FK_WebsiteId == WebsiteID)
                    .Where(p => requestedProductIds.Contains(p.Id)
                        || (string.IsNullOrEmpty(p.ItemNo)
                            ? allTitles.Contains((p.Title ?? "").Trim())
                            : allItemNos.Contains((p.ItemNo ?? "").Trim())))
                    .Select(s => new { s.Id, s.ItemNo, s.Title }).ToList();
                for (int i = 0; i < allData.Count; i++)
                {
                    var el = allData[i];
                    if (el.ProductId.HasValue && el.ProductId.Value <= 0)
                        throw new InvalidOperationException(
                            $"Excel 第 {el.SourceRowNumber} 列的商品 ID 必須大於 0；若要沿用商品編號／名稱判斷，請將商品 ID 留空。");

                    var item = updateItems.Find(e =>
                        MatchesImportedProduct(el, e.Id, e.ItemNo, e.Title));
                    if (el.ProductId.HasValue && item == null)
                        throw new InvalidOperationException(
                            $"Excel 第 {el.SourceRowNumber} 列的商品 ID {el.ProductId} 不存在，或不屬於目前網站，已停止匯入以避免誤新增。");
                    el.FK_WebsiteId = WebsiteID;
                    if (item != null) el.Id = item.Id;
                    var identityKey = ProductImportIdentityKey(el);
                    if (!productsByIdentity.TryGetValue(identityKey, out var groupedProduct))
                    {
                        groupedProduct = el;
                        groupedProduct.stocks = new List<ProductStockDto>();
                        prods.Add(el);
                        productsByIdentity[identityKey] = groupedProduct;
                    }
                    groupedProduct.stocks?.Add(mapper.Map<ProductStockDto>(el));
                }
                if (prods.Count != expectedProductCount)
                    throw new InvalidOperationException(
                        $"商品分組結果不一致：掃描辨識 {expectedProductCount} 隻，正式匯入僅辨識 {prods.Count} 隻。為避免商品被靜默覆蓋，本次匯入已停止，請聯絡系統管理員。");
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
                nameof(ProductImportDto.ProductId), nameof(ProductImportDto.ItemNo),
                nameof(ProductImportDto.ProdName),
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

            foreach (var stockGroup in products
                .GroupBy(ProductImportIdentityKey)
                .SelectMany(productGroup => productGroup
                    .GroupBy(ProductImportStockKey)))
            {
                var quantities = stockGroup
                    .Where(e => e.Stock.HasValue)
                    .Select(e => e.Stock!.Value)
                    .Distinct()
                    .ToList();
                if (quantities.Count <= 1)
                    continue;

                var first = stockGroup.First();
                var productLabel = string.IsNullOrWhiteSpace(first.ItemNo)
                    ? first.ProdName
                    : $"ItemNo：{first.ItemNo}／{first.ProdName}";
                warnings.Add(new ImportMassageItem
                {
                    Name = $"可銷售量衝突－{productLabel}",
                    Description = $"同一商品規格填寫了不同的可銷售量：{string.Join("、", quantities.OrderBy(e => e))}。請確認後只保留一致數值，避免後列覆蓋前列。",
                    Sheet = "商品",
                    RowNumbers = stockGroup
                        .Where(e => e.Stock.HasValue)
                        .Select(e => e.SourceRowNumber)
                        .Distinct()
                        .OrderBy(e => e)
                        .ToList(),
                    ComparisonValues = stockGroup
                        .Where(e => e.Stock.HasValue)
                        .Select(e => new ImportMassageComparisonValue
                        {
                            RowNumber = e.SourceRowNumber,
                            Label = "可銷售量",
                            Value = e.Stock!.Value.ToString()
                        })
                        .OrderBy(e => e.RowNumber)
                        .ToList(),
                    CanIgnore = true
                });
            }

            var itemNoGroups = products.GroupBy(ProductImportIdentityKey);

            foreach (var group in itemNoGroups)
            {
                var names = group
                    .Select(e => e.ProdName)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .GroupBy(Norm)
                    .Select(e => e.First())
                    .ToList();
                var firstProduct = group.First();
                var itemNo = firstProduct.ProductId.HasValue
                    ? $"商品 ID {firstProduct.ProductId}"
                    : string.IsNullOrWhiteSpace(firstProduct.ItemNo)
                        ? firstProduct.ProdName
                        : firstProduct.ItemNo;
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

        private static string ProductImportIdentityKey(ProductImportDto product)
            => product.ProductId.HasValue
                ? $"id:{product.ProductId.Value}"
                : string.IsNullOrWhiteSpace(product.ItemNo)
                ? $"title:{Norm(product.ProdName)}"
                : $"item:{Norm(product.ItemNo)}";

        private static bool MatchesImportedProduct(
            ProductImportDto imported,
            long existingId,
            string? existingItemNo,
            string? existingTitle)
        {
            if (imported.ProductId.HasValue)
                return imported.ProductId.Value > 0 && imported.ProductId.Value == existingId;

            return string.IsNullOrWhiteSpace(imported.ItemNo)
                ? string.IsNullOrWhiteSpace(existingItemNo)
                    && Norm(existingTitle) == Norm(imported.ProdName)
                : Norm(existingItemNo) == Norm(imported.ItemNo);
        }

        private static string ProductImportStockKey(ProductImportDto product)
            => string.IsNullOrWhiteSpace(product.SubItemNo)
                ? $"spec:{ProductImportSpecKey(product)}"
                : $"sub-item:{Norm(product.SubItemNo)}";

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
                overwriteExistingPrices,
                reportProgress);
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

        private static bool IsMenuUnderImportedParent(
            IReadOnlyList<WebMenu> menus,
            WebMenu child,
            string? parentTitle,
            string? parentRouterName,
            bool useTitleFallback = true)
        {
            if (!child.FK_TopNodeId.HasValue)
                return false;

            var normalizedRouter = Norm(parentRouterName);
            if (!string.IsNullOrEmpty(normalizedRouter))
            {
                var routerMatches = menus
                    .Where(e => Norm(e.RouterName) == normalizedRouter)
                    .ToList();
                if (routerMatches.Count > 0)
                    return routerMatches.Any(e => e.Id == child.FK_TopNodeId.Value);
                if (!useTitleFallback)
                    return false;
            }

            var normalizedTitle = Norm(parentTitle);
            return !string.IsNullOrEmpty(normalizedTitle)
                && menus.Any(e => e.Id == child.FK_TopNodeId.Value
                    && Norm(e.Title) == normalizedTitle);
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
            bool overwriteExistingPrices,
            Action<int, string>? reportProgress)
        {
            reportProgress?.Invoke(36, "正在整理商品規格類型");
            await InsetProdSpecTypes(prods, overwriteExistingSpecs);
            reportProgress?.Invoke(38, "正在整理商品規格");
            await InsetProdSpec(prods, overwriteExistingSpecs);

            reportProgress?.Invoke(40, "正在寫入商品基本資料");
            var upsertResult = await UpsertProducts(
                prods,
                errors,
                overwriteExistingProductNames,
                reportProgress);
            var products = upsertResult.Products;
            reportProgress?.Invoke(45, "正在寫入商品庫存與價格");
            await UpsertStocksAndPricesBatchAsync(
                products,
                prods,
                errors,
                overwriteExistingSpecs,
                overwriteExistingPrices,
                reportProgress);
            reportProgress?.Invoke(50, "正在儲存商品、庫存與價格");
            await db.SaveChangesAsync();

            for (var productIndex = 0; productIndex < products.Count; productIndex++)
            {
                var product = products[productIndex];
                reportProgress?.Invoke(
                    51 + (int)Math.Floor((productIndex + 1) * 3d / products.Count),
                    $"正在整理商品頁面內容（{productIndex + 1}/{products.Count}）");
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

            reportProgress?.Invoke(54, "正在完成商品資料儲存");
            await db.SaveChangesAsync();
            return (upsertResult.AddedCount, upsertResult.UpdatedCount);
        }
        private async Task<(List<Prod> Products, int AddedCount, int UpdatedCount)> UpsertProducts(
            List<ProductImportDto> dtos,
            List<ImportMassageItem> errors,
            bool overwriteExistingProductNames,
            Action<int, string>? reportProgress)
        {
            long userId = await loginUserData.GetUserId();
            string orgName = await loginUserData.GetWebsiteOrgName();
            long websiteId = await loginUserData.GetWebsiteId();
            var results = new List<Prod>();
            var addedCount = 0;
            var updatedCount = 0;
            var existingProductIds = dtos
                .Where(e => e.Id != 0)
                .Select(e => e.Id)
                .Distinct()
                .ToList();
            var existingProductsById = await db.Prods
                .Where(e => existingProductIds.Contains(e.Id)
                    && e.FK_WebsiteId == websiteId
                    && !e.IsDeleted)
                .ToDictionaryAsync(e => e.Id);

            for (var productIndex = 0; productIndex < dtos.Count; productIndex++)
            {
                var dto = dtos[productIndex];
                reportProgress?.Invoke(
                    40 + (int)Math.Floor((productIndex + 1) * 4d / dtos.Count),
                    $"正在寫入商品基本資料（{productIndex + 1}/{dtos.Count}）");
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
                        if (!existingProductsById.TryGetValue(dto.Id, out var existingProduct))
                            throw new InvalidOperationException($"找不到商品 Id {dto.Id}，或商品不屬於目前網站。");
                        prod = existingProduct;
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

        private static long NormalizeProductPriceRoleId(long roleId)
            => roleId is 0 or 1 ? 1 : roleId;

        private static (long pid, long s1, long s2) StockKey(long pid, long? s1, long? s2)
            => (pid, s1 ?? 0, s2 ?? 0);

        private async Task UpsertStocksAndPricesBatchAsync(
            List<Prod> items,                      // 追蹤中的新/舊商品（未必已 Save）
            List<ProductImportDto> prods,          // 對應的 Excel DTO
            List<ImportMassageItem> errors,
            bool overwriteExistingSpecs,
            bool overwriteExistingPrices,
            Action<int, string>? reportProgress)
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
            var dtoByResolvedProductId = prods
                .Where(p => p.Id > 0)
                .GroupBy(p => p.Id)
                .ToDictionary(g => g.Key, g => g.Last());

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

            var pricesByKey = dbPrices
                .GroupBy(p => (
                    p.FK_PSId,
                    RoleId: NormalizeProductPriceRoleId(p.FK_RId),
                    Bonus: (int)(p.Bonus ?? 0)))
                .ToDictionary(
                    p => p.Key,
                    p => p.OrderByDescending(x => x.Id).ToList());

            for (var productIndex = 0; productIndex < items.Count; productIndex++)
            {
                var prod = items[productIndex];
                reportProgress?.Invoke(
                    45 + (int)Math.Floor((productIndex + 1) * 4d / items.Count),
                    $"正在寫入商品庫存與價格（{productIndex + 1}/{items.Count}）");
                try
                {
                    // 對應到 Excel DTO
                    ProductImportDto? dto = null;
                    if (prod.Id > 0)
                        dtoByResolvedProductId.TryGetValue(prod.Id, out dto);
                    if (dto == null && !string.IsNullOrWhiteSpace(prod.ItemNo))
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
                        var hasPrice = HasImportedColumn(dto, nameof(dto.Price));
                        var matchedBySingleStockFallback = false;

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

                        // ProductId 已明確指定商品時，單一規格商品可安全回退到唯一的庫存列。
                        // 這也能相容舊匯出檔中空白規格／子料號欄位被錯誤填值的情況。
                        if (stockEntity == null
                            && dto.ProductId == prod.Id
                            && hasPrice)
                        {
                            var existingStocks = dbStocks
                                .Where(x => x.FK_Pid == prod.Id && !x.IsDeleted)
                                .ToList();
                            if (existingStocks.Count == 1)
                            {
                                stockEntity = existingStocks[0];
                                matchedBySingleStockFallback = true;
                            }
                            else if (existingStocks.Count > 1)
                            {
                                throw new InvalidOperationException(
                                    $"商品 ID {prod.Id} 有多個規格，但 Excel 的 SubItemNo／規格無法對應；價格未更新。請使用最新匯出檔後再匯入。");
                            }
                        }

                        // ③ 都沒有 → 新建規格
                        if (stockEntity == null)
                        {
                            stockEntity = new Prod_Stock
                            {
                                FK_S1id = s1,
                                FK_S2id = s2,
                                Stock = s.Stock ?? 0,
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
                            if (HasImportedColumn(dto, nameof(dto.Stock)) && s.Stock.HasValue)
                                stockEntity.Stock = s.Stock.Value;
                            if (HasImportedColumn(dto, nameof(dto.Min_Qty))) stockEntity.Min_Qty = s.Min_Qty;
                            if (HasImportedColumn(dto, nameof(dto.Alert_Qty))) stockEntity.Alert_Qty = s.Alert_Qty;
                            if (!matchedBySingleStockFallback
                                && HasImportedColumn(dto, nameof(dto.SubItemNo)))
                                stockEntity.SubItemNo = s.SubItemNo;
                            if (HasImportedColumn(dto, nameof(dto.SpecDescription))) stockEntity.SpecDescription = s.SpecDescription;
                        }

                        var isNewStock = stockEntity.Id == 0;
                        // 詢價（不刪舊價；只標記並把通用價歸零）
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
                            var normalizedRoleId = NormalizeProductPriceRoleId(roleId);
                            List<Prod_Price> priceEntities;
                            var isNewPrice = false;

                            if (stockEntity.Id != 0)
                            {
                                // ✅ 既有規格：用 (psId, roleId, bonusKey) 查 DB 快取
                                if (pricesByKey.TryGetValue(
                                    (stockEntity.Id, normalizedRoleId, bonusKey),
                                    out var existingPriceEntities))
                                {
                                    priceEntities = existingPriceEntities;
                                }
                                else
                                {
                                    if (!overwriteExistingPrices)
                                        continue;
                                    var entity = new Prod_Price
                                    {
                                        FK_PSId = stockEntity.Id,
                                        FK_RId = normalizedRoleId
                                    };
                                    db.Prod_Prices.Add(entity);
                                    priceEntities = new List<Prod_Price> { entity };
                                    pricesByKey[(stockEntity.Id, normalizedRoleId, bonusKey)] = priceEntities;
                                    isNewPrice = true;
                                }
                            }
                            else
                            {
                                // ✅ 新規格（尚未有 Id）：用「本地集合」去重，而不是一律新增
                                stockEntity.Prod_Prices ??= new List<Prod_Price>();
                                priceEntities = stockEntity.Prod_Prices
                                    .Where(pp => !pp.IsDeleted
                                        && NormalizeProductPriceRoleId(pp.FK_RId) == normalizedRoleId
                                        && (int)(pp.Bonus ?? 0) == bonusKey)
                                    .ToList();

                                if (priceEntities.Count == 0)
                                {
                                    var entity = new Prod_Price
                                    {
                                        Prod_Stock = stockEntity,
                                        FK_RId = normalizedRoleId
                                    };
                                    db.Prod_Prices.Add(entity);
                                    // 保險起見把兩邊關聯都維護好（避免某些情況下未自動 fixup）
                                    stockEntity.Prod_Prices.Add(entity);
                                    priceEntities.Add(entity);
                                    isNewPrice = true;
                                }
                            }

                            if (!isNewPrice && !overwriteExistingPrices)
                                continue;
                            // 舊資料可能同時存在角色 0、1 的非會員價；兩者在前台是同一方案，
                            // 必須一起更新，否則畫面仍可能挑到未異動的舊價格。
                            foreach (var entity in priceEntities)
                            {
                                entity.Price = dtoPrice.Price ?? 0;
                                entity.Bonus = bonusKey;
                                entity.IsDeleted = false;
                            }
                        }

                        if (dto.ProductId == prod.Id
                            && hasPrice
                            && overwriteExistingPrices
                            && roleBonusMap.Count == 0)
                        {
                            throw new InvalidOperationException(
                                $"商品 ID {prod.Id} 的 Excel 價格列沒有可寫入的會員身分，價格未更新。");
                        }

                        // 簡易價格範本沒有建議售價欄；非會員現金價仍同步到庫存基準價，
                        // 避免不同畫面讀取不同價格來源而顯示舊值。
                        var primaryCashPrice = roleBonusMap
                            .FirstOrDefault(x => NormalizeProductPriceRoleId(x.Key.roleId) == 1
                                && x.Key.bonusKey == 0)
                            .Value;
                        if (primaryCashPrice != null
                            && hasPrice
                            && (isNewStock || overwriteExistingPrices)
                            && !HasImportedColumn(dto, nameof(dto.SuggestPrice)))
                        {
                            stockEntity.Price = primaryCashPrice.Price ?? 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (overwriteExistingPrices)
                        throw new InvalidOperationException($"{prod.Title}：{ex.Message}", ex);
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
