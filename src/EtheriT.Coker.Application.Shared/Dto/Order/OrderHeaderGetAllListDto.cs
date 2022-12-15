using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderHeaderGetAllListDto
    {
        public string Id { get; set; }
        public string Orderer { get; set; }
        public string RecipientAddress { get; set; }
        public string Shipping { get; set; }
        public string Payment { get; set; }
        public string State { get; set; }
        public int Total { get; set; }
        public virtual DateTime CreationTime { get; set; }

    }
}
