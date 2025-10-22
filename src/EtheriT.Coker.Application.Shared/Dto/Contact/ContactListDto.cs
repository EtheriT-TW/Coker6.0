using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Contact
{
    public class ContactListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string TargetEmail { get; set; }
        public string Email { get; set; }
        public DateTime CreationTime {  get; set; }

    }
}
