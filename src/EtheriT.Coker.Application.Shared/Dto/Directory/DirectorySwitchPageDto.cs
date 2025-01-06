
namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectorySwitchPageDto
    {
        public long id { get; set; }
        public List<long>? dirids { get; set; }
        public string routername { get; set; }
        public int type { get; set; }
    }
}
