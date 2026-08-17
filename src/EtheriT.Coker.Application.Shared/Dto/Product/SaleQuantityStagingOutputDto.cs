using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class SaleQuantityStagingOutputDto
    {
        /// <summary>規格 Id，供列表批次更新庫存使用，不直接顯示。</summary>
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; }
        public string Specs { get; set; }
        public int SaleQuantity { get; set; }
        public int StockQuantity { get; set; }

    }
}
