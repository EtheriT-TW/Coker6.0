
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayResponseDataDto
    {
        public int RtnCode { get; set; }
        public string RtnMsg { get; set; }
        public string PlatformID { get; set; }
        public string MerchantID { get; set; }
        public string Token { get; set; }
        public string TokenExpireDate { get; set; }
    }
}
