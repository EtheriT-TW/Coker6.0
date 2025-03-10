
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderHeaderDisplayDto
    {
        public long? OrdererId { get; set; }
        public long Id { get; set; }
        public int? OldSubtotal { get; set; }
        public string Subtotal { get; set; }
        public string Discount { get; set; }
        public string Bonus { get; set; }
        public string CouponId { get; set; }
        public string Service_Charge { get; set; }
        public string Freight { get; set; }
        public string Total { get; set; }
        public int State { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string StateStr { get; set; }
        public string Orderer { get; set; }
        public string OrdererSex { get; set; }
        public string OrdererEmail { get; set; }
        public string OrdererCellphone { get; set; }
        public string? OrdererTelephone { get; set; }
        public string OrdererAddress { get; set; }
        public string Recipient { get; set; }
        public string RecipientSex { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientCellphone { get; set; }
        public string? RecipientTelephone { get; set; }
        public string RecipientAddress { get; set; }
        public string? Remark { get; set; }
        public int InvoiceRecipient { get; set; }
        public string? InvoiceTitle { get; set; }
        public string? UniformId { get; set; }
        public string InvoiceAddress { get; set; }
        public string Shipping { get; set; }
        public string Payment { get; set; }
        public long PaymentCode { get; set; }
        public long  ThirdParties { get; set; }
        public string? Memo { get; set; }
        public string? TransactionId { get; set; }
        public string? RefundTransactionId { get; set; }
        public string? CreationTime { get; set; }
    }
}
