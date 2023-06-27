
namespace EtheriT.Coker.Application.Shared.Dto.Tag
{
    public class TagGroupGetAllListDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public List<long>? FK_Tid { get; set; }
        public List<string>? TagTitle { get; set; }
        public bool? Disp_Opt { get; set; }
    }
}
