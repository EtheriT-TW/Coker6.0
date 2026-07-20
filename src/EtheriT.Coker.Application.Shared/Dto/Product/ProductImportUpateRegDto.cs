using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    /// <summary>
    /// 商品 Excel 原始列。Price 保留為文字，讓「時價」能先通過讀檔階段，
    /// 再由 mapping 轉換成 ProductImportDto 的數值價格與時價旗標。
    /// </summary>
    public class ProductImportUpateRegDto: ProductImportUpateDto
    {
        public string Price { get; set; } = string.Empty;
    }
}
