
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayLogisticsAppService
    {
        public Task<ResponseMessageDto> ECPayLogisticsGetMap(string SCIds, string LogisticsSubType);
        public Task<ResponseMessageDto> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData);
        public Task<ResponseMessageDto> ECPayLogisticsExpressCreate(long ohid, List<string> prod_titles);
        public Task<bool> ECPayLogisticsExpressCreateResponse(ECPayLogisticsCreateResponseDto ResultResponseData);
    }
}
