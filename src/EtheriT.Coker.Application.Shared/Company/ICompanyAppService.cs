using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Company
{
    public interface ICompanyAppService
    {
        public Task<OutputCompanyDto> Get();
        public Task<OutputCompanyDto> GetByTaxId(string taxId);
        public Task<OutputCompanyDto> GetByIdentity(string? taxId, string? name);
        public Task<ResponseMessageDto> Save(CompanyDto dto);
    }
}
