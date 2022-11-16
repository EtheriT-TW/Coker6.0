using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Token
{
    public interface ITokenAppService
    {
        public string CreateToken(string account);
    }
}
