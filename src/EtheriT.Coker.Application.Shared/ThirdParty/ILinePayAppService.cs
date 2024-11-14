using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface ILinePayAppService
    {
        public Task<ResponseMessageDto> LinePayRequest(long ohid);
        public Task<ResponseMessageDto> LinePayConfirm(long ohid);
    }
}
