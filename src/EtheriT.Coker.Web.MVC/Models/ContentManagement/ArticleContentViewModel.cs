namespace EtheriT.Coker.Web.MVC.Models.ContentManagement;

public sealed class ArticleContentViewModel
{
    public bool ArticleOnly { get; init; }

    public string ListUrl => ArticleOnly
        ? "/api/Article/GetAllList"
        : "/api/Directory/GetDirectoryDetailList";
}
