using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Token;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TokenController : Controller
    {

        private readonly ITokenAppService tokenAppService;
        public TokenController(
            ITokenAppService tokenAppService
            )
        {
            this.tokenAppService = tokenAppService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<TokenResponseDto> CreateToken()
        {
            return await tokenAppService.CreateToken();
        }

        [HttpGet]
        public TokenResponseDto CheckToken()
        {
            return tokenAppService.CheckToken();
        }

    }
}