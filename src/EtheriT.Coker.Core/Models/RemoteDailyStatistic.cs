namespace EtheriT.Coker.Core.Models
{
    public sealed class RemoteDailyStatistic
    {
        public long Id { get; set; }
        public DateTime StatisticDate { get; set; }
        public long FK_WebsiteId { get; set; }
        public byte Scope { get; set; }
        public long FK_WebmenuId { get; set; }
        public long FK_ArticleId { get; set; }
        public long FK_ProdId { get; set; }
        public long FK_TechCertId { get; set; }
        public long PageViews { get; set; }
        public long EffectiveViews { get; set; }
        public long LegacyViews { get; set; }
        public long UniqueVisitors { get; set; }
        public long EffectiveUniqueVisitors { get; set; }
        public long TotalVisibleSeconds { get; set; }
        public DateTime AggregatedAt { get; set; }
    }
}
