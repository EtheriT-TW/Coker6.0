using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;

namespace EtheriT.Coker.Application.Authorization
{
    public interface IFrontAccountAppService
    {
        Task<LoginOutputDto> FrontLogin(FrontLoginInputDto dto);
        Task<LoginOutputDto> FrontLoginByToken(Guid token);
        Task<LoginOutputDto> FrontThirdLogin(FrontThirdLoginInputDto dto);
        Task<LoginOutputDto> FrontLogout();
        Task<ResponseMessageDto> AddFrontUser(FrontAddUserDto dto);
        Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto);
        Task<ResponseUserEditDto> GetFrontUserData();
        Task<string> GetFrontUserLevelName();
        Task<ResponseMessageDto> AccountOpening(Guid openId);
        Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto);
        Task<ResponseMessageDto> SendForget(SendForgetDto dto);
        Task<ResponseMessageDto> ForgetIdCheck(Guid forgetId);
        Task<ResponseMessageDto> PasswordChage(PasswordChageDto dto);
        Task<ResponseMessageDto> EmailChage(EmailChangeDto dto);
        CheckRedirectUrlOutputDto checkRedirectUrl(string? redirectUrl);
    }
}
