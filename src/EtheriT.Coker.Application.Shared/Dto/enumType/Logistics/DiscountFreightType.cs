using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.Logistics
{
    public enum DiscountFreightType
    {
        指定折抵後運費 = 1,   // 達門檻後，運費直接改成某個值
        折抵固定運費金額 = 2   // 達門檻後，從原箱運費扣掉某個值
    }
}
