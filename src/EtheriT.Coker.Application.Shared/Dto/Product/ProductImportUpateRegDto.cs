using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductImportUpateRegDto: ProductImportDto
    {
        public string Price { get; set; } = string.Empty;
    }
}
