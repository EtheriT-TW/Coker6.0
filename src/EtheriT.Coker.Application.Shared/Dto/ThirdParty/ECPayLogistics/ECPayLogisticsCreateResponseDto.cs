
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsCreateResponseDto
    {
        public string MerchantID { get; set; }
        public string MerchantTradeNo { get; set; }
        public int RtnCode { get; set; }
        public string RtnMsg { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string LogisticsType { get; set; }
        public string LogisticsSubType { get; set; }
        public int GoodsAmount { get; set; }
        public string UpdateStatusDate { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverCellPhone { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string CVSPaymentNo { get; set; }
        public string? CVSValidationNo { get; set; }
        public string? BookingNote { get; set; }
        public string CheckMacValue { get; set; }
    }
}
