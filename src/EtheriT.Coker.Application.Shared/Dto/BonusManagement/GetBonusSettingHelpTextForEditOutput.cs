using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetBonusSettingHelpTextForEditOutput
    {
        public string? SignupBonusPointsHelpText { get; set; }

        public string? MinOrderForRedemptionHelpText { get; set; }

        public string? MaxRedemptionPercentHelpText { get; set; }

        public string? MinOrderForEarnPointsHelpText { get; set; }

        public string? RewardRatePercentHelpText { get; set; }

        public string? RewardPointsExpireDaysHelpText { get; set; }
    }
}
