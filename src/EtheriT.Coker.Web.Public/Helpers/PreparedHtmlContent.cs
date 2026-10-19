using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Web.Public.Services;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Helpers;

public sealed class PreparedHtmlContent(PageArtifactStore store, IHtmlProcessor processor)
{
    // 只存佔位標記，當次頁面的麵包屑 markup 不存入產物。
    private const string BreadcrumbSlot = "<!--coker-prepared-breadcrumb-v1-->";
    public sealed class Result
    {
        public string Html { get; set; } = "";
        public int MainHeadingCount { get; set; }
    }

    public async Task<Result> GetAsync(string orgName, long siteId, string slot, string? html, string? parent,
        string wrapper = "", bool lazy = false, string? breadcrumb = null,
        bool rewriteUploads = false, string parentOrgNames = "", string generatedSuffix = "")
    {
        Result Create()
        {
            var content = html ?? "";
            if (!string.IsNullOrWhiteSpace(generatedSuffix))
                content = processor.ExtractBodyInnerHtml(content) + generatedSuffix;
            if (!string.IsNullOrWhiteSpace(wrapper))
                content = $"<div class=\"{HtmlEncoder.Default.Encode(wrapper)}\">{content}</div>";
            var combined = MainHeadingHtml.PrepareDocument(processor, content, parent);
            var root = combined.BodyOnly
                ? combined.Document.DocumentNode.SelectSingleNode("//body")! : combined.Document.DocumentNode;
            var headings = root.Descendants("h1").Count(MainHeadingHtml.HasText);
            BreadcrumbBlockHtml.Inject(processor, combined.Document,
                string.IsNullOrWhiteSpace(breadcrumb) ? null : BreadcrumbSlot);
            if (lazy) ContentImageLoadingHtml.Prepare(processor, combined.Document);
            content = combined.BodyOnly ? root.InnerHtml : root.OuterHtml;
            if (rewriteUploads && !string.IsNullOrWhiteSpace(orgName))
                content = Regex.Replace(content,
                    $"(?<attr>src|href|data-pdf-url)=([\"'])/upload/(?!{Regex.Escape(parentOrgNames)})",
                    $"${{attr}}=$2/upload/{orgName}/", RegexOptions.IgnoreCase);
            return new Result { Html = content, MainHeadingCount = headings };
        }

        // GeneratedHtmlSuffix 可能是即時產生的目錄／表單；不落盤，也不共用不同請求結果。
        var result = !string.IsNullOrWhiteSpace(generatedSuffix)
            ? Create()
            : await store.GetAsync(orgName, siteId,
                $"render/{slot}/{wrapper}/{lazy}/{!string.IsNullOrWhiteSpace(breadcrumb)}/{rewriteUploads}",
                [html, parent, orgName, parentOrgNames], () => Task.FromResult(Create()),
                cached => cached.Html != null && cached.MainHeadingCount >= 0);
        return new Result
        {
            Html = result.Html.Replace(BreadcrumbSlot, breadcrumb ?? "", StringComparison.Ordinal),
            MainHeadingCount = result.MainHeadingCount
        };
    }
}
