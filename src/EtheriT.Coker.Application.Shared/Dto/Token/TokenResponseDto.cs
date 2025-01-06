using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Token
{
    public class TokenResponseDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Token { get; set; }
        public Guid? RefreshToken { get; set; }
        public bool IsLogin {  get; set; }
        public string? name { get; set; }
        public bool AgreePrivacy { get; set; }
    }
}
