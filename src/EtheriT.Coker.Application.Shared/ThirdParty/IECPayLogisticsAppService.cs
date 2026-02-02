
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayLogisticsAppService
    {
        public Task<ResponseMessageDto> ECPayLogisticsGetMap(string SCIds, string LogisticsSubType);
        public Task<bool> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData);
        public Task<ResponseMessageDto> ECPayLogisticsExpressCreate(long ohid, List<string> prod_titles);
        public Task<bool> ECPayLogisticsExpressCreateResponse(ECPayLogisticsCreateResponseDto ResultResponseData);
    }
}
