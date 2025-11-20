using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Order.Dto
{
    public class DetailBuildResult
    {
        public List<Core.Models.ShoppingCart> ShoppingCarts { get; set; } = new();
        public Dictionary<long, Prod_Stock> StockDict { get; set; } = new();
        public int Subtotal { get; set; }              // 金額小計
        public int TotalBonus { get; set; }            // 總紅利
    }
}
