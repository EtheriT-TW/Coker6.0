
namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileGetProdDisplayDto
    {
        public long Id { get; set; }
        public int FileType { get; set; }
        public string Name { get; set; }
        public List<string> Link { get; set; }
        public int SerNo { get; set; }
    }
}
