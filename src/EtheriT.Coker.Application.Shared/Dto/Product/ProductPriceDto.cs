
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductPriceDto
    {
        public long Id { get; set; }
        public long? FK_PSId { get; set; }
        public long FK_RId { get; set; }
        public double? Price { get; set; }
        public double? OriPrice { get; set; }
        public double? SuggestPrice { get; set; }
        public double Bonus { get; set; }
        public bool IsDelete { get; set; }
    }
}
