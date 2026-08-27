using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class CreateOrUpdateSettingsDto
    {
        /// <summary>
        /// 是否啟用紅利功能
        /// </summary>
        public bool BonusEnabled { get; set; }
        /// <summary>
        /// 迎新禮 (加入會員贈送紅利的點數)
        /// </summary>
        public int? SignupBonusPoints { get; set; }

        /// <summary>
        /// 紅利扣抵條件 (消費滿X元可啟動紅利扣抵)
        /// </summary>
        public decimal? MinOrderForRedemption { get; set; }

        /// <summary>
        /// 最高抵扣% (達到啟動紅利扣抵條件後，每次最高抵扣%)
        /// </summary>
        public decimal? MaxRedemptionPercent { get; set; }

        /// <summary>
        /// 單筆訂單紅利折抵上限
        /// </summary>
        [Range(typeof(decimal), "1", "99999999", ErrorMessage = "最高折抵上限必須大於 0，或留空表示沒有限制")]
        public decimal? MaximumDiscount { get; set; }

        /// <summary>
        /// 消費滿額條件 (當次消費滿額X元可啟動獲得數紅利)
        /// </summary>
        public decimal? MinOrderForEarnPoints { get; set; }

        /// <summary>
        /// 獲得%數紅利 (消費滿額時，可獲得%數紅利)
        /// </summary>
        public decimal? RewardRatePercent { get; set; }

        /// <summary>
        /// 紅利回饋計算方式
        /// </summary>
        public BonusRewardCalculationTypeEnum RewardCalculationType { get; set; } = BonusRewardCalculationTypeEnum.Percent;

        /// <summary>
        /// 固定回饋點數
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "固定回饋點數必須大於 0")]
        public int? RewardFixedPoints { get; set; }

        /// <summary>
        /// 固定點數是否按消費門檻倍數累計
        /// </summary>
        public bool RewardFixedPointsCumulative { get; set; } = true;

        /// <summary>
        /// 紅利有效天數
        /// </summary>
        public int? RewardPointsExpireDays { get; set; }
    }
}
