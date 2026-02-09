using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Advertise;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DirectoryController : Controller
    {
        private readonly IDirectoryAppService directoryAppService;

        public DirectoryController(IDirectoryAppService directoryAppService)
        {
            this.directoryAppService = directoryAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto)
        {
            return await directoryAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<DirectoryGetDataDto> GetDataOne(long Id)
        {
            return await directoryAppService.GetDataOne(Id);
        }
        [HttpPost]
        public async Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto)
        {
            return await directoryAppService.GetReleMenu(dto);
        }
        [HttpPost]
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            return await directoryAppService.GetReleInfo(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetDirectoryDetailList(string type, long id, DataSourceLoadOptions loadOptions)
        {
            switch (type) {
                case "Articles":
                    return await directoryAppService.GetDirectoryArticlesList(id, loadOptions);
                case "Products":
                    return await directoryAppService.GetDirectoryProductsList(id, loadOptions);
                case "Menus":
                    return await directoryAppService.GetDirectoryMenusList(id, loadOptions);
                case "Advertise":
                    return await directoryAppService.GetDirectoryAdvertiseList(id, loadOptions);
                default:
                    return new JsonResult(new List<ArticleListGetDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await directoryAppService.GetAllList(loadOptions);
        }
        public async Task<JsonResult> GetAdvertiseList(DataSourceLoadOptions loadOptions)
        {
            return await directoryAppService.GetAdvertiseList(loadOptions);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await directoryAppService.Delete(Id);
        }
        [HttpPost]
        public async Task<List<AdvertiseDisplayDto>> GetReleAd(DataIdWebsiteIdDto dto)
        {
            return await directoryAppService.GetReleAd(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> GetDirectoryFacetConfig(long Id)
        {
            return await directoryAppService.GetDirectoryFacetConfig(Id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveDirectoryFacetConfig(DirectoryFacetConfigDto dto)
        {
            return await directoryAppService.SaveDirectoryFacetConfig(dto);
        }
    }
}
