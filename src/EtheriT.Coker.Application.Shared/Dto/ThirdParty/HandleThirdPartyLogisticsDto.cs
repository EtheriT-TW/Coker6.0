
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class HandleThirdPartyLogisticsDto
    {
        public string? Action { get; set; }
        public long OrderId { get; set; }
        public string? Token { get; set; }
        public string? ExtraData { get; set; }
    }
}
