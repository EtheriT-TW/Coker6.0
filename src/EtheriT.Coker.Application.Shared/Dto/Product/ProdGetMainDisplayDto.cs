using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdGetMainDisplayDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Introduction { get; set; }
        public string Description { get; set; }
        public double? Discount { get; set; }
        public List<TagGetSelectedDto> TagDatas { get; set; }
        public List<TechCertDisplayDto> TechCertDatas { get; set; }
        public List<ProductStockDto> Stocks { get; set; }
        public List<FileGetProdDisplayDto> Files_Original { get; set; }
        public List<FileGetProdDisplayDto> Files_Medium { get; set; }
        public List<FileGetProdDisplayDto> Files_Small { get; set; }
    }
}
