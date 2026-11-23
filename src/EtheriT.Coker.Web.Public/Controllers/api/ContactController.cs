using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Contact;
using EtheriT.Coker.Application.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ContactController : Controller
	{
		private readonly IContactAppService contactAppService;
		public ContactController(IContactAppService contactAppService) { 
			this.contactAppService = contactAppService;
		}
        [HttpPost]
        public async Task<ResponseMessageDto> submit(FormSubmitDto dto)
        {
            // 同一 IP 10 分鐘內最多送出 10 次，避免被拿來大量寄信
            if (!AccountRequestGuard.Allow(HttpContext, "contact-submit", 10))
                return new ResponseMessageDto { Error = "操作過於頻繁，請稍後再試" };
            return await contactAppService.submit(dto);
        }
    }
}
