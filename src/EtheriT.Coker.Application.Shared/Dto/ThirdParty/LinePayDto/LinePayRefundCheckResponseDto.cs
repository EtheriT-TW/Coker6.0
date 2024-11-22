
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayRefundCheckResponseDto : LinePayResponseDto
    {
        public List<InfoDto> info { get; set; }
        public class InfoDto
        {
            public string transactionId { get; set; }
            public DateTime transactionDate { get; set; }
            public string transactionType { get; set; }
            public string productName { get; set; }
            public string currency { get; set; }
            public List<PayInfoDto> payInfo { get; set; }
            public class PayInfoDto
            {
                public string method { get; set; }
                public int amount { get; set; }
            }
            public List<RefundDto> refundList { get; set; }
            public class RefundDto
            {
                public string refundTransactionId { get; set; }
                public string transactionType { get; set; }
                public int refundAmount { get; set; }
                public DateTime refundTransactionDate { get; set; }
            }
            public string orderId { get; set; }
            public string payStatus { get; set; }
            public List<LinePayPackageDto> packages { get; set; }
        }
    }
}
