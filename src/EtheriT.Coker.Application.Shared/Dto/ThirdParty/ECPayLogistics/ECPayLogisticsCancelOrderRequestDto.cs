
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsCancelOrderRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string CVSPaymentNo { get; set; }
        public string CVSValidationNo { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }
    }
}
