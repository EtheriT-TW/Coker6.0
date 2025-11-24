using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class QuantityUpdateItemResult
    {
        public long CartId { get; set; }
        public bool Success { get; set; }
        public bool Removed { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
    }
}
