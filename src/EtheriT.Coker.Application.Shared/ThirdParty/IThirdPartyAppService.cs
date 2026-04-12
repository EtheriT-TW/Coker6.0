using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IThirdPartyAppService
    {
        public Task<ResponseMessageDto> GetAllThirdParty(ThirdPartyServiceTypeEnum ServeiceType);
        public Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto);
        public Task<JsonResult> GetDisplayPayment();
        public Task<List<ThirdPartyKeypairItemOutputDto>> GetPaymentResult(long paytypeid);
        public Task<ResponseMessageDto> CheckSource(string Token);
        public Task<ResponseMessageDto> HandleThirdPartyPayment(HandleThirdPartyPaymentDto dto);
        public Task<ResponseMessageDto> HandleThirdPartyLogistics(HandleThirdPartyLogisticsDto dto);
    }
}
