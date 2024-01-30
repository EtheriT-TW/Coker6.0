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
        public NewsletterImageDto? imageCompress { get; set; }
        public NewsletterImageDto? Icon { get; set; }
        public NewsletterImageDto? IconCompress { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? MainTitle { get; set; } = string.Empty;
        public bool? Visible { get; set; } = true;
        public string? Conten { get; set; } = string.Empty;
        public List<NewsletterLinkDto>? List { get; set; }
        public NewsletterLinkDto? More { get; set; }
    }
}
