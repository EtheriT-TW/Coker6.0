using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    // 行銷活動
    public class Marketing : FullAuditedEntity
    {
        // 是否開啟
        public bool Enable { get; set; }
        // 折抵目標(滿額、滿幾件)
        public int Target {  get; set; }
        // 折扣類型(金額扣抵、打折......)
        public int DiscountType { get; set; }
        // 折扣值
        public double Discount { get; set; }
        // 最高折抵
        public double? MaxDiscount { get; set; }
        // 起始時間
        public virtual DateTime? StartTime { get; set; }
        // 結束時間
        public virtual DateTime? EndTime { get; set; }
        // 是否永久顯示
        public bool Permanent { get; set; }
    }
}
