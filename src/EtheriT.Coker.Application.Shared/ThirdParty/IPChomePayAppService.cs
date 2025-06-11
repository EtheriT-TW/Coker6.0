
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.PChomePayDto;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IPChomePayAppService
    {
        public Task<ResponseMessageDto> PChomePayRequest(long ohid);
        public Task<IActionResult> PChomePayReturn(string ohid);
        public Task<string> PChomePayNotify(PChomePayNotifyDto dto);
        public Task<ResponseMessageDto> PChomePayCheckPaymentStatus(long ohid);
        public Task<ResponseMessageDto> PChomePayRefund(long ohid, int? refund);
        public Task<ResponseMessageDto> PChomePayRefundState(long ohid);
        public Task<ResponseMessageDto> PChomePayCancelOrder(long ohid);
        public Task<ResponseMessageDto> PChomePayBalance();
        public Task<ResponseMessageDto> PChomePayDeliveryNote(long ohid);
    }
}
