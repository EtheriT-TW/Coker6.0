
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayRefundDataDto
    {
        public string PlatformID { get; set; }
        public string MerchantID { get; set; }
        public string MerchantTradeNo { get; set; }
        public string TradeNo { get; set; }
        public string Action { get; set; }
        public int TotalAmount { get; set; }
    }
}
