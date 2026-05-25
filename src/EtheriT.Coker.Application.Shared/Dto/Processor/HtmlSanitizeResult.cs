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

        public string Css { get; set; } = "";

        public string ContentHash { get; set; } = "";

        public string SanitizeVersion { get; set; } = "";

        public string SanitizePolicy { get; set; } = "PublicHtml";

        public bool WasSanitized { get; set; }
    }
}
