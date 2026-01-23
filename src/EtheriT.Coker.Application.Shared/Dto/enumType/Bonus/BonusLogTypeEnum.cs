using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.Bonus
{
    public enum BonusLogTypeEnum
    {
        Unknown = 0,  // 舊資料或未判別
        Earn = 1,     // 發放（非訂單也算）
        Redeem = 2,   // 折抵
        Refund = 3,   // 取消/退貨退回折抵
        Adjust = 4,    // 人工/系統調整（含追回、補扣）
        EarnRevoke = 5 // 發放撤銷（主要用於訂單取消/退貨追回紅利）
    }
}
