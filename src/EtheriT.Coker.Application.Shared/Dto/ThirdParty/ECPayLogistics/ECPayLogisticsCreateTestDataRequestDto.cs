
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsCreateTestDataRequestDto
    {
        public string MerchantID { get; set; }
        public string ClientReplyURL { get; set; }
        public string PlatformID { get; set; }
        public string LogisticsSubType { get; set; }
        public string CheckMacValue { get; set; }
    }
}
