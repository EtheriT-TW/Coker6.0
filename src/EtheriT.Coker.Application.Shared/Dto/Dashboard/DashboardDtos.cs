namespace EtheriT.Coker.Application.Shared.Dto.Dashboard
{
    public sealed class DashboardSystemOverviewDto
    {
        public string StorageSize { get; set; } = string.Empty;
        public string StorageUpdatedAt { get; set; } = string.Empty;
        public string MonthFlowSize { get; set; } = string.Empty;
        public string MonthRange { get; set; } = string.Empty;
    }

    public sealed class DashboardTrafficOutputDto
    {
        public List<DashboardTrafficItemDto> Items { get; set; } = new();
        public string Granularity { get; set; } = "day";
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class DashboardTrafficItemDto
    {
        public DateTime Date { get; set; }
        public long PageViews { get; set; }
        public long Visitors { get; set; }
    }

    public sealed class DashboardTrafficHeatmapOutputDto
    {
        public int Days { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableDays { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<DashboardTrafficHeatmapCellDto> Items { get; set; } = new();
        public List<DashboardRecommendedTrafficSlotDto> RecommendedSlots { get; set; } = new();
    }

    public sealed class DashboardTrafficHeatmapCellDto
    {
        public int DayOfWeek { get; set; }
        public int Hour { get; set; }
        public int SampleDays { get; set; }
        public long TotalVisitors { get; set; }
        public long TotalViews { get; set; }
        public double AverageVisitors { get; set; }
        public double AverageViews { get; set; }
    }

    public sealed class DashboardRecommendedTrafficSlotDto
    {
        public int DayOfWeek { get; set; }
        public int Hour { get; set; }
        public int SampleDays { get; set; }
        public double AverageVisitors { get; set; }
        public double AverageViews { get; set; }
    }

    public sealed class DashboardPopularPageDto
    {
        public long WebMenuId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Views { get; set; }
        public long Visitors { get; set; }
    }

    public sealed class DashboardContactsOutputDto
    {
        public bool HasData { get; set; }
        public long PendingCount { get; set; }
        public List<DashboardContactFormSummaryDto> Forms { get; set; } = new();
        public List<DashboardRecentContactDto> Recent { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class DashboardCommerceOverviewDto
    {
        public bool IsCommerceEnabled { get; set; }
        public bool HasOrderData { get; set; }
        public long PendingConfirmationCount { get; set; }
        public long PendingPaymentCount { get; set; }
        public long AwaitingShipmentCount { get; set; }
        public long ShippingCount { get; set; }
        public long LowStockCount { get; set; }
        public long SoldOutProductCount { get; set; }
        public decimal TodayOrderAmount { get; set; }
        public decimal YesterdayOrderAmount { get; set; }
        public decimal MonthOrderAmount { get; set; }
        public decimal PreviousMonthOrderAmount { get; set; }
        public long TodayPaidOrderCount { get; set; }
        public long MonthPaidOrderCount { get; set; }
        public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new();
    }

    public sealed class DashboardRecentOrderDto
    {
        public long Id { get; set; }
        public string Orderer { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public sealed class DashboardOrderTrendOutputDto
    {
        public bool IsCommerceEnabled { get; set; }
        public int Days { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DashboardOrderTrendItemDto> Items { get; set; } = new();
    }

    public sealed class DashboardOrderTrendItemDto
    {
        public DateTime Date { get; set; }
        public long OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public sealed class DashboardContactFormSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public long PendingCount { get; set; }
        public long ProcessingCount { get; set; }
        public long RepliedCount { get; set; }
        public long CompletedCount { get; set; }
    }

    public sealed class DashboardRecentContactDto
    {
        public long Id { get; set; }
        public string FormName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
    }
}
