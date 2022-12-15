
namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartGetDrop
    {
        public long? SCId { get; set; }
        public long? PId { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
