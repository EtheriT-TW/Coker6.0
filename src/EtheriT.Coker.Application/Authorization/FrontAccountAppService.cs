using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;

namespace EtheriT.Coker.Application.Authorization
{
    public sealed class FrontAccountAppService : IFrontAccountAppService
    {
        private readonly AccountAppService core;

        public FrontAccountAppService(AccountAppService core)
        {
            this.core = core;
        }

        public Task<LoginOutputDto> FrontLogin(FrontLoginInputDto dto) => core.FrontLogin(dto);
        public Task<LoginOutputDto> FrontLoginByToken(Guid token) => core.FrontLoginByToken(token);
        public Task<LoginOutputDto> FrontThirdLogin(FrontThirdLoginInputDto dto) => core.FrontThirdLogin(dto);
        public Task<LoginOutputDto> FrontLogout() => core.FrontLogout();
        public Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto) => core.AddFrontUser(dto);
        public Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto) => core.FrontUserEdit(dto);
        public Task<ResponseUserEditDto> GetFrontUserData() => core.GetFrontUserData();
        public Task<string> GetFrontUserLevelName() => core.GetFrontUserLevelName();
        public Task<ResponseMessageDto> AccountOpening(Guid openId) => core.AccountOpening(openId);
        public Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto) => core.ReSendOpening(dto);
        public Task<ResponseMessageDto> SendForget(SendForgetDto dto) => core.SendForget(dto);
        public Task<ResponseMessageDto> ForgetIdCheck(Guid forgetId) => core.ForgetIdCheck(forgetId);
        public Task<ResponseMessageDto> PasswordChage(PasswordChageDto dto) => core.PasswordChage(dto);
        public Task<ResponseMessageDto> EmailChage(EmailChangeDto dto) => core.EmailChage(dto);
        public CheckRedirectUrlOutputDto checkRedirectUrl(string? redirectUrl) => core.checkRedirectUrl(redirectUrl);
    }
}
