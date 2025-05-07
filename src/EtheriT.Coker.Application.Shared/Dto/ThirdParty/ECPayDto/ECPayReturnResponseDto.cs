
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayReturnResponseDto
    {
        public string MerchantID { get; set; }
        public RpHeaderDto RpHeader { get; set; }
        public class RpHeaderDto
        {
            public string Timestamp { get; set; }
        }
        public int TransCode { get; set; }
        public string TransMsg { get; set; }
        public string Data { get; set; }
    }
}
