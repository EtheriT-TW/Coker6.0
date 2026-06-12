using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CartMarketingRuleDto
    {
        public long Id { get; set; }

        public MarketingRuleTypeEnum RuleType { get; set; }

        public MarketingScopeTypeEnum ScopeType { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal? MaxDiscountAmount { get; set; }
    }
}
