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
        public int UseBonus { get; set; }
    }
}
