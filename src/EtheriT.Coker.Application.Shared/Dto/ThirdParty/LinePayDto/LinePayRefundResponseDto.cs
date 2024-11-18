

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayRefundResponseDto : LinePayResponseDto
    {
        public LinePayRefundInfoDto? info { get; set; }
        public class LinePayRefundInfoDto
        {
            public string? refundTransactionId { get; set; }
            public string? refundTransactionDate { get; set; }
        }
    }
}
