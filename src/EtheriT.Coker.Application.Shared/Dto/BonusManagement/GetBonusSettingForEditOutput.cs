using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetBonusSettingForEditOutput
    {
        public long SiteId { get; set; }

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
        /// 消費滿額條件 (當次消費滿額X元可啟動獲得數紅利)
        /// </summary>
        public decimal? MinOrderForEarnPoints { get; set; }

        /// <summary>
        /// 獲得%數紅利 (消費滿額時，可獲得%數紅利)
        /// </summary>
        public decimal? RewardRatePercent { get; set; }

        /// <summary>
        /// 紅利有效天數
        /// </summary>
        public int? RewardPointsExpireDays { get; set; }
    }
}
