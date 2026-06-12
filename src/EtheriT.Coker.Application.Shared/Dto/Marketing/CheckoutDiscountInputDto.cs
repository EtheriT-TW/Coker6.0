namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CheckoutDiscountInputDto
    {
        public decimal ProductSubtotal { get; set; }

        public List<CheckoutDiscountItemDto> Items { get; set; } = new();

        public long? CouponId { get; set; }

        public long? ShippingId { get; set; }

        public long? PaymentId { get; set; }

        public bool PreviewOnly { get; set; }
    }
}