
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto
{
    public class PChomePayRefundStateDto
    {
        public string refund_id {  get; set; }
        public string status { get; set; }
        public int amount { get; set; }
        public int fee { get; set; }
        public int transfer_fee { get; set; }
        public string refund_date { get; set; }
        public string cover_transfee { get; set; }
        public string actual_refund_date { get; set; }
    }
}
