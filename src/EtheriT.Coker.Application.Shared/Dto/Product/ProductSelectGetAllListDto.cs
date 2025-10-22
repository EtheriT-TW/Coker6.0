using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductSelectGetAllListDto: ProductGetAllListDto
    {
        public bool IsSelected { get; set; }
        public string TagNames { get; set; } = string.Empty;
    }
}
