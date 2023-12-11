using EtheriT.Coker.Application.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    public class WebsiteEditDto
    {
        public string? DefaultUrl { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Contact { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
