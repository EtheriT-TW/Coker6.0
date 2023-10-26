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
        [HttpPost]
        public async Task<ResponseMessageDto> Save(CompanyDto dto)
        {
            return await companyAppService.Save(dto);
        }
    }
}