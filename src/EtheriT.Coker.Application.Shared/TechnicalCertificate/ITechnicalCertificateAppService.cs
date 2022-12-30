using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.TechnicalCertificate
{
    public interface ITechnicalCertificateAppService
    {
        public Task<ResponseMessageDto> AddUp(TechCertDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetChoseList(DataSourceLoadOptions loadOptions);
        public Task<TechCertDto> GetOne(int id);
        public Task<ResponseMessageDto> Delete(long Id);
    }
}
