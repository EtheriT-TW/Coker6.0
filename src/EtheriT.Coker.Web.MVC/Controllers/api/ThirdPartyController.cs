using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.ThirdParty;
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
        private readonly ILinePayAppService linePayAppService;
        public ThirdPartyController(
            IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService)
        {
            this.thirdPartyAppService = thirdPartyAppService;
            this.linePayAppService = linePayAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto)
        {
            return await thirdPartyAppService.SaveThirdParty(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayConfirm(long ohid)
        {
            return await linePayAppService.LinePayConfirm(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayVoid(long ohid)
        {
            return await linePayAppService.LinePayVoid(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LinePayRefund(long ohid, int? refund)
        {
            return await linePayAppService.LinePayRefund(ohid, refund);
        }
        [HttpGet]
        public async Task<LinePayResponseDto> LinePayCheckPaymentStatus(long ohid)
        {
            return await linePayAppService.LinePayCheckPaymentStatus(ohid);
        }
    }
}
