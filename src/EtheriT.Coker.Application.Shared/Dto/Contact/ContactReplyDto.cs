using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Contact
{
    public class ContactReplyDto
    {
        public long Id { get; set; }
        public ContactStatusEnum Status { get; set; }
        public string Reply { get; set; } = string.Empty;
    }
}
