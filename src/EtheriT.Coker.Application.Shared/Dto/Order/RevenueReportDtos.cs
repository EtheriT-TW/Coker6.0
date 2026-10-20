namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public enum RevenueReportGranularity
    {
        Day = 1,
        Month = 2
    }

    public sealed class RevenueReportItemDto
    {
        public DateTime PeriodStart { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public long OrderCount { get; set; }
        public decimal Amount { get; set; }
        public long InvalidOrderCount { get; set; }
        public decimal InvalidAmount { get; set; }
    }

    public sealed class RevenueReportOutputDto
    {
        public RevenueReportGranularity Granularity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<RevenueReportItemDto> Items { get; set; } = new();
    }
}