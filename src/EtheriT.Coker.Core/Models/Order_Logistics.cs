using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Order_Logistics : FullAuditedEntity
    {
        public long FK_OhId { get; set; }
        public string LogisticsType { get; set; }
        public string LogisticsSubType { get; set; }
        public string MerchantTradeNo { get; set; }
        public DateTime? MerchantTradeDate { get; set; }
        public string? AllPayLogisticsID { get; set; }
        public string? BookingNote { get; set; }
        public string? CVSStoreID { get; set; }
        public string? CVSStoreName { get; set; }
        public string? CVSAddress { get; set; }
        public string? CVSTelephone { get; set; }
        public string? CVSOutSide { get; set; }
        public string? CVSPaymentNo { get; set; }
        public string? CVSValidationNo { get; set; }
        public decimal? GoodsWeight {  get; set; }
        public string? Temperature { get; set; }
        public string? Specification { get; set; }
        public string? LogisticsStatusCode { get; set; }
        public DateTime? UpdateStatusDate { get; set; }
    }
}
