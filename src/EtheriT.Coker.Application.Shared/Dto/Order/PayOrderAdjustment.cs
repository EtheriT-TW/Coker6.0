using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class PayOrderAdjustment
    {
        public string Type { get; set; } = ""; // Bonus, Coupon, Promotion
        public string Name { get; set; } = "";
        public int Amount { get; set; } // 正值表示折抵額
    }
}
