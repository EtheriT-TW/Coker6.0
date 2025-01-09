
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayDataDto
    {
        public string PlatformID { get; set; }
        public string MerchantID { get; set; }
        public int RememberCard {  get; set; }
        public int PaymentUIType { get; set; }
        public string ChoosePaymentList { get; set; }
        public ECPayOrderInfoDto OrderInfo { get; set; }
        public ECPayCardInfoDto CardInfo { get; set; }
        public ECPayUnionPayInfoDto UnionPayInfo { get; set; }
        public ECPAyATMInfoDto ATMInfo { get; set; }
        public ECPayCVSInfoDto CVSInfo { get; set; }
        public ECPayBarcodeInfoDto BarcodeInfo { get; set; }
        public ECPayConsumerInfoDto ConsumerInfo { get; set; }
        public string CustomField { get; set; }

    }
}
