using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.UserHabits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserHabitsController : Controller
    {
        private readonly IUserHabitsAppService userHabitsAppService;
        public UserHabitsController(IUserHabitsAppService userHabitsAppService) { 
            this.userHabitsAppService = userHabitsAppService;
        }
        [HttpGet]
        public async Task<JsonResult> GetUserGroupList(DataSourceLoadOptions loadOptions)
        {
            return await userHabitsAppService.GetUserGroupList(loadOptions);
        }
    }
}
