using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Newsletter
{
    public class NewsletterFrameDto
    {
        public List<NewsletterLinkDto> MainManu { get; set; } = new List<NewsletterLinkDto>();
        public NewsletterImageDto Logo { get; set; }
        public int No { get; set; }
        public string Title { get; set; }
        public NewslatterContenDto? Conten1 { get; set; }
        public NewslatterContenDto? News { get; set; }
        public NewslatterContenDto? Active { get; set; }
        public NewslatterContenDto? Conten2 { get; set; }
    }
}
