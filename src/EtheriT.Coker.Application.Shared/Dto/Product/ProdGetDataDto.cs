
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdGetDataDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }
        public bool RemovedFromShelves { get; set; }
        public int Ser_No { get; set; }
        public string Introduction { get; set; }
        public string Description { get; set; }
        public double? Discount { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool Permanent { get; set; }
        public string? ItemNo {  get; set; }
        public ProdStatusEnum Status { get; set; }
        public List<TagGetSelectedDto> TagDatas { get; set; }
        public List<TechCertGetSelectedDto> TechCertDatas { get; set; }
        public List<ProductStockDto> Stocks { get; set; }
        public List<FileGetProdDisplayDto> Files { get; set; }
        public List<FileGetProdDisplayDto> Multimedia { get; set; }
    }
}
