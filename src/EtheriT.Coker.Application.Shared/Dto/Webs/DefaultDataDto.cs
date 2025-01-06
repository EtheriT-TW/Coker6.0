using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
	public class DefaultDataDto
    {
        public long Id { get; set; }
        public string OrgName { get; set; }
        public string ParntOrgNames { get; set; }
        public int Layout_Type { get; set; }
        public string View { get; set; }
        public string Description { get; set; }
        public string Root {  get; set; }
        public string Css {  get; set; }

		public string locale { get; set; } = "zh-tw";
        public WebsiteLevelEnum Level { get; set; }
	}
}
