using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Processor
{
    public class HtmlSanitizeResult
    {
        public string Html { get; set; } = "";

        /// <summary>私有產生檔的輸出（含資源版本）。不得回存為編輯器／發布原始內容。</summary>
        public string? PublishedHtml { get; set; }
        public string? FirstImageUrl { get; set; }
        public bool HasImageMetadata { get; set; }

        public string Css { get; set; } = "";

        public string ContentHash { get; set; } = "";

        public string SanitizeVersion { get; set; } = "";

        public string SanitizePolicy { get; set; } = "PublicHtml";

        public bool WasSanitized { get; set; }
    }
}
