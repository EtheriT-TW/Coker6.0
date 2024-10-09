using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Token
{
    public class TokenKeyItem
    {
        public string AccessToken { get; set; } = string.Empty;
        public Guid RefreshToken { get; set; }
        public Guid UUID { get; set; }
        public bool IsLogin {  get; set; }
    }
}
