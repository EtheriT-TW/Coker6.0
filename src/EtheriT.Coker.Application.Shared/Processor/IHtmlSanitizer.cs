using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Processor
{
    public interface IHtmlSanitizer
    {
        public string NormalizeLegacyAttributes(string html);
        public string RepairLegacyPublishedHtml(string publishedHtml, string editorHtml);
        public string SanitizePublicHtml(string html);
        public string SanitizePublicCss(string css);
    }
}
