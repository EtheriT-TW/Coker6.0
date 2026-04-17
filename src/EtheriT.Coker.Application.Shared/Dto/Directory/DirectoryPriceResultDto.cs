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
        public string? OriPrice { get; set; }
        public string? Bonus { get; set; }
        public string? SuggestPrice { get; set; }
        public bool IsTimePrice { get; set; }
        public bool IsMemberPrice { get; set; }
        public string? PriceDisplayText { get; set; }
        public string? BaseRoleName { get; set; }
        public string? CurrentRoleName { get; set; }
    }
}
