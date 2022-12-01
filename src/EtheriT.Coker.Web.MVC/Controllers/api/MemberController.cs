using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
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
        public MemberController(
            IMemberAppService memberAppService
            )
        {
            this.memberAppService = memberAppService;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await memberAppService.GetAllList(loadOptions);
        }

        [HttpGet]
        public async Task<MemberGetAllDataDto> GetAllData(long id)
        {
            return await memberAppService.GetAllData(id);
        }

        [HttpPost]
        public async Task<ResponseMessageDto> Update(MemberUpdateDto dto)
        {
            return await memberAppService.Update(dto);
        }
    }
}
