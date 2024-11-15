using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface ILinePayAppService
    {
        public Task<ResponseMessageDto> LinePayRequest(long ohid);
        public Task<IActionResult> LinePayConfirm(string transactionId, string orderId);
        public Task<IActionResult> LinePayCancel(string transactionId, string orderId);
        public Task<LinePayResponseDto> LinePayCheckPaymentStatus(string transactionId, string orderId);
    }
}
