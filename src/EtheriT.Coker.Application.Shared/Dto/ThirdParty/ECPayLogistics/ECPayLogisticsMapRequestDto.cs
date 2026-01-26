
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsMapRequestDto
    {
        public string MerchantID { get; set; }
        public string MerchantTradeNo { get; set; }
        public string LogisticsType { get; set; } = "CVS";
        public string LogisticsSubType { get; set; }
        public string IsCollection { get; set; }
        public string ServerReplyURL { get; set; }
        public string ExtraData { get; set; }
        public int Device { get; set; }
    }
}
