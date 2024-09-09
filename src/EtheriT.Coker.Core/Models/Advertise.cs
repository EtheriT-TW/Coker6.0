using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Advertise : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long Type { get; set; }
        [StringLength(100)] public string? Img { get; set; }
        public int SerNO { get; set; }
        public bool Visible { get; set; }
        [StringLength(300)] public string Title { get; set; }
        public bool Target { get; set; }
        [StringLength(255)] public string? Link { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool Permanent { get; set; }
        public Website? Website { get; set; }
    }
}
