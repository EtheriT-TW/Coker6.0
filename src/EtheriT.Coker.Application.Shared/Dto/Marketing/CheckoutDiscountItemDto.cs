using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CheckoutDiscountItemDto
    {
        public long ShoppingCartId { get; set; }

        public long ProductId { get; set; }

        public long ProductStockId { get; set; }

        public long? ProductPriceId { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Subtotal => UnitPrice * Quantity;
    }
}