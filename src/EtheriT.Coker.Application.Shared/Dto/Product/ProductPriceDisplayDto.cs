using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductPriceDisplayDto
    {
        public long Id { get; set; }
        public long FK_PSId { get; set; }
        public long FK_RId { get; set; }

        public decimal? Price { get; set; }
        public decimal? Bonus { get; set; }
        public decimal? OriPrice { get; set; }

        public string? RoleName { get; set; }
        public int RoleSerNo { get; set; }

        public bool IsBonusPrice { get; set; }
        public bool CanBuy { get; set; }
    }
}
