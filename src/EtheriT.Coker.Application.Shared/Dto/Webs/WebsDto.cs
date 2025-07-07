using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Webs.Dto
{
    public class WebsDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string OrgName { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public string DefaultUrl { get; set; }
        public bool Check { get; set; }
    }
}
