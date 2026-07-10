using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.User;

namespace EtheriT.Coker.Application.Authorization
{
    public interface IBackstageAccountAppService
    {
        Task<LoginOutputDto> Login(LoginInputDto dto);
        Task<UserDto> GetCurrentUser();
        Task<LoginOutputDto> Chech();
        Task<ResponseMessageDto> Logout();
        Task<ResponseMessageDto> UpdatePassword(UpdatePasswordDto dto);
        Task<ResponseUserEditDto> GetEditUser(DataDelectDto dto);
        Task<ResponseMessageDto> AddUser(AddUser dto);
        Task<ResponseMessageDto> SendForget(long userId);
    }
}
