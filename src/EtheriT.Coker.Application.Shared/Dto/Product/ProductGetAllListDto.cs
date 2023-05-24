
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductGetAllListDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public bool Disp_Opt { get; set; }
        public int Ser_No { get; set; }
        public string Price { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool Permanent { get; set; }
    }
}
