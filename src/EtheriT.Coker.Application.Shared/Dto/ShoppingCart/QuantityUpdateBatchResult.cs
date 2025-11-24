using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class QuantityUpdateBatchResult: ResponseObject
    {
        public List<QuantityUpdateItemResult> Items { get; set; } = new();
        public bool HasChange => Items.Any(x => x.OldQuantity != x.NewQuantity || x.Removed);
    }
}
