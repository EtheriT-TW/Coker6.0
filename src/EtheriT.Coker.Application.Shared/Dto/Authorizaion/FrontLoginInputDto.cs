using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorizaion.Dto
{
    public class FrontLoginInputDto
    {
        public string Email { get; set; }
        public long WebsiteId { get; set; }
        public string Password { get; set; }
    }
}
