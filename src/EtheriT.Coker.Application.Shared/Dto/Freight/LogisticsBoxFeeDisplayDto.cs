using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class LogisticsBoxFeeDisplayDto
    {
        public long LogisticsBoxId { get; set; }
        public string Name { get; set; }
        public int CapacityPoint { get; set; }
        public decimal Fee { get; set; }
    }
}
