
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomePayPaymentResponseDto
    {
        public string order_id { get; set; }
        public string payment_url { get; set; }
        public string code { get; set; }
        public string error_type { get; set; }
        public string message { get; set; }
    }
}
