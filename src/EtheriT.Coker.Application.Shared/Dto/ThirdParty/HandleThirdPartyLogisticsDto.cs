
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class HandleThirdPartyLogisticsDto
    {
        public string? Action { get; set; }
        public long OrderId { get; set; }
        public string? Token { get; set; }
        public string? ExtraData { get; set; }
        public string? SenderName { get; set; }
        public string? SenderCellPhone { get; set; }
        public string? SenderZipCode { get; set; }
        public string? SenderAddress { get; set; }
        public decimal? GoodsWeight { get; set; }
        public string? Specification { get; set; }
    }
}
