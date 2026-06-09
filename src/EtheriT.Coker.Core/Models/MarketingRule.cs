using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 行銷活動規則
    /// </summary>
    public class MarketingRule : FullAuditedEntity
    {
        public long FK_MarketingCampaignId { get; set; }

        public virtual MarketingCampaign MarketingCampaign { get; set; }

        /// <summary>
        /// 規則類型
        /// 第一階段：
        /// AmountDiscount = 滿額折固定金額
        /// PercentDiscount = 滿額打折
        /// </summary>
        public MarketingRuleTypeEnum RuleType { get; set; }

        /// <summary>
        /// 適用範圍
        /// 第一階段只用 AllOrder
        /// 未來擴充指定商品、指定分類、指定規格
        /// </summary>
        public MarketingScopeTypeEnum ScopeType { get; set; }

        /// <summary>
        /// 是否啟用此規則
        /// 第一階段通常跟 Campaign 一起控制即可
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 規則排序
        /// </summary>
        public int SortOrder { get; set; }

        public virtual MarketingCondition Condition { get; set; }

        public virtual MarketingReward Reward { get; set; }

        public virtual ICollection<MarketingScopeItem> ScopeItems { get; set; } = new List<MarketingScopeItem>();
    }
}
