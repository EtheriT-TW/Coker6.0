using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class PasswordChageDto
    {
        public Guid ForgetID { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public long WebsiteId { get; set; }
    }
}
