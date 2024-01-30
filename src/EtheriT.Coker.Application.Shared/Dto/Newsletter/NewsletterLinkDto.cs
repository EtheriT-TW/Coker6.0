using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Newsletter
{
    public class NewsletterLinkDto
    {
        public string link { get; set; }
        public string? title { get; set; }
        public string? className { get; set; }
        public bool target { get; set; }
        public string? alert { get; set; }
        public string? memo { get; set; }
        public string? conten { get; set; }
        public DateTime? date { get; set; }
        public string? local { get; set; }
    }
}
