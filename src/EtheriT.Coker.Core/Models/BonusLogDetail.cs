using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class BonusLogDetail
    {
        public long FK_BonusId { get; set; }
        public long FK_BonusLogsId { get; set; }
        public long UsedAmount { get; set; }
        [ForeignKey(nameof(FK_BonusId))]
        public Bonus Bonus { get; set; }
        [ForeignKey(nameof(FK_BonusLogsId))]
        public BonusLog BonusLog { get; set; }
    }
}
