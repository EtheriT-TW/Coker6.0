using EtheriT.Coker.Application.Shared.Dto.BonusManagement;

namespace EtheriT.Coker.Web.MVC.Models.BonusManagement
{
    public class SettingsViewModel : GetBonusSettingForEditOutput
    {
        public string? SignupBonusPointsHelpText { get; set; }

        public string? MinOrderForRedemptionHelpText { get; set; }

        public string? MaxRedemptionPercentHelpText { get; set; }

        public string? MinOrderForEarnPointsHelpText { get; set; }

        public string? RewardRatePercentHelpText { get; set; }

        public string? RewardPointsExpireDaysHelpText { get; set; }
    }
}
