
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsCreateHomeRequestDto : ECPayLogisticsCreateRequestDto
    {
        public override string LogisticsType => "Home";
        public string GoodsWeight { get; set; }
        public string SenderZipCode { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverZipCode { get; set; }
        public string ReceiverAddress { get; set; }
        public string Temperature { get; set; }
        public string Distance { get; set; } = "00";
        public string Specification { get; set; }
        public string ScheduledPickupTime { get; set; } = "4";
        public string ScheduledDeliveryTime { get; set; } = "4";
    }
}
