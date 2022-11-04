using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IAccountAppService accountAppService;
        public UserController(ILogger<UserController> logger, IAccountAppService accountAppService)
        {
            _logger = logger;
            this.accountAppService = accountAppService;
        }
        [HttpPost]
        public async Task<LoginOutputDto> Login(LoginInputDto dto)
        {
            var result = await accountAppService.Login(dto);
            return result;
        }
    }
}
