
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomeRefundDto
    {
        public string order_id { get; set; }
        public string refund_id { get; set; }
        public string pay_type { get; set; }
        public int trade_amount { get; set; }
        public int fee { get; set; }
        public int transfer_fee { get; set; }
        public string cover_transfee { get; set; }
    }
}
