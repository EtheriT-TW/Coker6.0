
namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileGetArticleDisplayDto
    {
        public long Id { get; set; }
        public int FileType { get; set; }
        public string Name { get; set; }
        public List<string> Link { get; set; }
        public int SerNo { get; set; }
        public bool isVisible { get; set; }
        public bool isEncryption { get; set; }
        public string areakey { get; set; }
    }
}
