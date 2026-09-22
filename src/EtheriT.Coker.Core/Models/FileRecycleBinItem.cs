using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class FileRecycleBinItem : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_FileUploadId { get; set; }
        public DateTime RecycledTime { get; set; }
        [StringLength(200)]
        public string OriginalPath { get; set; } = string.Empty;
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
