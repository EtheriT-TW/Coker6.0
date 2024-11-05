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
        public Task<string> CreateToken(string account,Guid secret, int expireMinutes = 30);
        public Task<bool> DelToken();
        public Task<TokenResponseDto> CreateToken();
        public TokenResponseDto CheckToken();
        public Task<TokenResponseDto> RefreshToken(Guid? id);
        public Task<Guid> GetUUID();
		public Task<List<Guid>> GetAllUUID(Guid UUID);
		public Task<TokenKeyItem> NewToken(string? Accont = null, Guid? UUID = null, long? UserId = null);
    }
}
