using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 行銷活動觸發條件
    /// </summary>
    public class MarketingCondition : FullAuditedEntity
    {
        public long FK_MarketingRuleId { get; set; }

        public virtual MarketingRule MarketingRule { get; set; }

        /// <summary>
        /// 條件類型
        /// 第一階段使用 OrderAmount
        /// </summary>
        public MarketingConditionTypeEnum ConditionType { get; set; }

        /// <summary>
        /// 滿額門檻
        /// 例如滿 1000
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// 滿件門檻
        /// 未來滿件優惠、買三送一、任選 N 件可用
        /// </summary>
        public int? MinQuantity { get; set; }

        /// <summary>
        /// 是否只計算適用範圍內的商品
        /// 第一階段全站活動固定 false
        /// 未來指定商品滿額時會用 true
        /// </summary>
        public bool OnlyScopeItems { get; set; }

        /// <summary>
        /// 是否排除已折扣、贈品、加價購商品
        /// 第一階段可先 false
        /// </summary>
        public bool ExcludeDiscountedItems { get; set; }
    }
}
