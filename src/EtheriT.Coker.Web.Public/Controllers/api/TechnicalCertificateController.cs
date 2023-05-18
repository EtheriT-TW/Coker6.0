using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EtheriT.Coker.Application.Freight;

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

        [HttpPost]
        public async Task<List<TechCertDisplayDto>> GetDisplayData(LongIdDto dto)
        {
            return await technicalCertificateAppService.GetDisplayData(dto);
        }
    }
}
