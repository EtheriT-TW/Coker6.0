
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomePayStateDto
    {
        public string order_id { get; set; }
        public string amount { get; set; }
        public string pay_type { get; set; }
        public int trade_amount { get; set; }
        public int platform_amount { get; set; }
        public int pp_fee { get; set; }
        public string create_date { get; set; }
        public string pay_date { get; set; }
        public string actual_pay_date { get; set; }
        public string fail_date { get; set; }
        public string status { get; set; }
        public string status_code { get; set; }
        public PChomePayPaymentInfoDto payment_info { get; set; }
        public string available_date { get; set; }
        public List<PChomePayItemsDto> items { get; set; }
    }
}
