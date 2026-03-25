using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class PayOrderItem
    {
        public string ItemId { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; } = "";
        public decimal OriginalUnitPrice { get; set; }
        public int PayUnitPrice { get; set; }
        public decimal OriginalLineAmount { get; set; }
        public int PayLineAmount { get; set; }
        public bool IsShipping { get; set; }
    }
}
