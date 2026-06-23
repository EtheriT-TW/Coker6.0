using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class RefundOrderData
    {
        public long OrderId { get; set; }

        public string OrderNo { get; set; } = "";

        public string TransactionId { get; set; } = "";

        public int RefundAmount { get; set; }
    }
}
