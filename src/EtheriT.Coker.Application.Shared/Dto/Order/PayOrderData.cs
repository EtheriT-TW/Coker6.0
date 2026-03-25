using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class PayOrderData
    {
        public string OrderId { get; set; } = "";
        public int PayableAmount { get; set; }
        public string Currency { get; set; } = "TWD";
        public DateTime PaymentTime { get; set; } = DateTime.Now;
        public List<PayOrderItem> Items { get; set; } = new();
        public List<PayOrderAdjustment> Adjustments { get; set; } = new();
    }
}
