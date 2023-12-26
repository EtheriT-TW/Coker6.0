using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Newsletter
{
    public class NewslatterContenDto
    {
        public NewsletterImageDto? image { get; set; }
        public NewsletterImageDto? Icom { get; set; }
        public string Title { get; set; }
        public string MainTitle { get; set; }
        public List<string> Conten { get; set; } = new List<string>();
        public List<NewsletterLinkDto>? List { get; set; }
        public NewsletterLinkDto? more { get; set; }
    }
}
