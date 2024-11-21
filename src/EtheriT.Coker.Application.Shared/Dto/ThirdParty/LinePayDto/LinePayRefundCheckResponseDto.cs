
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayRefundCheckResponseDto : LinePayResponseDto
    {
        public List<InfoDto> info { get; set; }
        public class InfoDto
        {
            public List<RefundDto> refundList { get; set; }
            public class RefundDto
            {
                public long refundTransactionId { get; set; }
                public string transactionType { get; set; }
                public long refundAmount { get; set; }
                public string refundTransactionDate { get; set; }
            }
            public long originalTransactionId { get; set; }
        }
    }
}
