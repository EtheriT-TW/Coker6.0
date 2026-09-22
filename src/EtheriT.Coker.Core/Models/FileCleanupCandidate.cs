using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class FileCleanupCandidate : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_FileUploadId { get; set; }
        public DateTime FirstDetectedTime { get; set; }
        public DateTime LastConfirmedTime { get; set; }
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
