using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Token
{
    public interface ITokenAppService
    {
        public Task<ResponseMessageDto> CreateToken();
        public Task<string> CreateToken(string account);
        public Task<bool> DelToken();
    }
}
