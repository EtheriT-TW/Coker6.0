using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : Controller
    {
        public async Task<ResponseMessageDto> Line()
        {
            ResponseMessageDto output = new ResponseMessageDto();
            try {
                var authenticateResult = await HttpContext.AuthenticateAsync();
                var claims = authenticateResult.Principal.Claims;
                if (claims != null) {
                    var lineId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                }throw new Exception("No claims found");
            }
            catch {
                output.Message = "登入失敗";
            }
            return output;
        }
    }
}
