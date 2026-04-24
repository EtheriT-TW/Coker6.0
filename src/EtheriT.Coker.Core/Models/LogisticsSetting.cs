using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class LogisticsSetting : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(50)] public string Title { get; set; }
        public PreserveTypeEnum PreserveType { get; set; }
        public ShippingTypeEnum LogisticsType { get; set; }
        public FreightTypeEnum FreightType { get; set; }
        public FreightStatusTypeEnum FreightStatusType { get; set; }
        public DiscountFreightType? DiscountFreightType { get; set; }
        public int? Freight { get; set; }
        public int? Low_Con { get; set; }
        public int? Dis_Freight { get; set; }
        public bool Set_Default { get; set; }
        public int? FreightAmt2 { get; set; }
        public bool SupportCashOnDelivery { get; set; } = false;
        public Website? Website { get; set; }
        public ICollection<MappingLogisticsSettingAndProd> MappingLogisticsSettingAndProds { get; set; }
        public ICollection<Order_Header> Order_Headers { get; set; }
        public ICollection<LogisticsBoxFee> logisticsBoxFees { get; set; }
    }
}
