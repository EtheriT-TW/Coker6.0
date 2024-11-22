using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
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
        [HttpPost]
        public async Task<ResponseMessageDto> AddUpUserGroup(UserGroupAddUpDto dto)
        {
            return await userHabitsAppService.AddUpUserGroup(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> GetUserGroupOne(long id) { 
            return await userHabitsAppService.GetUserGroupOne(id);
        }
    }
}
