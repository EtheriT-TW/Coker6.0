using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class MemberOrderDto
    {
        public long Id { get; set; }
        public string OrderDate { get; set; }
        public string Payment { get; set; }
        public string OrderTotal { get; set; }
        public string Status { get; set; }
    }
}
