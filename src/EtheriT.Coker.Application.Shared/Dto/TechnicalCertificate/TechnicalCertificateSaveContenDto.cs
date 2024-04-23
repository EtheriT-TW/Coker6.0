using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
    public class TechnicalCertificateSaveContenDto
    {
        public int Id { get; set; }
        public string? SaveHtml { get; set; }
        public string? SaveCss { get; set; }
    }
}
