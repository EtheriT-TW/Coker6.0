
namespace EtheriT.Coker.Application.Shared.Dto.Article
{
	public class ArticleSaveContenDto
    {
        public int Id { get; set; }
        public string? SaveHtml { get; set; }
        public string? SaveCss { get; set; }
        public bool? EditorContentChanged { get; set; }
    }
}
