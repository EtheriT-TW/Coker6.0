using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EtheriT.Coker.Web.Public.Helpers
{
    [HtmlTargetElement("coker-safe-html")]
    public class CokerSafeHtmlTagHelper : TagHelper
    {
        private readonly IHtmlSanitizeService htmlSanitizeService;
        private readonly PreparedHtmlContent preparedHtml;

        public CokerSafeHtmlTagHelper(
            IHtmlSanitizeService htmlSanitizeService,
            PreparedHtmlContent preparedHtml)
        {
            this.htmlSanitizeService = htmlSanitizeService;
            this.preparedHtml = preparedHtml;
        }

        [HtmlAttributeName("content")]
        public string? Content { get; set; }

        [HtmlAttributeName("website-id")]
        public long WebsiteId { get; set; }

        [HtmlAttributeName("source-type")]
        public HtmlSanitizeSourceType SourceType { get; set; }

        [HtmlAttributeName("source-id")]
        public long SourceId { get; set; }

        [HtmlAttributeName("content-key")]
        public string ContentKey { get; set; } = "Default";

        [HtmlAttributeName("sanitize-policy")]
        public string SanitizePolicy { get; set; } = "PublicHtml";

        [HtmlAttributeName("css")]
        public string Css { get; set; } = "";

        [HtmlAttributeName("parent-content")]
        public string? ParentContent { get; set; }

        [HtmlAttributeName("parent-source-id")]
        public long ParentSourceId { get; set; }

        [HtmlAttributeName("parent-css")]
        public string ParentCss { get; set; } = "";

        [HtmlAttributeName("rewrite-upload-paths")]
        public bool RewriteUploadPaths { get; set; }

        [HtmlAttributeName("upload-org-name")]
        public string UploadOrgName { get; set; } = "";

        [HtmlAttributeName("upload-parent-org-names")]
        public string UploadParentOrgNames { get; set; } = "";

        [HtmlAttributeName("content-wrapper-class")]
        public string ContentWrapperClass { get; set; } = "";

        [HtmlAttributeName("breadcrumb-html")]
        public string? BreadcrumbHtml { get; set; }

        [HtmlAttributeName("lazy-load-images")]
        public bool LazyLoadImages { get; set; }

        // 只接收後端產生並清洗的 markup，不得直接傳入編輯器或 request 內容。
        [HtmlAttributeName("generated-html-suffix")]
        public string GeneratedHtmlSuffix { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(GeneratedHtmlSuffix))
            {
                output.Content.Clear();
                return;
            }

            if (WebsiteId <= 0 || SourceId <= 0)
            {
                output.Content.Clear();
                return;
            }

            // 新舊頁面共用：缺檔／資源變更時重建，命中時直接讀取私有產物。
            var initialized = await htmlSanitizeService.EnsurePublicContentAsync(new()
            {
                WebsiteId = WebsiteId,
                SourceType = SourceType,
                SourceId = SourceId,
                Html = Content,
                Css = Css,
                ContentKey = ContentKey,
                SanitizePolicy = SanitizePolicy
            });
            var verifiedContent = initialized.PublishedHtml ?? initialized.Html;

            string? verifiedParentContent = null;
            if (!string.IsNullOrWhiteSpace(ParentContent) && ParentSourceId > 0)
            {
                var initializedParent = await htmlSanitizeService.EnsurePublicContentAsync(new()
                {
                    WebsiteId = WebsiteId,
                    SourceType = HtmlSanitizeSourceType.選單,
                    SourceId = ParentSourceId,
                    Html = ParentContent,
                    Css = ParentCss,
                    ContentKey = "Published",
                    SanitizePolicy = SanitizePolicy
                });
                verifiedParentContent = initializedParent.PublishedHtml ?? initializedParent.Html;
            }
            var rendered = await preparedHtml.GetAsync(UploadOrgName, WebsiteId,
                $"tag/{(int)SourceType}/{SourceId}/{ContentKey}/{SanitizePolicy}/{ParentSourceId}",
                verifiedContent, verifiedParentContent, ContentWrapperClass, LazyLoadImages, BreadcrumbHtml,
                RewriteUploadPaths, UploadParentOrgNames, GeneratedHtmlSuffix);
            // 子內容與父選單都在後端完成 hash 驗證後，才組合並輸出 HTML。
            output.Content.SetHtmlContent(rendered.Html);
        }
    }
}
