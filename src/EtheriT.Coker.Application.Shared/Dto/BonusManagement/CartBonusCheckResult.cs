using EtheriT.Coker.Application.Shared.i18n;
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
                    return L.get("BonusNotEnoughExceeded", AvailableBonus, CurrentCartBonus, CurrentCartBonus - AvailableBonus);
                }

                return L.get("BonusNotEnoughShortfall", AvailableBonus, CurrentCartBonus, IncrementBonus, ShortfallBonus);
            }
        }
    }
}
