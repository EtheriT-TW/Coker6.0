using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CartMarketingCampaignDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = "";

        public MarketingCampaignTypeEnum CampaignType { get; set; }

        public int Priority { get; set; }

        public bool CanStack { get; set; }

        public bool Repeatable { get; set; }

        public List<CartMarketingRuleDto> Rules { get; set; } = new();
    }
}
