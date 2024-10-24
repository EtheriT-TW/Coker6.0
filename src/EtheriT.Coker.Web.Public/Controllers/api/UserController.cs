using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IAccountAppService accountAppService;
        public UserController(
            IAccountAppService accountAppService)
        {
            this.accountAppService = accountAppService;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> AddUser(FrontAddUserDto dto)
        {
            var result = await accountAppService.AddFrontUser(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> ReSendOpening(SendOpeningDto dto)
        {
            var result = await accountAppService.ReSendOpening(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<LoginOutputDto> Login(FrontLoginInputDto dto)
        {
            var result = await accountAppService.FrontLogin(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> FrontUserEdit(FrontEditUserDto dto)
        {
            var result = await accountAppService.FrontUserEdit(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> PasswordForget(SendForgetDto dto)
        {
            var result = await accountAppService.SendForget(dto);
            return result;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> PasswordChage(PasswordChageDto dto)
        {
            var result = await accountAppService.PasswordChage(dto);
            return result;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> GetUserData(Guid refreshToken)
        {
            var result = await accountAppService.GetFrontUserData(refreshToken);
            return result;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> ForgetIdCheck(Guid ForgetId)
        {
            var result = await accountAppService.ForgetIdCheck(ForgetId);
            return result;
        }
        [HttpGet]
        public async Task<LoginOutputDto> Logout()
        {
            var result = await accountAppService.FrontLogout();
            return result;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto> AccountOpening(Guid OpenId)
        {
            var result = await accountAppService.AccountOpening(OpenId);
            return result;
        }
    }
}
