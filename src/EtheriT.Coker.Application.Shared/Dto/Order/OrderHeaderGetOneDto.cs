
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderHeaderGetOneDto
    {
        public long Id { get; set; }
        public string Action {  get; set; }
        public string Orderer { get; set; }
        public string? OrdererTelePhone { get; set; }
        public string OrdererCellPhone { get; set; }
        public string OrdererEmail { get; set; }
        public string Recipient { get; set; }
        public string? RecipientTelePhone { get; set; }
        public string RecipientCellPhone { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientEmail { get; set; }
        public InvoiceTypeEnum? InvoiceType { get; set; }
        public string? Carrier { get; set; }
        public string? InvoiceTypeTitle { get; set; }
        public int? InvoiceRecipient { get; set; }
        public string? InvoiceTitle { get; set; }
        public string? UniformId { get; set; }
        public string? InvoiceAddress { get; set; }
        public string Payment { get; set; }
        public long PaymentCode { get; set; }
        public string PaymentIcon { get; set; }
        public long ThirdParties { get; set; }
        public string Shipping { get; set; }
        public OrderStatusEnum State { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string StateStr { get; set; }
        public string Remark { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal? Discount { get; set; }
        public int? Bonus { get; set; }
        public int? GetBonus { get; set; }
        public long? CouponId { get; set; }
        public decimal Freight { get; set; }
        public int? Service_Charge { get; set; }
        public string CreationTime { get; set; }
        public string Memo { get; set; }
        public string SystemMemo { get; set; }
        public string TransactionId { get; set; }
        public string RefundTransactionId { get; set; }
        public bool CanRefund { get; set; }
        public DateTime? RepayAvailableAt { get; set; }
        public int RepayRemainingSeconds { get; set; }
        public string RepayWaitingMessage { get; set; } = "";
    }
}
