using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class BonusLog
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public int Amount { get; set; }
        public string Note { get; set; }
        public FrontUser User { get; set; }
        public DateTime ExecutionTime { get; set; } = DateTime.Now;
    }
}
