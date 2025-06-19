using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OAuthController : Controller
    {
        private readonly IAccountAppService _accountAppService;
        public OAuthController(IAccountAppService accountAppService)
        {
            _accountAppService = accountAppService;
        }
        [HttpGet]
        public async Task<IActionResult> success(OAuthSuccessInputDto dto)
        {
            TempData["OAuthSuccess"] = true;
            await _accountAppService.FrontLoginByToken(dto.Token);
            return Redirect(dto.redirect);
        }
        public IActionResult error(OAuthErrorInputDto dto)
        {
            TempData["OAuthError"] = dto.code.ToString();
            if (string.IsNullOrEmpty(dto.redirect))
                dto.redirect = "/";

             return Redirect(dto.redirect);
        }
    }
}
