
namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingQuantityUpdateDto
    {
        public long Id { get; set; }
        public Guid FK_Tid { get; set; }
        public int Quantity { get; set; }
    }
}
