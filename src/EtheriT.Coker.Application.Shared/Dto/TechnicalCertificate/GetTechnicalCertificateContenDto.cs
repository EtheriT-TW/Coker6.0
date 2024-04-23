using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
    public class GetTechnicalCertificateContenDto: ResponseMessageDto
    {
        public string? Title { get; set; }
        public TechnicalCertificateSaveContenDto? Conten { get; set; }
    }
}
