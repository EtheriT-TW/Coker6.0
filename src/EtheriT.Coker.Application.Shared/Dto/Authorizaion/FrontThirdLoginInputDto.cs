using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class FrontThirdLoginInputDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public long FK_WebsiteId { get; set; }
        public bool SendWelcomeMail { get; set; } = true;
        public bool SendActivationMail { get; set; } = false;
    }
}
