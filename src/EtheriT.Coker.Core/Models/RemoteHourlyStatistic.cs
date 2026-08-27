namespace EtheriT.Coker.Core.Models
{
    public sealed class RemoteHourlyStatistic
    {
        public long Id { get; set; }
        public DateTime StatisticHour { get; set; }
        public long FK_WebsiteId { get; set; }
        public long PageViews { get; set; }
        public long EffectiveViews { get; set; }
        public long LegacyViews { get; set; }
        public long UniqueVisitors { get; set; }
        public long EffectiveUniqueVisitors { get; set; }
        public long TotalVisibleSeconds { get; set; }
        public DateTime AggregatedAt { get; set; }
    }
}
