
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class HandleThirdPartyPaymentDto
    {
        public string? Action {  get; set; }
        public long OrderId { get; set; }
        public string ThirdParties {  get; set; }
        public string? Token { get; set; }
    }
}
