using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryPriceResultDto
    {
        public long ProductId { get; set; }
        public string? Price { get; set; }
        public decimal? PriceValue { get; set; }
        public string? OriPrice { get; set; }
        public string? Bonus { get; set; }
        public string? SuggestPrice { get; set; }
        public bool IsTimePrice { get; set; }
        public bool IsMemberPrice { get; set; }
        public bool HasBonusPrice { get; set; }
        public string? PriceDisplayText { get; set; }
        public string? BaseRoleName { get; set; }
        public string? CurrentRoleName { get; set; }
        /// <summary>這個價格取自哪個規格（Prod_Stock.Id）；商品頁用來預設選取，讓價格與目錄卡片一致</summary>
        public long? DisplayStockId { get; set; }
    }
}
