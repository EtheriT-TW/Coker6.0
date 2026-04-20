using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductStockDisplayDto
    {
        public long Pid { get; set; }
        public long Id { get; set; }

        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public int? FK_ST1id { get; set; }
        public int? FK_ST2id { get; set; }

        public string S1_Title { get; set; } = "";
        public string S2_Title { get; set; } = "";
        public string S1_Name { get; set; } = "";
        public string S2_Name { get; set; } = "";

        public bool TimePrice { get; set; }
        public decimal? Price { get; set; }
        public decimal? SuggestPrice { get; set; }
        public int? Min_Qty { get; set; }
        public int? Stock { get; set; }
        public int? PackingPoint { get; set; }
        public int? Alert_Qty { get; set; }
        public string SubItemNo { get; set; } = "";
        public int? Ser_No { get; set; }

        public List<ProductPriceDisplayDto> Prices { get; set; } = new();
    }
}
