using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Newsletter
{
    public class NewsletterFrameDto
    {
        public long Id { get; set; }
        public List<NewsletterLinkDto> MainManu { get; set; } = new List<NewsletterLinkDto>();
        public NewsletterImageDto Logo { get; set; }
        public NewsletterImageDto LogoCompress { get; set; }
        public int No { get; set; }
        public string Title { get; set; }
        public string BGColor { get; set; } = "#2959b4";
        public NewslatterContenDto? Conten1 { get; set; }
        public NewslatterContenDto? News { get; set; }
        public NewslatterContenDto? Active { get; set; }
        public NewslatterContenDto? Conten2 { get; set; }
        public NewslatterContenDto? Conten3 { get; set; }
        public NewslatterContenDto? Footer { get; set; }
    }
}
