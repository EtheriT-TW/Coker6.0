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
        public async Task<IActionResult> success(Guid Token, string redirect)
        {
            TempData["OAuthSuccess"] = true;
            var resule = await _accountAppService.FrontLoginByToken(Token);
            if (resule.Success) return Redirect(redirect);
            else if (int.TryParse(resule.Error, out var code)) return error((OAuthErrorTypeEnum)code, redirect);
            else {
                TempData["OAuthSuccess"] = false;
                TempData["OAuthError"] = resule.Error;
                return Redirect(redirect);
            }
        }
        public IActionResult error(OAuthErrorTypeEnum code, string? redirect)
        {
            TempData["OAuthError"] = code.ToString();
            if (string.IsNullOrEmpty(redirect))
                redirect = "/";

             return Redirect(redirect);
        }
    }
}
