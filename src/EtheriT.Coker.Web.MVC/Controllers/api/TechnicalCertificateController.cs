using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TechnicalCertificateController : Controller
    {

        private readonly ITechnicalCertificateAppService technicalCertificateAppService;
        public TechnicalCertificateController(
            ITechnicalCertificateAppService technicalCertificateAppService
            )
        {
            this.technicalCertificateAppService = technicalCertificateAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(TechnicalCertificateDto dto)
        {
            return await technicalCertificateAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await technicalCertificateAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<List<TechnicalCertificateGetAllDto>> GetAll()
        {
            return await technicalCertificateAppService.GetAll();
        }
        [HttpGet]
        public async Task<TechnicalCertificateDto> GetOne(int id)
        {
            return await technicalCertificateAppService.GetOne(id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Delete(TechnicalCertificateDelectDto dto)
        {
            return await technicalCertificateAppService.Delete(dto);
        }
    }
}

