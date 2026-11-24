using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryAddUpDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public long? FK_Mid { get; set; }
        public int SortBy { get; set; }
        public bool Visible { get; set; }
        // 文章目錄使用的樣板代碼；非文章目錄一律存 null。
        public string? ArticleTemplateKey { get; set; }
        public List<TagSelectedDto> TagSelected { get; set; } = new List<TagSelectedDto>();
        public List<TagSelectedDto> ReverseTagSelected { get; set; } = new List<TagSelectedDto>();
    }
}
