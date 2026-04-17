using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductSummaryPriceDto
    {
        /// <summary>實際顯示用金額（現金部分）</summary>
        public decimal? Price { get; set; }

        /// <summary>紅利點數</summary>
        public int Bonus { get; set; }

        /// <summary>原價（若有）</summary>
        public decimal? OriPrice { get; set; }

        /// <summary>建議售價</summary>
        public decimal? SuggestPrice { get; set; }

        /// <summary>是否為時價（顯示「時價」）</summary>
        public bool IsTimePrice { get; set; }

        /// <summary>是否為會員價</summary>
        public bool IsMemberPrice { get; set; }

        /// <summary>基準角色（例如：非會員）</summary>
        public string? BaseRoleName { get; set; }

        /// <summary>目前身份角色名稱（例如：VIP）</summary>
        public string? CurrentRoleName { get; set; }

        /// <summary>已格式化顯示文字（可選，前端可直接用）</summary>
        public string? PriceDisplayText { get; set; }
    }
}
