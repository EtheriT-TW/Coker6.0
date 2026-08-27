using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Helpers
{
    [HtmlTargetElement("coker-safe-html")]
    public class CokerSafeHtmlTagHelper : TagHelper
    {
        private readonly IHtmlSanitizeService htmlSanitizeService;
        private readonly IHtmlProcessor htmlProcessor;

        public CokerSafeHtmlTagHelper(
            IHtmlSanitizeService htmlSanitizeService,
            IHtmlProcessor htmlProcessor)
        {
            this.htmlSanitizeService = htmlSanitizeService;
            this.htmlProcessor = htmlProcessor;
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

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            if (string.IsNullOrWhiteSpace(Content))
            {
                output.Content.Clear();
                return;
            }

            if (WebsiteId <= 0 || SourceId <= 0)
            {
                output.Content.Clear();
                return;
            }

            var isCurrent = await htmlSanitizeService.IsCurrentAsync(
                WebsiteId,
                SourceType,
                SourceId,
                Content,
                Css,
                ContentKey,
                SanitizePolicy
            );

            var verifiedContent = Content;
            if (!isCurrent)
            {
                // 相容舊資料：第一次前台讀取時清洗並建立／更新 hash state。
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
                verifiedContent = initialized.Html;
            }

            var renderedHtml = verifiedContent;
            if (!string.IsNullOrWhiteSpace(ContentWrapperClass))
            {
                renderedHtml = $"<div class=\"{HtmlEncoder.Default.Encode(ContentWrapperClass)}\">{renderedHtml}</div>";
            }
            if (!string.IsNullOrWhiteSpace(ParentContent) && ParentSourceId > 0)
            {
                var isParentCurrent = await htmlSanitizeService.IsCurrentAsync(
                    WebsiteId,
                    HtmlSanitizeSourceType.選單,
                    ParentSourceId,
                    ParentContent,
                    ParentCss,
                    "Published",
                    SanitizePolicy
                );

                var verifiedParentContent = ParentContent;
                if (!isParentCurrent)
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
                    verifiedParentContent = initializedParent.Html;
                }

                renderedHtml = MainHeadingHtml.PrepareInheritedContent(
                    htmlProcessor,
                    renderedHtml,
                    verifiedParentContent);
            }
            else
            {
                renderedHtml = MainHeadingHtml.PrepareInheritedContent(
                    htmlProcessor,
                    renderedHtml,
                    null);
            }

            if (RewriteUploadPaths && !string.IsNullOrWhiteSpace(UploadOrgName))
            {
                var parentOrgNames = Regex.Escape(UploadParentOrgNames ?? string.Empty);
                renderedHtml = Regex.Replace(
                    renderedHtml,
                    $"(?<attr>src|href|data-pdf-url)=([\"'])/upload/(?!{parentOrgNames})",
                    $"${{attr}}=$2/upload/{UploadOrgName}/",
                    RegexOptions.IgnoreCase
                );
            }

            // 子內容與父選單都在後端完成 hash 驗證後，才組合並輸出 HTML。
            output.Content.SetHtmlContent(renderedHtml);
        }
    }
}
