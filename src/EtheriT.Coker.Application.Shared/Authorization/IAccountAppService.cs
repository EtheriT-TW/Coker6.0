using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.User;

namespace EtheriT.Coker.Application.Authorization
{
    public interface IAccountAppService
    {
        public Task<LoginOutputDto> Login(LoginInputDto dto);
        public Task<LoginOutputDto> FrontLogin(FrontLoginInputDto dto);
        public Task<LoginOutputDto> FrontLogout();
        public Task<ResponseMessageDto> AccountOpening(Guid OpenId);
        public Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto);
        public Task<UserDto> GetCurrentUser();
        public Task<LoginOutputDto> Chech();
        public Task<ResponseMessageDto> Logout();
		public Task<ResponseMessageDto> UpdatePassword(UpdatePasswordDto dto);
        public Task<ResponseUserEditDto> GetEditUser(DataDelectDto dto);
        public Task<ResponseMessageDto> AddUser(AddUser dto);
        public Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto);
    }
}