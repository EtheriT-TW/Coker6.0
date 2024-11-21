using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
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
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        public ThirdPartyController(
            IThirdPartyAppService thirdPartyAppService,
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService)
        {
            this.thirdPartyAppService = thirdPartyAppService;
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
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
        public async Task<ResponseMessageDto> PayRefund(string payment, long ohid, int? refund)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (payment)
            {
                case "LinePay":
                    return await linePayAppService.LinePayRefund(ohid, refund);
                case "PCHomePay":
                    return await pchomePayAppService.PChomePayRefund(ohid, refund);
            }
            response.Success = false;
            response.Message = "支付方式不存在";
            return response;
        }
        [HttpGet]
        public async Task<LinePayResponseDto> LinePayCheckPaymentStatus(long ohid)
        {
            return await linePayAppService.LinePayCheckPaymentStatus(ohid);
        }
        [HttpGet]
        public async Task<PChomePayStateDto> PChomePayCheckPaymentStatus(long ohid)
        {
            return await pchomePayAppService.PChomePayCheckPaymentStatus(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PChomePayBalance()
        {
            return await pchomePayAppService.PChomePayBalance();
        }
    }
}
