
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderHeaderAddDto
    {
        public string Orderer { get; set; }
        public int OrdererSex { get; set; }
        public string OrdererEmail { get; set; }
        public string? OrdererTelePhone { get; set; }
        public string OrdererCellPhone { get; set; }
        public string OrdererAddress { get; set; }
        public long? RecipientId { get; set; }
        public string Recipient { get; set; }
        public int RecipientSex { get; set; }
        public string RecipientEmail { get; set; }
        public string? RecipientTelePhone { get; set; }
        public string RecipientCellPhone { get; set; }
        public string RecipientAddress { get; set; }
        public string? Remark { get; set; }
        public int InvoiceRecipient { get; set; }
        public string? InvoiceTitle { get; set; }
        public string? UniformId { get; set; }
        public string InvoiceAddress { get; set; }
        public int Shipping { get; set; }
        public int Payment { get; set; }
        public int State { get; set; }
        public int Subtotal { get; set; }
        public int? Discount { get; set; }
        public int? Bonus { get; set; }
        public long? CouponId { get; set; }
        public int Freight { get; set; }
        public int? Service_Charge { get; set; }
        public List<OrderDetailAddDto> OrderDetails { get; set; }
    }
}
