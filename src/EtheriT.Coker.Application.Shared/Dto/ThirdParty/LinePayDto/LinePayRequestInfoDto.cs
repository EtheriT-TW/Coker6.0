
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayRequestInfoDto
    {
        public string? TransactionId { get; set; }
        public string? PaymentAccessToken { get; set; }
        public LinePayPaymentUrlDto? PaymentUrl { get; set; }
        public class LinePayPaymentUrlDto
        {
            public string? App { get; set; }
            public string? Web { get; set; }
        }
    }
}
