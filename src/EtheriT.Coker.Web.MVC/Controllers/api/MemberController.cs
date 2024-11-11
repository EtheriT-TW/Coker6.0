using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Member;
using EtheriT.Coker.Application.Shared.Member;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MemberController : Controller
    {
        private readonly IMemberAppService memberAppService;
        private readonly IAccountAppService accountAppService;
        public MemberController(
            IMemberAppService memberAppService,
            IAccountAppService accountAppService
        )
        {
            this.memberAppService = memberAppService;
            this.accountAppService = accountAppService;
        }
        [HttpGet]
        public async Task<JsonResult> GetAllFrontList(DataSourceLoadOptions loadOptions)
        {
            return await memberAppService.GetAllFrontList(loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await memberAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllManagerList(DataSourceLoadOptions loadOptions)
        {
            return await memberAppService.GetAllManagerList(loadOptions);
        }

        [HttpGet]
        public async Task<MemberGetAllDataDto> GetAllData(long id)
        {
            return await memberAppService.GetAllData(id);
        }
        [HttpGet]
        public async Task<MemberGetAllDataDto> GetSelfData()
        {
            return await memberAppService.GetSelfData();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ForgetPassword(long UserId)
        {
            return await accountAppService.SendForget(UserId);
        }

        [HttpPost]
        public async Task<ResponseMessageDto> Update(MemberUpdateDto dto)
        {
            return await memberAppService.Update(dto);
        }
        [HttpGet]
        public async Task<List<SelectDto>> GetAllRole()
        {
            return await memberAppService.GetAllRole();
        }
    }
}
