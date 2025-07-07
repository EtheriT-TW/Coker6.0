using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.enumType.OAuth
{
    public class OAuthErrorInputDto
    {
        public OAuthErrorTypeEnum code { get; set; }
        public string? redirect { get; set; } = null;
    }
}
