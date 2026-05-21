using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AdvertiseController : Controller
    {
        private readonly IAdvertiseAppService advertiseAppService;

        public AdvertiseController(IAdvertiseAppService advertiseAppService)
        {
            this.advertiseAppService = advertiseAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(AdvertiseDto dto)
        {
            return await advertiseAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetEnterAdList(DataSourceLoadOptions loadOptions)
        {
            return await advertiseAppService.GetList(loadOptions, 1);
        }
        [HttpGet]
        public async Task<JsonResult> GetRightSideAdList(DataSourceLoadOptions loadOptions)
        {
            return await advertiseAppService.GetList(loadOptions, 2);
        }
        [HttpGet]
        public async Task<AdvertiseGetDataDto> GetDataOne(long Id)
        {
            return await advertiseAppService.GetDataOne(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await advertiseAppService.Delete(Id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> GetConten(SearchIDDto dto)
        {
            return await advertiseAppService.GetConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ImportConten(ArticleSaveContenDto dto)
        {
            return await advertiseAppService.ImportConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto)
        {
            return await advertiseAppService.SaveConten(dto);
        }
    }
}
