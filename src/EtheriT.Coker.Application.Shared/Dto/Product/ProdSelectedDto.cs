using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdSelectedDto
    {
        public long? Id { get; set; }
        public long FK_ProdId { get; set; }
        public bool IsDeleted { get; set; }
        public string prod_Name { get; set; } = string.Empty;
    }
}
