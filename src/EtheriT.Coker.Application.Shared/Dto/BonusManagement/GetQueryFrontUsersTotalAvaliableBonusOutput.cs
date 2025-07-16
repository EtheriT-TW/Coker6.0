using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetQueryFrontUsersTotalAvaliableBonusOutput
    {
        public Guid UserUUID { get; set; }
        public int TotalAvaliableBonus { get; set; }
    }
}
