using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 行銷活動適用範圍明細
    /// </summary>
    public class MarketingScopeItem : FullAuditedEntity
    {
        public long FK_MarketingRuleId { get; set; }

        public virtual MarketingRule MarketingRule { get; set; }

        /// <summary>
        /// 範圍目標類型
        /// 商品 / 規格 / 分類 / 品牌 / 標籤
        /// </summary>
        public MarketingScopeTargetTypeEnum TargetType { get; set; }

        /// <summary>
        /// 目標 Id
        /// 例如商品 Id、分類 Id、規格 Id
        /// </summary>
        public long TargetId { get; set; }
    }
}
