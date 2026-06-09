using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class MarketingOptionsDto
    {
        public List<LookUpItemDto> CampaignTypes { get; set; } = new();

        public List<LookUpItemDto> RuleTypes { get; set; } = new();

        public List<LookUpItemDto> DisplayStatuses { get; set; } = new();

        public List<LookUpItemDto> EditableStatuses { get; set; } = new();
    }
}
