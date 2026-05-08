
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductPriceDto
    {
        public long Id { get; set; }
        public long? FK_PSId { get; set; }
        public long FK_RId { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriPrice { get; set; }
        public decimal? SuggestPrice { get; set; }
        public int Bonus { get; set; }
        public string? RoleName { get; set; }
        public string? BaseRoleName { get; set; }
        public bool IsDelete { get; set; }
    }
}
