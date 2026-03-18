
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayLogisticsAppService
    {
        public Task<ResponseMessageDto> ECPayLogisticsGetMap(string SCIds, string LogisticsSubType);
        public Task<ResponseMessageDto> ECPayLogisticsGetMapResponse(ECPayLogisticsMapResponseDto ResultResponseData);
        public Task<ResponseMessageDto> ECPayLogisticsExpressCreate(long ohid, List<string> prod_titles, ShippingTypeEnum LogisticsType);
        public Task<bool> ECPayLogisticsExpressCreateResponse(ECPayLogisticsCreateResponseDto ResultResponseData);
    }
}
