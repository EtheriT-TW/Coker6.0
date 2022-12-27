
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductPriceDto
    {
        public long Id { get; set; }
        public long FK_PSId { get; set; }
        public long FK_RId { get; set; }
        public double? Price { get; set; }
        public double? Bonus { get; set; }
    }
}
