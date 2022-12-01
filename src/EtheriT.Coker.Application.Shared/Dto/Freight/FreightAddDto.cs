
namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class FreightAddDto
    {
        public string Title { get; set; }
        public int Preserve { get; set; }
        public int Shipping { get; set; }
        public bool IsFree { get; set; }
        public int? Ori_Freight { get; set; }
        public int? Low_Con { get; set; }
        public int? Dis_Freight { get; set; }
        public bool Set_Default { get; set; }
    }
}
