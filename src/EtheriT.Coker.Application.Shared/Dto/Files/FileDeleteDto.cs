
namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileDeleteDto
    {
        public long Sid { get; set; }
        public int Type { get; set; }
        public List<long>? Fid { get; set; }
    }
}
