using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class BonusLogDetail
    {
        public long FK_BonusId { get; set; }
        public long FK_BonusLogs { get; set; }
        public long UsedAmount { get; set; }
        public Bonus Bonus { get; set; }
        public BonusLog BonusLog { get; set; }
    }
}
