
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;

namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class FreightDto
    {
        public long? Id { get; set; }
        public Guid TId { get; set; }
        public string Title { get; set; }
        public int PreserveType { get; set; }
        public int LogisticsType { get; set; }
        public int FreigntType { get; set; }
        public int? Freight { get; set; }
        public int? Low_Con { get; set; }
        public int? Dis_Freight { get; set; }
        public bool Set_Default { get; set; }
        public int? FreigntAmt2 { get; set; }
    }
}
