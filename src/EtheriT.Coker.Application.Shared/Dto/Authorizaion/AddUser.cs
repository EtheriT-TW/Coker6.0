using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class AddUser: EditUserDto
    {
        public string Password { get; set;}
        public string PasswordConfirm { get; set; }
    }
}
