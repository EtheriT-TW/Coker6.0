
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsLogisticsStatusResponseDto
    {
        public decimal? ActualWeight { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string BookingNote { get; set; }
        public int? CollectionAllocateAmount { get; set; }
        public DateTime? CollectionAllocateDate { get; set; }
        public int? CollectionAmount { get; set; }
        public int? CollectionChargeFee { get; set; }
        public string CVSPaymentNo { get; set; }
        public string CVSValidationNo { get; set; }
        public int? GoodsAmount { get; set; }
        public string GoodsName { get; set; }
        public decimal? GoodsWeight { get; set; }
        public int? HandlingCharge { get; set; }
        public int LogisticsStatus { get; set; }
        public string LogisticsType { get; set; }
        public string MerchantID { get; set; }
        public string MerchantTradeNo { get; set; }
        public DateTime? ShipChargeDate { get; set; }
        public string ShipmentNo { get; set; }
        public DateTime TradeDate { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderCellPhone { get; set; }
        public string CheckMacValue { get; set; }
    }
}
