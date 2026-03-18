using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Order_Header : FullAuditedEntity
    {
        public long? Fk_UserId { get; set; }
        public Guid Fk_Tid { get; set; }
        public Guid FK_UUID { get; set; }
        public long FK_WebsiteId { get; set; }
        [StringLength(150)]
        public string Orderer { get; set; }
        public int OrdererSex { get; set; }
        [StringLength(150)]
        public string OrdererEmail { get; set; }
        [StringLength(50)]
        public string? OrdererTelePhone { get; set; }
        [StringLength(50)]
        public string OrdererCellPhone { get; set; }
        [StringLength(250)]
        public string OrdererAddress { get; set; }
        [StringLength(150)]
        public string Recipient { get; set; }
        public int RecipientSex { get; set; }
        [StringLength(150)]
        public string RecipientEmail { get; set; }
        [StringLength(50)]
        public string? RecipientTelePhone { get; set; }
        [StringLength(50)]
        public string RecipientCellPhone { get; set; }
        [StringLength(250)]
        public string RecipientAddress { get; set; }
        [StringLength(500)]
        public string? Remark { get; set; }
        public int? InvoiceRecipient { get; set; }
        public InvoiceTypeEnum? InvoiceType { get; set; }
        public PersonalInvoiceTypeEnum? PersonalInvoiceType { get; set; }
        public string? Carrier { get; set; }
        public string? InvoiceTitle { get; set; }
        public string? UniformId { get; set; }
        [StringLength(250)]
        public string? InvoiceAddress { get; set; }
        public long Shipping { get; set; }
        public long Payment { get; set; }
        public OrderStatusEnum State { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? Discount { get; set; }
        public int? Bonus { get; set; }
        public int? GetBonus { get; set; }
        public long? CouponId { get; set; }
        public decimal Freight { get; set; }
        public int? Service_Charge { get; set; }
        [StringLength(500)]
        public string? Memo { get; set; }
        public string? TransactionId { get; set; }
        public string? refundTransactionId { get; set; }
        public DateTime? refundTransactionDate { get; set; }
        public int? RepayTimes { get; set; }
        public DateTime? RepayDate { get; set; }
        public bool IsTemp { get; set; }
        public string? AllPayLogisticsID { get; set; }
        public string? MerchantTradeNo { get; set; }
        public string? CVSStoreID { get; set; }
        public string? LogisticsStatusCode { get; set; }
        public DateTime? LogisticsUpdateStatusDate { get; set; }
        public string? CVSPaymentNo { get; set; }
        public string? CVSValidationNo { get; set; }
        public string? BookingNote { get; set; }
        public PaymentType PaymentType { get; set; }
        public LogisticsSetting LogisticsSetting { get; set; }
        public List<Order_Details> Order_Details { get; set; }

    }
}
