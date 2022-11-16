using EtheriT.Coker.Web.MVC.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Token
{
    public class TokenAppService: ITokenAppService
    {
        private readonly JwtHelpers jwt;
        public TokenAppService(JwtHelpers jwt) {
            this.jwt = jwt;
        }
        public string CreateToken(string account) { 
            return jwt.GenerateToken(account);
        }
    }
}
