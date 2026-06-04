using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetFrontUserBonusUsageLogDto
    {
        public DateTime? CreationTime { get; set; }

        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 顯示用異動點數：
        /// Redeem = 負數
        /// Refund = 正數
        /// </summary>
        public int UseBonus { get; set; }

        /// <summary>
        /// 原始紅利紀錄類型
        /// </summary>
        public BonusLogTypeEnum Type { get; set; }

        /// <summary>
        /// 是否為返還紀錄
        /// </summary>
        public bool IsRefund => Type == BonusLogTypeEnum.Refund;

        /// <summary>
        /// 前台顯示用類型名稱
        /// </summary>
        public string TypeName => Type switch
        {
            BonusLogTypeEnum.Refund => "返還",
            BonusLogTypeEnum.Redeem => "使用",
            _ => Type.ToString()
        };
    }
}
