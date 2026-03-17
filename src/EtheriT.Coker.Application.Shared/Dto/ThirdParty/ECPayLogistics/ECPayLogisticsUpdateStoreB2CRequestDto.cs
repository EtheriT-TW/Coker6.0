
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsUpdateStoreB2CRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string ShipmentDate { get; set; }
        public string ReceiverStoreID { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }
    }
}
