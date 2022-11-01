using EtheriT.Coker.Application.Authorizaion.Dto;

namespace EtheriT.Coker.Application.Authorization
{
    public interface IAccountAppService
    {
        public Task<LoginOutputDto> Login(LoginInputDto dto);
    }
}