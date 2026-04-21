using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetFrontUserBonusHistoryItemDto
    {
        public long Id { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int AddBonus { get; set; }
        public int RemainBonus { get; set; }
        public string Note { get; set; } = string.Empty;
        public List<GetFrontUserBonusUsageLogDto> UseLogs { get; set; } = new();
    }
}
