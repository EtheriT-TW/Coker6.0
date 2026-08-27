namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartVariantUpdateDto
    {
        public long CartId { get; set; }
        public long ProductStockId { get; set; }
        public long ProductPriceId { get; set; }
        public int Quantity { get; set; }
    }

    public class ShoppingCartVariantUpdateResultDto
    {
        public long CartId { get; set; }
        public long? RemovedCartId { get; set; }
        public bool Merged { get; set; }
    }
}
