using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetQueryFrontUsersBonusLogOutput
    {
        public int Amount { get; set; }
        public string? Note { get; set; }
        public DateTime ExecuteTime { get; set; }
        public DateTime? ExpireTime { get; set; }
    }
}
