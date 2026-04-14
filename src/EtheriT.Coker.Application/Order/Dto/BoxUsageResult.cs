using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Order.Dto
{
    public class BoxUsageResult
    {
        public long LogisticsBoxId { get; set; }
        public string Name { get; set; } = "";
        public int CapacityPoint { get; set; }
        public decimal Fee { get; set; }
        public int Count { get; set; }
    }
}
