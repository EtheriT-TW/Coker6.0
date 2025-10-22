using EtheriT.Coker.Application.Contact;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using DevExtreme.AspNet.Mvc;
using System.Xml.Linq;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ContactController : Controller
    {
        private readonly IContactAppService contactAppService;
        public ContactController(IContactAppService contactAppService)
        {
            this.contactAppService = contactAppService;
        }
        [HttpGet]
        public async Task<JsonResult> GetContactListAll(DataSourceLoadOptions loadOptions)
        {
            return await contactAppService.GetContactListAll(loadOptions);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> GetDataOne(long id)
        {
            return await contactAppService.GetDataOne(id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ReplyContact(ContactReplyDto dto)
        {
            return await contactAppService.ReplyContact(dto);
        }
    }
}
