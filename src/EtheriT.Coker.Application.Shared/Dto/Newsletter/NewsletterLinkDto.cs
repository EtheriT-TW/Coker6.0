using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Newsletter
{
    public class NewsletterLinkDto
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public bool Target { get; set; }
        public string Alert { get; set; }
        public string? Memo { get; set; }
        public string? Conten { get; set; }
        public DateTime? Date { get; set; }
        public string? Local { get; set; }
    }
}
