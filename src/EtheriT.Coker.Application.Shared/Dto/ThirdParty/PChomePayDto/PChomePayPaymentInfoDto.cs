
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomePayPaymentInfoDto
    {
        public string virtual_account {  get; set; }
        public string bank_code { get; set; }
        public string expire_days { get; set; }
        public string expire_date { get; set; }
        public string installment {  get; set; }
        public float rate { get; set; }
        public string card_last_number { get; set; }
        public int pp_fee { get; set; }
        public string logistic_id { get; set; }
        public string receiver_name { get; set; }
        public string receiver_mobile { get; set; }
        public string store_id { get; set; }
        public string store_name { get; set; }
        public string pincode { get; set; }
        public string barcode1 { get; set; }
        public string barcode2 { get; set; }
        public string barcode3 { get; set; }
    }
}
