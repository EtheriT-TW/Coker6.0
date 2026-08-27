using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Web.Public.Helpers
{
    [HtmlTargetElement("coker-safe-inherited-content")]
    public class CokerSafeInheritedContentTagHelper : TagHelper
    {
        private readonly IHtmlSanitizeService htmlSanitizeService;
        private readonly IHtmlProcessor htmlProcessor;

        public CokerSafeInheritedContentTagHelper(
            IHtmlSanitizeService htmlSanitizeService,
            IHtmlProcessor htmlProcessor)
        {
            this.htmlSanitizeService = htmlSanitizeService;
            this.htmlProcessor = htmlProcessor;
        }

        [HtmlAttributeName("website-id")]
        public long WebsiteId { get; set; }

        [HtmlAttributeName("parent-content")]
        public string? ParentContent { get; set; }

        [HtmlAttributeName("parent-source-id")]
        public long ParentSourceId { get; set; }

        [HtmlAttributeName("parent-css")]
        public string ParentCss { get; set; } = "";

        [HtmlAttributeName("sanitize-policy")]
        public string SanitizePolicy { get; set; } = "PublicHtml";

        [HtmlAttributeName("rewrite-upload-paths")]
        public bool RewriteUploadPaths { get; set; }

        [HtmlAttributeName("upload-org-name")]
        public string UploadOrgName { get; set; } = "";

        [HtmlAttributeName("upload-parent-org-names")]
        public string UploadParentOrgNames { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            var childContent = await output.GetChildContentAsync();
            var renderedHtml = childContent.GetContent();

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

            output.Content.SetHtmlContent(renderedHtml);
        }
    }
}
