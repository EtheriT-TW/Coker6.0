using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.EnterAd
{
    public class HtmlContentGetAllListDto
    {
        public long Id { get; set; }
        public long Type { get; set; }
        public string? Title { get; set; }
        public string? Img { get; set; }
        public string? Html { get; set; }
        public string? Css { get; set; }
        public int Ser_no { get; set; }
        public bool Disp_opt { get; set; }
        public int ObjectType { get; set; }
        public bool Target { get; set; }
        public string? Link { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool permanent { get; set; }
    }
}
