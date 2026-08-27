using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 物流與付款方式的使用限制。
    ///
    /// FK_LogisticsSettingId 為 null：
    ///     表示系統預設規則，以 ShippingType 為判斷依據。
    ///
    /// FK_LogisticsSettingId 有值：
    ///     表示特定網站物流設定的覆寫規則。
    /// </summary>
    public class LogisticsPaymentRestriction : FullAuditedEntity
    {
        /// <summary>
        /// 物流型態。
        /// 系統預設規則會使用此欄位查詢。
        ///
        /// 有值時，FK_LogisticsSettingId 必須為 null。
        /// </summary>
        public ShippingTypeEnum? ShippingType { get; set; }

        /// <summary>
        /// 特定物流設定 ID。
        /// null 表示系統預設規則；
        /// 有值表示覆寫該 LogisticsSetting 的設定。
        /// </summary>
        public long? FK_LogisticsSettingId { get; set; }

        /// <summary>
        /// 付款方式 ID。
        /// </summary>
        public long FK_PaymentTypeId { get; set; }

        /// <summary>
        /// 此物流是否支援該付款方式。
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 物流端額外限制的最低金額。
        ///
        /// null 表示沿用 PaymentType.MinAmount，
        /// 不進行物流端覆寫。
        /// </summary>
        public decimal? OverrideMinAmount { get; set; }

        /// <summary>
        /// 物流端額外限制的最高金額。
        ///
        /// null 表示沿用 PaymentType.MaxAmount，
        /// 不進行物流端覆寫。
        /// </summary>
        public decimal? OverrideMaxAmount { get; set; }

        public LogisticsSetting? LogisticsSetting { get; set; }

        public PaymentType? PaymentType { get; set; }
    }
}
