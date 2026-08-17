using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayLogisticsAppService
    {
        public Task<ECPayLogisticsMapRequestDto> ECPayLogisticsGetMapRequestBody(string SCIds, string LogisticsSubType, string IsCollection);
        public Task<ResponseMessageDto> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData);
        public Task<ResponseMessageDto> ECPayLogisticsExpressCVSCreate(long ohid);
        public Task<ResponseMessageDto> ECPayLogisticsExpressCreateResponse(ECPayLogisticsCallbackDto dto);
        public Task<ResponseMessageDto> ECPayLogisticsPrintOrderInfo(ECPayLogisticsPrintOrderInfoEnum type, long ohid);
        public Task<ResponseMessageDto> ECPayLogisticsTradeInfo(long ohid);
    }
}
