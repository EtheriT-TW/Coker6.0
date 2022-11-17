using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Authorization
{
    public interface IAccountAppService
    {
        public Task<LoginOutputDto> Login(LoginInputDto dto);
        public Task<UserDto> GetCurrentUser();
        public Task<LoginOutputDto> Chech();
        public Task<ResponseMessageDto> Logout();
    }
}