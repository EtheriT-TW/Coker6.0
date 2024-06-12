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
        public string Html { get; set; }
        public double? Discount { get; set; }
        public string? ItemNo {  get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public List<TagGetSelectedDto> TagDatas { get; set; }
        public List<TechCertDisplayDto> TechCertDatas { get; set; }
        public List<ProductStockDto> Stocks { get; set; }
        public List<FileGetImgDto> Files { get; set; }
        public List<FileGetProdDisplayDto> Img_Original { get; set; }
        public List<FileGetProdDisplayDto> Img_Medium { get; set; }
        public List<FileGetProdDisplayDto> Img_Small { get; set; }
    }
}
