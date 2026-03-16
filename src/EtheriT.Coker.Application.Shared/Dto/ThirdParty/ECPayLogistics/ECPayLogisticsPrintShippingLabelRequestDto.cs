
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsPrintShippingLabelRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }
    }
}
