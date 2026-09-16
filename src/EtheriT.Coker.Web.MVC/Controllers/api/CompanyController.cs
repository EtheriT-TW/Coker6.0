using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ICompanyAppService companyAppService;
        public CompanyController(ICompanyAppService companyAppService) { 
            this.companyAppService = companyAppService;
        }
        [HttpGet]
        public async Task<OutputCompanyDto> GetByTaxId(string taxId)
        {
            return await companyAppService.GetByTaxId(taxId);
        }
        [HttpGet]
        public async Task<OutputCompanyDto> GetByIdentity(string? taxId, string? name)
        {
            return await companyAppService.GetByIdentity(taxId, name);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Save(CompanyDto dto)
        {
            return await companyAppService.Save(dto);
        }
    }
}
