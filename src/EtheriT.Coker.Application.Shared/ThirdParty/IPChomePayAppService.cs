
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IPChomePayAppService
    {
        //public Task<ResponseMessageDto> LinePayRequest(long ohid);
        //public Task<IActionResult> LinePayConfirm(string transactionId, string orderId);
        //public Task<ResponseMessageDto> LinePayConfirm(long ohid);
        //public Task<IActionResult> LinePayCancel(string transactionId, string orderId);
        //public Task<ResponseMessageDto> LinePayVoid(long ohid);
        //public Task<ResponseMessageDto> LinePayRefund(long ohid, int? refund);
        //public Task<LinePayResponseDto> LinePayCheckPaymentStatus(long ohid);
        public Task<ResponseMessageDto> PChomePayHeaders();
    }
}
