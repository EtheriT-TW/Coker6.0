using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.User;

namespace EtheriT.Coker.Application.Authorization
{
    public sealed class BackstageAccountAppService : IBackstageAccountAppService
    {
        private readonly AccountAppService core;

        public BackstageAccountAppService(AccountAppService core)
        {
            this.core = core;
        }

        public Task<LoginOutputDto> Login(LoginInputDto dto) => core.Login(dto);
        public Task<UserDto> GetCurrentUser() => core.GetCurrentUser();
        public Task<LoginOutputDto> Chech() => core.Chech();
        public Task<ResponseMessageDto> Logout() => core.Logout();
        public Task<ResponseMessageDto> UpdatePassword(UpdatePasswordDto dto) => core.UpdatePassword(dto);
        public Task<ResponseUserEditDto> GetEditUser(DataDelectDto dto) => core.GetEditUser(dto);
        public Task<ResponseMessageDto> AddUser(AddUser dto) => core.AddUser(dto);
        public Task<ResponseMessageDto> SendForget(long userId) => core.SendForget(userId);
    }
}
