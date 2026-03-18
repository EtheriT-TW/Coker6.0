
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayOrderInfoDto
    {
        public string MerchantTradeDate { get; set; }
        public string MerchantTradeNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string ReturnURL { get; set; }
        public string TradeDesc { get; set; }
        public string ItemName { get; set; }
    }
}
