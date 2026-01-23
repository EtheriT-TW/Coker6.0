using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class BonusLiability
    {
        public Guid UUID { get; set; }
        public int OutstandingPoints { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
