
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public  class LinePayConfirmInfoDto
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string AuthorizationExpireDate { get; set; }
        public string RegKey { get; set; }
        public List<LinePayPayInfoDto> PayInfo { get; set; }
        public List<LinePayInfoPackageDto> Packages { get; set; }
        public MerchantReferenceDto MerchantReference { get; set; }
        public class MerchantReferenceDto
        {
            public List<AffiliateCardDto> AffiliateCards { get; set; }
            public class AffiliateCardDto
            {
                public string CardType { get; set; }
                public string CardId { get; set; }
            }
        }
    }
}
