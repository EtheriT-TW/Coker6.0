using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Member;
using EtheriT.Coker.Application.Shared.Member;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FreightController : Controller
    {
        //private readonly IFreightAppService freightAppService;
        //public FreightController(
        //    IFreightAppService freightAppService
        //    )
        //{
        //    this.freightAppService = freightAppService;
        //}

        //[HttpGet]
        //public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        //{
        //    return await freightAppService.GetAllList(loadOptions);
        //}

        //[HttpGet]
        //public async Task<MemberGetAllDataDto> GetDataById(long id)
        //{
        //    return await freightAppService.GetData(id);
        //}

        //[HttpPost]
        //public async Task<ResponseMessageDto> Add(MemberUpdateDto dto)
        //{
        //    return await freightAppService.Update(dto);
        //}

        //[HttpPost]
        //public async Task<ResponseMessageDto> Update(MemberUpdateDto dto)
        //{
        //    return await freightAppService.Update(dto);
        //}
    }
}
