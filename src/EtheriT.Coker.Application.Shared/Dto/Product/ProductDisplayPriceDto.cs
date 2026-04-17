using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductDisplayPriceDto
    {
        public long PriceId { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int Bonus { get; set; }
        public decimal? OriPrice { get; set; }
        public decimal? SuggestPrice { get; set; }
        public bool IsBonusPrice { get; set; }
        public bool IsMemberPrice { get; set; }
        public bool IsSelectable { get; set; }
        public bool IsDefault { get; set; }
        public string? PriceDisplayText { get; set; }
    }
}
