
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsUpdateStoreC2CRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string CVSPaymentNo { get; set; }
        public string CVSValidationNo { get; set; }
        public string StoreType { get; set; }
        public string ReceiverStoreID { get; set; }
        public string ReturnStoreID { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }
    }
}
