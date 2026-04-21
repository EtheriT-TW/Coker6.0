using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetFrontUserBonusHistoryOutput
    {
        public bool Success { get; set; } = true;
        public int Page_Total { get; set; }
        public List<GetFrontUserBonusHistoryItemDto> Data { get; set; } = new();
    }
}
