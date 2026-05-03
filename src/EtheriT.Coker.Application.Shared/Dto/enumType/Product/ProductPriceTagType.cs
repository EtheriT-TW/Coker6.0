using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.Product
{
    public enum ProductPriceTagType
    {
        MemberPrice = 1,   // 有會員價（不一定顯示金額）
        Bonus = 2,         // 有紅利方案
        Promotion = 3,     // 一般促銷（未來）
        Gift = 4,          // 贈品
        Discount = 5       // 折扣
    }
}
