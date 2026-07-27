using EtheriT.Coker.Application.Shared.Dto.Payment;
using EtheriT.Coker.Application.Shared.Payment;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Payment
{
    public class PaymentAvailabilityService : IPaymentAvailabilityService
    {
        private const long EcpayThirdPartyId = 4;
        private readonly CokerDbContext db;

        public PaymentAvailabilityService(CokerDbContext db)
        {
            this.db = db;
        }

        public async Task<List<PaymentAvailabilityItemDto>> GetAvailableAsync(
            long websiteId,
            long logisticsSettingId,
            decimal amount)
        {
            if (websiteId <= 0)
                throw new Exception("網站不存在。");

            if (amount < 0)
                throw new Exception("訂單金額不可小於 0。");

            var logistics = await db.LogisticsSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == logisticsSettingId &&
                    x.FK_WebsiteId == websiteId);

            if (logistics == null)
                throw new Exception("查無可用的物流設定。");

            var payments = await (
                from value in db.PaymentTypesValues.AsNoTracking()
                join paymentType in db.PaymentTypes.AsNoTracking()
                    on value.FK_PaymentTypesId equals paymentType.Id
                where value.FK_WebsiteId == websiteId && value.Used
                orderby paymentType.SerNo, paymentType.Id
                select paymentType
            ).ToListAsync();

            if (payments.Count == 0)
                return new List<PaymentAvailabilityItemDto>();

            var paymentTypeIds = payments.Select(x => x.Id).ToList();

            var defaultRules = await db.LogisticsType_Payments
                .AsNoTracking()
                .Where(x =>
                    x.FK_LogisticsSettingId == null &&
                    x.ShippingType == logistics.LogisticsType &&
                    paymentTypeIds.Contains(x.FK_PaymentTypeId))
                .ToDictionaryAsync(x => x.FK_PaymentTypeId);

            var customRules = await db.LogisticsType_Payments
                .AsNoTracking()
                .Where(x =>
                    x.FK_LogisticsSettingId == logisticsSettingId &&
                    x.ShippingType == null &&
                    paymentTypeIds.Contains(x.FK_PaymentTypeId))
                .ToDictionaryAsync(x => x.FK_PaymentTypeId);

            var output = new List<PaymentAvailabilityItemDto>();

            foreach (var payment in payments)
            {
                customRules.TryGetValue(payment.Id, out var customRule);
                defaultRules.TryGetValue(payment.Id, out var defaultRule);

                var rule = customRule ?? defaultRule;
                var isEnabled = rule?.IsEnabled ?? true;

                // A custom row overrides the shipping default. Null amount overrides always
                // fall back to PaymentType, not to the shipping default row.
                var minAmount = rule?.OverrideMinAmount ?? payment.MinAmount;
                var maxAmount = rule?.OverrideMaxAmount ?? payment.MaxAmount;

                if (!isEnabled ||
                    amount < minAmount ||
                    (maxAmount.HasValue && amount > maxAmount.Value))
                {
                    continue;
                }

                var isEcpay = payment.FK_ThirdPartyId == EcpayThirdPartyId;

                output.Add(new PaymentAvailabilityItemDto
                {
                    Id = payment.Id,
                    Title = payment.Title ?? string.Empty,
                    Code = payment.Code ?? string.Empty,
                    Icon = string.IsNullOrWhiteSpace(payment.Icons)
                        ? string.Empty
                        : $"/images/paymenticon/{payment.Icons}",
                    ThirdPartyId = payment.FK_ThirdPartyId,
                    ProviderCode = isEcpay
                        ? "ECPay"
                        : payment.FK_ThirdPartyId > 0
                            ? $"ThirdParty:{payment.FK_ThirdPartyId}"
                            : "Default",
                    RenderMode = isEcpay ? "Embedded" : "Standard",
                    MinAmount = minAmount,
                    MaxAmount = maxAmount
                });
            }

            return output;
        }

        public async Task ValidateAsync(
            long websiteId,
            long logisticsSettingId,
            long paymentTypeId,
            decimal amount)
        {
            var availablePayments = await GetAvailableAsync(
                websiteId,
                logisticsSettingId,
                amount);

            if (availablePayments.All(x => x.Id != paymentTypeId))
                throw new Exception("目前的物流方式或訂單金額不支援所選付款方式，請重新選擇。");
        }
    }
}
