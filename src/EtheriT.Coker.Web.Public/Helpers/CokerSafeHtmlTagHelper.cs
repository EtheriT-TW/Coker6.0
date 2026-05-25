using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Processor;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EtheriT.Coker.Web.Public.Helpers
{
    [HtmlTargetElement("coker-safe-html")]
    public class CokerSafeHtmlTagHelper : TagHelper
    {
        private readonly IHtmlSanitizeService htmlSanitizeService;

        public CokerSafeHtmlTagHelper(IHtmlSanitizeService htmlSanitizeService)
        {
            this.htmlSanitizeService = htmlSanitizeService;
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
                ContentKey,
                SanitizePolicy
            );

            if (!isCurrent)
            {
                // 前台不重新清洗。
                // 若狀態不存在或版本不符，直接不輸出，避免未確認 HTML 顯示到前台。
                output.Content.Clear();
                return;
            }

            // 注意：這不是清洗。
            // 只是將「已通過後台清洗且 HtmlSanitizeState 驗證通過」的內容輸出成 HTML。
            output.Content.SetHtmlContent(Content);
        }
    }
}