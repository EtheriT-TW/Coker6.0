using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class FreightDto
    {
        public long? Id { get; set; }
        public string Title { get; set; }
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
        public List<ProdSelectedDto> ProdIds { get; set; } = new();
        public List<LogisticsBoxFeeDto> LogisticsBoxFees { get; set; } = new();
    }
}
