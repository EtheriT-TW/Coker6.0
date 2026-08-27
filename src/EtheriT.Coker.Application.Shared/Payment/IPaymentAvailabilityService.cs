using EtheriT.Coker.Application.Shared.Dto.Payment;

namespace EtheriT.Coker.Application.Shared.Payment
{
    public interface IPaymentAvailabilityService
    {
        Task<List<PaymentAvailabilityItemDto>> GetAvailableAsync(
            long websiteId,
            long logisticsSettingId,
            decimal amount);

        Task ValidateAsync(
            long websiteId,
            long logisticsSettingId,
            long paymentTypeId,
            decimal amount);
    }
}
