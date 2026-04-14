using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Order.Dto
{
    public class FreightCalcResult
    {
        public int Freight { get; set; }
        public int PackingPointTotal { get; set; }
        public List<BoxUsageResult> BoxUsages { get; set; } = new();
    }
}
