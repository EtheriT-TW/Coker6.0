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
        public decimal Subtotal { get; set; }          // 金額小計
        public int TotalBonus { get; set; }            // 總紅利（點數，非金額）
        public decimal Freight { get; set; }
        public int PackingPointTotal { get; set; }     // 包材點數，非金額
        public decimal Discount { get; set; }
        public string? DiscountBreakdownJson { get; set; }
        public List<BoxUsageResult> BoxUsages { get; set; } = new();
    }
}
