using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Token
{
    public interface ITokenAppService
    {
        public Task<string> CreateToken(string account,Guid secret);
        public Task<bool> DelToken();
        public Task<TokenResponseDto> CreateToken();
        public Task<TokenResponseDto> CheckToken(string id);
    }
}
