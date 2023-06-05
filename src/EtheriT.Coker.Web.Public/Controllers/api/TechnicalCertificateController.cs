using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TechnicalCertificateController : Controller
    {

        private readonly ITechnicalCertificateAppService technicalCertificateAppService;
        public TechnicalCertificateController(ITechnicalCertificateAppService technicalCertificateAppService)
        {
            this.technicalCertificateAppService = technicalCertificateAppService;
        }
    }
}
