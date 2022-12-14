
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.TechnicalCertificate
{
	public interface ITechnicalCertificateAppService
    {
        public Task<ResponseMessageDto> AddUp(TechnicalCertificateDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<TechnicalCertificateDto> GetOne(int id);
        public Task<ResponseMessageDto> Delete(TechnicalCertificateDelectDto dto);
    }
}
