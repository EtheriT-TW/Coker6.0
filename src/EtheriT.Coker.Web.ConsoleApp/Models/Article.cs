using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models
{
    [Table("Article")]
    public class Article: FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(100)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [StringLength(50)]
        public bool Visible { get; set; }
        public int SerNO { get; set; } = 500;
        public int Popular { get; set; } = 0;
        public bool PopularVisible { get; set; } = true;
        public string? SaveHtml { get; set; }
        [MaxLength]
        public string? SaveCss { get; set; }
        [MaxLength]
        public string? Html { get; set; }
        [MaxLength]
        public string? Css { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public List<Tag_Associate>? Associates { get; set; }
    }
}
