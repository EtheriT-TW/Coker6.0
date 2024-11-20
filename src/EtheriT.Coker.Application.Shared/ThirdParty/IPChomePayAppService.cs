
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IPChomePayAppService
    {
        public Task<ResponseMessageDto> PChomePayRequest(long ohid);
        //public Task<ResponseMessageDto> PChomePayReturn(object dto);
        //public Task<ResponseMessageDto> PChomePayFailReturn(object dto);
        public Task<string> PChomePayNotify(PChomePayNotifyDto dto);
        public Task<PChomePayStateDto> PChomePayCheckPaymentStatus(long ohid);
    }
}
