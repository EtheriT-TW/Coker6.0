using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class LogisticsType_PaymentTypeSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var creationTime = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Local);

            // 未設定限制時預設開放。
            // 門市專屬貨到付款只允許相同通路的一般取貨型別，
            // 其他物流型別預設關閉，網站仍可透過 FK_LogisticsSettingId 自訂覆寫。
            var allowedShippingTypes = new Dictionary<long, ShippingTypeEnum>
            {
                [7] = ShippingTypeEnum.Seven取貨,
                [8] = ShippingTypeEnum.全家取貨,
                [10] = ShippingTypeEnum.萊爾富取貨
            };

            var restrictions = allowedShippingTypes
                .SelectMany(payment => Enum.GetValues<ShippingTypeEnum>()
                    .Where(shippingType => shippingType != payment.Value)
                    .Select(shippingType => new LogisticsPaymentRestriction
                    {
                        // Seed 使用固定負數 ID，避免與正式資料的 Identity 正數衝突。
                        Id = -(payment.Key * 100 + (long)shippingType),
                        CreationTime = creationTime,
                        CreatorUserId = 1,
                        ShippingType = shippingType,
                        FK_LogisticsSettingId = null,
                        FK_PaymentTypeId = payment.Key,
                        IsEnabled = false,
                        OverrideMinAmount = null,
                        OverrideMaxAmount = null
                    }))
                .ToArray();

            modelBuilder.Entity<LogisticsPaymentRestriction>().HasData(restrictions);
        }
    }
}
