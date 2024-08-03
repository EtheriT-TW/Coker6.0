using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ThirdPartyController
    {
        private readonly IThirdPartyAppService thirdPartyAppService;
        public ThirdPartyController(IThirdPartyAppService thirdPartyAppService) { 
            this.thirdPartyAppService = thirdPartyAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto) { 
            return await thirdPartyAppService.SaveThirdParty(dto);
        }
    }
}
