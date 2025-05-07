
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayCreatePaymentDataDto
    {
        public string PlatformID { get; set; }
        public string MerchantID { get; set; }
        public string PayToken {  get; set; }
        public string MerchantTradeNo { get; set; }
    }
}
