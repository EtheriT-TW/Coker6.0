using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    public class WebSiteOrgNameDto
    {
        public long Id { get; set; }
        public string OrgName { get; set; }
        public WebsiteLevelEnum Level { get; set; }
    }
}
