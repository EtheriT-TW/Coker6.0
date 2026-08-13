using System.Collections.Generic;

namespace EtheriT.Coker.Application.Shared.Dto.Import
{
    public static class ProductImportDifferenceCodes
    {
        public const string ProductName = "product-name";
        public const string ProductSpec = "product-spec";
        public const string ProductPrice = "product-price";
        public const string TechnicalCertificate = "technical-certificate";
        public const string DuplicateMenuTitle = "duplicate-menu-title";
        public const string MenuParent = "menu-parent";
        public const string DirectoryPage = "directory-page";
    }

    public class ProductImportDifferenceDto
    {
        public string Code { get; set; } = string.Empty;
        public string Sheet { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ExistingValue { get; set; } = string.Empty;
        public string ExcelValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ProductImportAnalysisDto
    {
        public List<ImportMassageItem> Errors { get; set; } = new();
        public List<ProductImportDifferenceDto> Differences { get; set; } = new();
        public ProductImportSummaryDto Summary { get; set; } = new();
        public bool CanImport => Errors.Count == 0;
    }

    public class ConfirmProductImportDto
    {
        public long TaskId { get; set; }
        public long TemplateId { get; set; }
        public bool OverwriteExistingProductNames { get; set; }
        public bool OverwriteExistingSpecs { get; set; }
        public bool OverwriteExistingPrices { get; set; }
        public bool OverwriteExistingTechnicalCertificates { get; set; }
        public bool AllowDuplicateMenuTitles { get; set; }
        public bool OverwriteExistingMenuParents { get; set; }
        public bool OverwriteExistingDirectoryPages { get; set; }
        public List<ProductImportIgnoredRowDto> IgnoredRows { get; set; } = new();
    }

    public class ProductImportIgnoredRowDto
    {
        public string Sheet { get; set; } = string.Empty;
        public int RowNumber { get; set; }
    }
}
