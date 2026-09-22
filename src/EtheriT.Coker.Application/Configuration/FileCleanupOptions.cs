namespace EtheriT.Coker.Application.Configuration
{
    public class FileCleanupOptions
    {
        public int MinimumAgeDays { get; set; } = 14;
        public int RecycleBinRetentionDays { get; set; } = 30;
        public int MaxFilesPerScan { get; set; } = 10_000;
    }
}
