
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsCreateCVSRequestDto : ECPayLogisticsCreateRequestDto
    {
        public override string LogisticsType => "CVS";
        public decimal CollectionAmount { get; set; }
        public string ReceiverStoreID { get; set; }
        public string ReturnStoreID { get; set; }
    }
}
