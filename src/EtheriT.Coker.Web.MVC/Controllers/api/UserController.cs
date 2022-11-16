using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IAccountAppService accountAppService;
        public UserController(
            ILogger<UserController> logger, 
            IAccountAppService accountAppService)
        {
            _logger = logger;
            this.accountAppService = accountAppService;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<LoginOutputDto> Login(LoginInputDto dto)
        {
            var result = await accountAppService.Login(dto);
            return result;
        }
        [HttpPost]
        [Authorize]
        public async Task<LoginOutputDto> Chech()
        {
            return await accountAppService.Chech();
        }
        [HttpGet]
        [Authorize]
        public async Task<ResponseMessageDto> Logout() {
            return await accountAppService.Logout();
        }
    }
}
