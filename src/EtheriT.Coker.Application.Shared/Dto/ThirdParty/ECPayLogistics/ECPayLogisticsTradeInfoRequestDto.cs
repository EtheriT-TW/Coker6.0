
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsTradeInfoRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string MerchantTradeNo { get; set; }
        public int TimeStamp { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }
    }
}
