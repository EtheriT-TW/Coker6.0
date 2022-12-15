using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.EnterAd
{
    public class HtmlContentDto
    {
        public long Id { get; set; }
        public long FK_WebsiteId { get; set; }
        public Guid TId { get; set; }
        public int Type { get; set; }
        [StringLength(100)] public string? Img { get; set; }
        public string? Html { get; set; }
        public string? css { get; set; }
        public int Ser_no { get; set; }
        public bool Disp_opt { get; set; }
        public int ObjectType { get; set; }
        [StringLength(300)] public string? Title { get; set; }
        public bool Target { get; set; }
        [StringLength(255)] public string? Link { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool permanent { get; set; }
    }
}
