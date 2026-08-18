using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Application.Shared.Product
{
    public interface IProductImportAppService
    {
        Task<ImportOutputDto> ProdReplace(
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
            Action<int, string>? reportProgress);

        Task<ProductImportAnalysisDto> AnalyzeProductImport(
            string filePath,
            long templateId,
            List<ProductImportIgnoredRowDto> ignoredRows,
            Action<int, string>? reportProgress);
    }
}
