
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayPayInfoDto
    {
        public string Method { get; set; }
        public string Amount { get; set; }
        public string CreditCardNickname { get; set; }
        public string CreditCardBrand { get; set; }
        public string MaskedCreditCardNumber { get; set; }
    }
}
