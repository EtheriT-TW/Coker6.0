using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class CartBonusCheckResult
    {
        public bool IsEnough { get; set; }
        public int AvailableBonus { get; set; }
        public int CurrentCartBonus { get; set; }
        public int IncrementBonus { get; set; }
        public int TotalNeededBonus { get; set; }
        public int ShortfallBonus => Math.Max(0, TotalNeededBonus - AvailableBonus);

        public bool CartAlreadyExceeded => CurrentCartBonus > AvailableBonus;

        public string Message
        {
            get
            {
                if (IsEnough) return string.Empty;

                if (CartAlreadyExceeded)
                {
                    return
                        $"會員紅利不足，目前可用 {AvailableBonus:N0} 點，" +
                        $"購物車已需 {CurrentCartBonus:N0} 點，" +
                        $"已超出 {CurrentCartBonus - AvailableBonus:N0} 點";
                }

                return
                    $"會員紅利不足，目前可用 {AvailableBonus:N0} 點，" +
                    $"購物車已需 {CurrentCartBonus:N0} 點，" +
                    $"本次新增需 {IncrementBonus:N0} 點，" +
                    $"尚差 {ShortfallBonus:N0} 點";
            }
        }
    }
}
