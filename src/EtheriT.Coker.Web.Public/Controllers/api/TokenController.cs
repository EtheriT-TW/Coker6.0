using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        public async Task<ResponseMessageDto> CreateToken()
        {
            return await tokenAppService.CreateToken();
        }

    }

}