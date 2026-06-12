using EtheriT.Coker.Application.Shared.Dto.Marketing;

namespace EtheriT.Coker.Application.Shared.Marketing
{
    public interface ICheckoutDiscountService
    {
        public Task<CheckoutDiscountResultDto> CalculateAsync(CheckoutDiscountInputDto input);
    }
}