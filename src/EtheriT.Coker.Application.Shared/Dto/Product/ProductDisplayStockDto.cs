using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductDisplayStockDto
    {
        public long Id { get; set; }
        public long Pid { get; set; }
        public long? FK_ST1id { get; set; }
        public long? FK_S1id { get; set; }
        public string? S1_Title { get; set; }
        public long? FK_ST2id { get; set; }
        public long? FK_S2id { get; set; }
        public string? S2_Title { get; set; }
        public int? Stock { get; set; }
        public int? Min_Qty { get; set; }
        public bool TimePrice { get; set; }
        public decimal SuggestPrice { get; set; }
        public List<ProductDisplayPriceDto> Prices { get; set; } = new();
    }
}
