
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsReturnHomeDeliveryRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string LogisticsSubType { get; set; }
        public string ServerReplyURL { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderCellPhone { get; set; }
        public string SenderZipCode { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverCellPhone { get; set; }
        public string ReceiverZipCode { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverEmail { get; set; }
        public int GoodsAmount { get; set; }
        public string GoodsName { get; set; }
        public string Temperature { get; set; }
        public string Distance { get; set; }
        public string Specification { get; set; }
        public string ScheduledPickupTime { get; set; }
        public string ScheduledDeliveryTime { get; set; }
        public string Remark { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }

    }
}
