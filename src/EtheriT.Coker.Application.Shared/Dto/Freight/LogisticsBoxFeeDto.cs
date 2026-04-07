using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class LogisticsBoxFeeDto
    {
        public long? Id { get; set; }
        public long FK_LogisticsBoxId { get; set; }
        public decimal Fee { get; set; }
        public string? Name { get; set; }
    }
}
