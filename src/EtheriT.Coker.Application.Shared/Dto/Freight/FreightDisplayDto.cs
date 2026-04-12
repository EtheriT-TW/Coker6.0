
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;

namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class FreightDisplayDto
    {
        public long? Id { get; set; }
        public string Title { get; set; }
        public int? Freight { get; set; }
        public int? Low_Con { get; set; }
        public int? Dis_Freight { get; set; }
        public bool Set_Default { get; set; }
        public int FreightStatusType { get; set; }
        public String Describe { get; set; }
        public bool GetMap { get; set; }
        public string LogisticsSubType { get; set; }
        public string CVSStoreID { get; set; }
    }
}
