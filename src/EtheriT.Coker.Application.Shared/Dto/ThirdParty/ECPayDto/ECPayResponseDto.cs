
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayResponseDto
    {
        public string MerchantID { get; set; }
        public RqHeaderDto RqHeader { get; set; }
        public class RqHeaderDto
        {
            public string Timestamp { get; set; }
        }
        public int TransCode { get; set; }
        public string TransMsg { get; set; }
        public string Data { get; set; }
    }
}
