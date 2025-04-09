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
        public Task<string> CreateToken(string account,Guid secret, int expireMinutes = 30, string position = "");
        public Task<bool> DelToken();
        public Task<bool> IsTokenRevoked(string token);
        public Task<TokenResponseDto> CreateToken();
        public Task<TokenResponseDto> CheckToken(string? token);
        public Task<ResponseMessageDto> AgreePrivacy();
        public Task<TokenResponseDto> RefreshToken(Guid? id);
        public Task<Guid> GetUUID();
        public Guid GetUUID(Guid oldUUID);
        public Task<List<Guid>> GetAllUUID(Guid UUID);
		public Task<TokenKeyItem> NewToken(string? Accont = null, Guid? UUID = null, long? UserId = null);
    }
}
