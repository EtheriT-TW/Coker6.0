using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.Contact;
using EtheriT.Coker.Application.Contact;
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
			return await contactAppService.submit(dto);
		}
	}
}
