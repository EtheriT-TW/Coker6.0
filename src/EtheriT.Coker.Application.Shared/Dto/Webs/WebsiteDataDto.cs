using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    public class WebsiteDataDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string OrgName { get; set; }
        public string Logo { get; set; }
        public bool isSubsite { get; set; }
    }
}
