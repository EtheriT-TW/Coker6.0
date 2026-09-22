namespace EtheriT.Coker.Application.Shared.FileManagement
{
    public class FileCleanupItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? DetectedTime { get; set; }
        public DateTime? DeletionTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool PhysicalFileExists { get; set; }
    }

    public class FileCleanupBatchResultDto
    {
        public int MovedCount { get; set; }
        public int SkippedCount { get; set; }
    }
}
