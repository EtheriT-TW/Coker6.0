
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayRequestDto
    {
        public string MerchantID { get; set; }
        public RqHeaderDto RqHeader { get; set; }
        public class RqHeaderDto
        {
            public string Timestamp { get; set; }
        }
        public ECPayDataDto Data { get; set; }
    }
}
