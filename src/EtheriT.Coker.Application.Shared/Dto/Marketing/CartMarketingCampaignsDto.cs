using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CartMarketingCampaignsDto
    {
        public List<CartMarketingCampaignDto> OrderDiscounts { get; set; } = new();
    }
}
