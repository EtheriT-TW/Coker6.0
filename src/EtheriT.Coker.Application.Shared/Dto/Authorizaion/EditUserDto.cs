using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class EditUserDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string TelPhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

    }
}
