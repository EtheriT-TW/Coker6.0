using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.CodeParser;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Web.MVC.Models.BonusManagement;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class BonusManagementController : Controller
    {
        private readonly IBonusManagementAppService _bonusManagementAppService;
        private readonly IMapper mapper;
        public BonusManagementController(IBonusManagementAppService bonusManagementAppService,
                                         IMapper mapper)
        {
            _bonusManagementAppService = bonusManagementAppService;
            this.mapper = mapper;
        }
        public async Task<IActionResult> Settings()
        {
            var bonusSettingData = await _bonusManagementAppService.GetBonusSettingForEdit();
            var bonusSettingHelpText = await _bonusManagementAppService.GetBonusSettingHelpTextForEdit();

            SettingsViewModel model = new SettingsViewModel()
            {
                // 取得紅利設定
                SiteId = bonusSettingData.SiteId,
                SignupBonusPoints = bonusSettingData.SignupBonusPoints,
                MinOrderForRedemption = bonusSettingData.MinOrderForRedemption,
                MaxRedemptionPercent = bonusSettingData.MaxRedemptionPercent,
                MinOrderForEarnPoints = bonusSettingData.MinOrderForEarnPoints,
                RewardRatePercent = bonusSettingData.RewardRatePercent,
                RewardPointsExpireDays = bonusSettingData.RewardPointsExpireDays,

                // 取得紅利設定說明
                SignupBonusPointsHelpText = bonusSettingHelpText.SignupBonusPointsHelpText,
                MinOrderForRedemptionHelpText = bonusSettingHelpText.MinOrderForRedemptionHelpText,
                MaxRedemptionPercentHelpText = bonusSettingHelpText.MaxRedemptionPercentHelpText,
                MinOrderForEarnPointsHelpText = bonusSettingHelpText.MinOrderForEarnPointsHelpText,
                RewardRatePercentHelpText = bonusSettingHelpText.RewardRatePercentHelpText,
                RewardPointsExpireDaysHelpText = bonusSettingHelpText.RewardPointsExpireDaysHelpText
            };
            return View(model);
        }

        public async Task<IActionResult> Transaction()
        {
            var bonusSettingData = await _bonusManagementAppService.GetBonusSettingForEdit();

            TransactionViewModel model = new TransactionViewModel();
            if (bonusSettingData.RewardPointsExpireDays.HasValue)
            {
                model.ConstRewardPointsExpireDays = bonusSettingData.RewardPointsExpireDays.Value.ToString();
                model.ConstRewardPointsExpireDateTime = DateTime.Now.AddDays(bonusSettingData.RewardPointsExpireDays.Value);
            }
            else
            {
                model.ConstRewardPointsExpireDays = "無限制";
                model.ConstRewardPointsExpireDateTime = DateTime.Parse("2099/12/13 23:59:59");
            }

            return View(model);
        }

        public IActionResult Record()
        {
            return View();
        }
    }
}
