using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorizaion.Dto
{
    public class LoginOutputDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
