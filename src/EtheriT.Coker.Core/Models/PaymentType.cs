using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class PaymentType : FullAuditedEntity
    {
        // 名稱
        [StringLength(50)] public string? Title { get; set; }
        // 編碼
        [StringLength(50)] public string? Code { get; set; }
        public bool Used { get; set; }
        // 排序
        public int SerNo { get; set; }
        // 允許的最低金額
        public int MinAmount { get; set; } = 1;
        // 允許的上限金額
        public int? MaxAmount { get; set; }
        // Icon
        public string Icons { get; set; }
        // 是否允許退款
        public bool CanRefund { get; set; }
        // 退款所需時間(日)
        public int RefundWorkDay { get; set; }
        public long FK_ThirdPartyId { get; set; }
        //超過多久可以重新付款
        public int? RepayAfterMinutes { get; set; }
        public ThirdParty? ThirdParty { get; set; }
        public List<LogisticsPaymentRestriction>? LogisticsType_Payments { get; set; }
        public List<PaymentTypesValue>? paymentTypesValues { get; set; }
        public List<Order_Header>? Order_Headers { get; set; }
    }
}
