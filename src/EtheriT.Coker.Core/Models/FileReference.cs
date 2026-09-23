using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class FileReference : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long? FK_FileUploadId { get; set; }
        [StringLength(80)] public string SourceType { get; set; } = string.Empty;
        public long SourceId { get; set; }
        [StringLength(30)] public string SourceState { get; set; } = string.Empty;
        [StringLength(80)] public string SourceField { get; set; } = string.Empty;
        [StringLength(1000)] public string NormalizedPath { get; set; } = string.Empty;
        public int OccurrenceCount { get; set; }
        public bool PhysicalFileExists { get; set; }
        public DateTime LastConfirmedTime { get; set; }
    }
}
