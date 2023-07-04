using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Directory : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(60)] public string? Title { get; set; }
        [StringLength(150)] public string? Description { get; set; }
        public int Type { get; set; }
        public long? FK_Mid { get; set; }
        public bool Visible { get; set; }
        public Website? Website { get; set; }
    }
}
