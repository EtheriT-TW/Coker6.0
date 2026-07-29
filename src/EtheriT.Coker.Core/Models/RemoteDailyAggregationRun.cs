namespace EtheriT.Coker.Core.Models
{
    public sealed class RemoteDailyAggregationRun
    {
        public long Id { get; set; }
        public DateTime StatisticDate { get; set; }
        public int AggregationVersion { get; set; }
        public long SourceRows { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
