using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.Bonus
{
    public enum BonusStatusEnum
    {
        Active = 1,   // 可用（但是否真的可用，還要看時間）
        UsedUp = 2,   // 已用完
        Revoked = 3   // 退貨 / 作廢 / 不成立
    }
}
