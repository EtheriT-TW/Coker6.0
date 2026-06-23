using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Contact
{
    public class ContactBatchUpdateStatusDto
    {
        public List<long> Ids { get; set; } = new List<long>();
        public ContactStatusEnum Status { get; set; }
    }
}
