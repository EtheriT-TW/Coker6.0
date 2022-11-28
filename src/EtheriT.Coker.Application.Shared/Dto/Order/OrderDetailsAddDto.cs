using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailsAddDto
    {
        public long FK_OrderId { get; set; }
        public long FK_ProductId { get; set; }
        public int Amount { get; set; }
        public double Subtotal { get; set; }
        public int? Bonus { get; set; }

    }
}
