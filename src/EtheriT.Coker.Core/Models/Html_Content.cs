using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class Html_Content : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long? Menu_id { get; set; }
        public int Type { get; set; }
        [StringLength(100)] public string? Img { get; set; }
        [MaxLength] public string? Html { get; set; }
        [MaxLength] public string? Css { get; set; }
        public int Ser_no { get; set; }
        public bool Disp_opt { get; set; }
        public int ObjectType { get; set; }
        [StringLength(300)] public string? Title { get; set; }
        public bool Target { get; set; }
        [StringLength(255)] public string? Link { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool permanent { get; set; }
        public Website? Website { get; set; }
    }
}
