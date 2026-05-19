using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductMarketingLabelDto
    {
        public ProductMarketingLabelTypeEnum Type { get; set; }

        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 前端樣式用，不讓前端硬判斷 enum 數字。
        /// </summary>
        public string CssClass { get; set; } = string.Empty;
    }
}
