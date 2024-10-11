using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class FrontEditUserDto : FrontAddUserDto
    {
        public Guid UUID { get; set; }
        public string Account { get; set; }
        public string Nickname { get; set; }
        public int sex { get; set; }
        public string TelPhone { get; set; }
        public string CelPhone { get; set; }
        public string Address { get; set; }
        public DateTime Birthday { get; set; }
    }
}
