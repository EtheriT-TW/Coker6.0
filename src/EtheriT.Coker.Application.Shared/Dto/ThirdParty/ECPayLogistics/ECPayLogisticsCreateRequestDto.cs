
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsCreateRequestDto
    {
        public string MerchantID { get; set; }
        public string MerchantTradeNo { get; set; }
        public string MerchantTradeDate { get; set; }
        public virtual string LogisticsType { get; } = "";
        public string LogisticsSubType { get; set; }
        public decimal GoodsAmount { get; set; }
        public string IsCollection { get; set; }
        public string GoodsName { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderCellPhone { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverCellPhone { get; set; }
        public string ReceiverEmail { get; set; }
        public string TradeDesc { get; set; }
        public string ServerReplyURL { get; set; }
        public string ClientReplyURL { get; set; }
        public string Remark { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }
    }
}
