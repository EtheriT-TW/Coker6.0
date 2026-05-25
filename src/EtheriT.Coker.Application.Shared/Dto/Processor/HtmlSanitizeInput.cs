using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Processor
{
    public class HtmlSanitizeInput
    {
        public long WebsiteId { get; set; }

        public HtmlSanitizeSourceType SourceType { get; set; }

        public long SourceId { get; set; }

        public string ContentKey { get; set; } = "Default";

        public string SanitizePolicy { get; set; } = "PublicHtml";

        /// <summary>
        /// 請傳入已 Decode 的 HTML。
        /// Service 回傳的 Html 也是已清洗、未 Encode 的 HTML。
        /// 是否 HtmlEncode 存 DB，由使用端決定。
        /// </summary>
        public string Html { get; set; } = "";

        public string Css { get; set; } = "";

        /// <summary>
        /// 強制重新清洗，不管 state / hash 是否一致。
        /// </summary>
        public bool Force { get; set; } = false;
    }
}
