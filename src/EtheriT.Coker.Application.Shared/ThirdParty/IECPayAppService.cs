using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayAppService
    {
        public Task<ResponseMessageDto> ECPayOrderState(long ohid);
        public Task<ResponseMessageDto> ECPayRefund(long ohid);
        public Task<IActionResult> ECPayOrderResult(string ResultData);
        public Task<String> ECPayReturn(ECPayReturnResponseDto ResultResponseData);
        public Task<ResponseMessageDto> ECPayCreatePayment(ECPayPaymentInfoDto PaymentInfo);
        public Task<ResponseMessageDto> ECPayGetToken(OrderHeaderAddDto dto);
    }
}
