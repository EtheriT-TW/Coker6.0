using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
	public class Article : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(150)]
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        [StringLength(50)]
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; } = 500;
        public int Popular { get; set; } = 0;
        public bool PopularVisible { get; set; } = true;
        public bool RemovedFromShelves { get; set; } = false;
        [MaxLength]
        public string? SaveHtml { get; set; }
        [MaxLength]
        public string? SaveCss { get; set; }
        [MaxLength]
        public string? Html { get; set; }
        [MaxLength]
        public string? Css { get; set; }
        [MaxLength]
        public string? NewsletterHtml { get; set; }
        [MaxLength]
        public string? NewsletterCss { get; set; }
        [MaxLength]
        public string? DataJson { get; set; }
        public virtual DateTime? NodeDate { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public Website? Website { get; set; }
		public List<Remote> Remotes { get; set; } = new List<Remote>();
	}
}
