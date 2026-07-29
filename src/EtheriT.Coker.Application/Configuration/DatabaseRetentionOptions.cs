namespace EtheriT.Coker.Application.Configuration
{
    public sealed class DatabaseRetentionOptions
    {
        public int AuditLogRetentionDays { get; set; } = 365;
        public int AuditLogBatchSize { get; set; } = 500;
        public int AuditLogMaxRowsPerRun { get; set; } = 10_000;
        public int TokenBatchSize { get; set; } = 1_000;
        public int TokenMaxRowsPerRun { get; set; } = 20_000;
        public int OrphanCartMaxRowsPerRun { get; set; } = 5_000;
        public int RemoteRetentionDays { get; set; } = 90;
        public int RemoteBatchSize { get; set; } = 2_000;
        public int RemoteMaxRowsPerRun { get; set; } = 50_000;
        public int CommandTimeoutSeconds { get; set; } = 120;
    }
}
