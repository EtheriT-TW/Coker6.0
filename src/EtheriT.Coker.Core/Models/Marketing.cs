using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    // 行銷活動
    public class Marketing : FullAuditedEntity
    {
        // 活動狀態(促銷中、已關閉、已結束、未開始)
        public MarketingStatusEnum Status { get; set; }
        // 活動名稱
        public string Title { get; set; }
        // 活動描述
        public string? Description { get; set; }
        // 活動類型(滿額折抵、滿額贈...)
        public MarketingActivityTypeEnum ActivityType { get; set; }
        // 折抵目標(滿額、滿幾件)
        public int Target {  get; set; }
        // 折扣類型(金額扣抵、打折......)
        public MarketingDiscountTypeEnum DiscountType { get; set; }
        // 折扣值
        public double Discount { get; set; }
        // 最高折抵
        public double? MaxDiscount { get; set; }
        // 起始時間
        public DateTime? StartTime { get; set; }
        // 結束時間
        public DateTime? EndTime { get; set; }
        // 是否永久顯示
        public bool Permanent { get; set; } = true;
        // 是否允許重複使用
        public bool IsReusable { get; set; } = false;
        public long FK_WebsiteId { get; set; }
        public Website? Website { get; set; }
    }
}
