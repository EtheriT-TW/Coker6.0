using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion.Auth
{
    public class LoginOptionsDto
    {
        public bool LineEnabled { get; set; }
        public bool GoogleEnabled { get; set; }
        public bool FacebookEnabled { get; set; }
        public bool AppleEnabled { get; set; }
    }
}
