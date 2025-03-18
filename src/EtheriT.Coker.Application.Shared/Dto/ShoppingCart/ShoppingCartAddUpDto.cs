
namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartAddUpDto
    {
        public long? Id { get; set; }
        public string? ProdName { get; set; }
        public long? FK_Pid { get; set; }
        public long? FK_PSid { get; set; }
        public long? FK_PriceId { get; set; }
        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public int Quantity { get; set; }
    }
}
