using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        [Authorize]
        public async Task<ResponseMessageDto> AddUser(FrontAddUserDto dto)
        {
            var result = await accountAppService.AddFrontUser(dto);
            return result;
        }
    }
}
