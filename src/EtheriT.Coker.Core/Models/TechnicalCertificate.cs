using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class TechnicalCertificate : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public bool Disp_opt { get; set; }
        [StringLength(100)] public string? Img { get; set; }
        [StringLength(300)] public string? Title { get; set; }
        [StringLength(250)] public string? Description { get; set; }
        public int Ser_no { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool Permanent { get; set; }
        public Website? Website { get; set; }
    }
}
