using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class CheckRedirectUrlOutputDto
    {
        public long FK_WebsiteId { get; set; } = 0;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
