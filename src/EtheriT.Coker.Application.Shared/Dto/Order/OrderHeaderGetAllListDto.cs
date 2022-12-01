using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderHeaderGetAllListDto
    {
        public long Id { get; set; }
        public string Orderer { get; set; }
        public string RecipientAddress { get; set; }
        public int Shipping { get; set; }
        public int Payment { get; set; }
        public int State { get; set; }
        public int Total { get; set; }
        public virtual DateTime CreationTime { get; set; }

    }
}
