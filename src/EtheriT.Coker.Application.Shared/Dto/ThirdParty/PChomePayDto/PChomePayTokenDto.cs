
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomePayTokenDto
    {
        public string token { get; set; }
        public int expired_in { get; set; }
        public int? expired_timestamp { get; set; }
    }
}
