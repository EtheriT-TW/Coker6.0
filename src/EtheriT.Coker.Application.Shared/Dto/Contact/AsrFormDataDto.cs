using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Contact
{
    public class AsrFormDataDto: ResponseObject
    {
        public long Id {  get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string TargetEmail { get; set; }
        public string Html { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
