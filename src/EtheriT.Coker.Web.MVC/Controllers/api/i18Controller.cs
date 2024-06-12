using EtheriT.Coker.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application.Shared.i18n;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class i18Controller : Controller
    {

        private readonly LoginUserData loginUserData;
        public i18Controller(LoginUserData loginUserData) { 
            this.loginUserData = loginUserData;
        }
        [HttpGet]
        public async Task<Object?> getLocal()
        {
            var local = await loginUserData.GetWebsiteLocal();
            L.local = local;
            return JsonConvert.DeserializeObject(L.getAllJsonString());
        }
    }
}
