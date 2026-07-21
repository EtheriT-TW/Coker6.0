using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// Tracks the resumable PageText rebuild for one website and content type.
    /// </summary>
    public class PageTextBackfillState : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }

        [StringLength(20)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public long TargetMaxId { get; set; }
        public long LastProcessedId { get; set; }
        public int TotalCount { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public int RemainingNullCount { get; set; }

        [MaxLength]
        public string? FailedIdsJson { get; set; }

        [StringLength(4000)]
        public string? LastError { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
    }
}
