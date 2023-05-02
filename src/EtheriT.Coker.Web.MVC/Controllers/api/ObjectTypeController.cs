using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.ObjectType;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ObjectTypeController : Controller
    {
        public IObjectTypeAppService objectTypeAppService;
        public ObjectTypeController(IObjectTypeAppService objectTypeAppService) { 
            this.objectTypeAppService = objectTypeAppService;
        }
        [HttpGet]
        public async Task<ObjectTypeGetAlldto> GetAll()
        {
            return await objectTypeAppService.GetAll();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> CreateOrEdit(ObjectTypeItemDto dto) {
            return await objectTypeAppService.CreateOrEdit(dto);
        }
        [HttpDelete]
        public async Task<ResponseMessageDto> DeleteHtmlContent(DataDelectDto dto) {
            return await objectTypeAppService.DeleteHtmlContent(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> UpdateSerNo(UpdateSerNoListDto dto) { 
            return await objectTypeAppService.UpdateSerNo(dto);
        }
        [HttpPost]
        public async Task<HtmlContentGetHtmlDto> GetConten(SearchIDDto dto)
        {
            return await objectTypeAppService.GetConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(HtmlContentDetailDto dto) {
            return await objectTypeAppService.SaveConten(dto);
        }
    }
}
