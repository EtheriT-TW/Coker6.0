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
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class DashboardTrafficItemDto
    {
        public DateTime Date { get; set; }
        public long PageViews { get; set; }
        public long Visitors { get; set; }
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
        public long PendingCount { get; set; }
        public List<DashboardContactFormSummaryDto> Forms { get; set; } = new();
        public List<DashboardRecentContactDto> Recent { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
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
