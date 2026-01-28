
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayThirdPartyDataDto
    {
        public string MerchantID { get; set; }
        public string PlatformID { get; set; }
        public string HashKey { get; set; }
        public string HashIV { get; set; }
        public string ExpireDate { get; set; }
        public string StoreExpireDate_Barcode { get; set; }
        public string StoreExpireDate_CVS { get; set; }
        public string IsCollection { get; set; }
    }
}
