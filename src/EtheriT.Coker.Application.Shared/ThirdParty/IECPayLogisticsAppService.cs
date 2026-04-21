
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayLogisticsAppService
    {
        public Task<ECPayLogisticsMapRequestDto> ECPayLogisticsGetMapRequestBody(string SCIds, string LogisticsSubType, string IsCollection);
        public Task<ResponseMessageDto> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData);
        public Task<ECPayLogisticsCreateCVSRequestDto> ECPayLogisticsExpressCVSCreate(long ohid);
        public Task<ResponseMessageDto> ECPayLogisticsExpressCreateResponse(Dictionary<string, string> ResultResponseData);
        public Task<ResponseMessageDto> ECPayLogisticsPrintOrderInfoDto(ECPayLogisticsPrintOrderInfoEnum type, long ohid);
    }
}
