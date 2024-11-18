
namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public  class ShoppingCartDataDto
    {
        public long FK_PId { get; set; }
        public long FK_PSId { get; set; }
        public long? FK_S1Id { get; set; }
        public long? FK_S2Id { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int? Discont { get; set; }
        public int? Bonus { get; set; }
        public int? PriceType { get; set; }
        public bool IsOrder { get; set; }
    }
}
