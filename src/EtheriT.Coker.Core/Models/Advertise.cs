using EtheriT.Coker.Application.Shared.Dto.enumType.Advertise;
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
        public int Exposure { get; set; } = 0;
        public int Clicks { get; set; } = 0;
        [StringLength(300)] public string Title { get; set; }
        [StringLength(300)] public string? Describe { get; set; }
        public string? Html { get; set; }
        public string? Css { get; set; }
        public string? SaveHtml { get; set; }
        public string? SaveCss { get; set; }
        public bool Target { get; set; }
        [StringLength(255)] public string? Link { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool Permanent { get; set; }
        public AdvertiseActionType ActionType { get; set; } = AdvertiseActionType.Link;
        public List<Advertise_Log> Advertise_Logs { get; set; }
        public Website? Website { get; set; }
    }
}
