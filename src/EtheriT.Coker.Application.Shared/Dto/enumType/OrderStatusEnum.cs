using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType
{
    public enum OrderStatusEnum
    {
        待確認 = 1,
        已付款 = 2,
        已出貨 = 3,
        已取消 = 4,
        付款失敗 = 5,
    }
}
