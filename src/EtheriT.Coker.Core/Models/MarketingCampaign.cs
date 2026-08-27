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
    /// 行銷活動主檔
    /// </summary>
    public class MarketingCampaign : FullAuditedEntity
    {
        /// <summary>
        /// 所屬網站
        /// </summary>
        public long FK_WebsiteId { get; set; }

        /// <summary>
        /// 活動名稱
        /// 例如：全站滿 1000 折 100
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 活動說明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 活動大類
        /// 第一階段使用：滿額優惠
        /// </summary>
        public MarketingCampaignTypeEnum CampaignType { get; set; }

        /// <summary>
        /// 活動狀態
        /// 可存：草稿、活動中、已關閉
        /// 未開始、已結束主要由起訖時間計算顯示
        /// </summary>
        public MarketingDisplayStatusEnum Status { get; set; }

        /// <summary>
        /// 活動開始時間
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活動結束時間
        /// NeverEnd = true 時可為 null
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 是否不設定結束時間
        /// </summary>
        public bool NeverEnd { get; set; }

        /// <summary>
        /// 優先權
        /// 數字越小越優先
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否可與其他優惠併用
        /// 第一階段可以先固定 false
        /// </summary>
        public bool CanStack { get; set; }

        /// <summary>
        /// 資格是否可重複觸發。
        /// 例如滿 1000 的活動，訂單 2000 是否取得兩次資格；
        /// 或每購買 1 件指定商品取得一次資格時，購買 2 件是否取得兩次資格。
        /// 第一階段建議先 false
        /// </summary>
        public bool Repeatable { get; set; }

        public virtual ICollection<MarketingRule> Rules { get; set; } = new List<MarketingRule>(); 
        public virtual Website Website { get; set; }
    }
}
