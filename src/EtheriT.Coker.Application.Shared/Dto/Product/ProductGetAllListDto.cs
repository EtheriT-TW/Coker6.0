
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductGetAllListDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public bool Disp_Opt { get; set; }
        public int Ser_No { get; set; }
        public string Price { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool Permanent { get; set; }
    }
}
