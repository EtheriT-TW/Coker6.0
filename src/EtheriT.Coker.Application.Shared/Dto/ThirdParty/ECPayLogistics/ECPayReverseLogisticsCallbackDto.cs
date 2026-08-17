
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayReverseLogisticsCallbackDto
    {
        public string MerchantID { get; set; }
        public string RtnMerchantTradeNo { get; set; }
        public string RtnCode { get; set; }
        public string RtnMsg { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string GoodsAmount { get; set; }
        public string UpdateStatusDate { get; set; }
        public string BookingNote { get; set; }
        public string CheckMacValue { get; set; }
    }
}
