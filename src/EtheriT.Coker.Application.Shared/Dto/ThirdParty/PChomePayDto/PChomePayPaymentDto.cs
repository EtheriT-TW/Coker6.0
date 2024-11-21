
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomePayPaymentDto
    {
        public string order_id { get; set; }
        public List<string> pay_type { get; set; }
        public int amount { get; set; }
        public List<PChomePayItemsDto> items { get; set; }
        public string return_url { get; set; }
        public string fail_return_url { get; set; }
        public string notify_url { get; set; }
        public string buyer_email { get; set; }
        public PChomePayPaymentInfo atm_info { get; set; }
        public class PChomePayPaymentInfo
        {
            public int expire_days { get; set; }
        }
        public string card_installment { get; set; }
        public string return_timer { get; set; }
        public string member_key { get; set; }
    }
}
