using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.TechnicalCertificate
{
    public interface ITechnicalCertificateAppService
    {
        public Task<ResponseMessageDto> AddUp(TechCertDto dto);
		public Task<ImportOutputDto> AddAll(List<TechCertDto> dto);
		public Task<ResponseMessageDto> TechCertAssociateAddDelect(List<TechCertProdAssociateDto> dto);
        public Task<List<TechCertGetSelectedDto>> GetTechCertAssociate(long Pid);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<List<TechCertDisplayDto>> GetDisplayData(long pid);
        public Task<TechCertDto> GetOne(int id);
        public Task<GetTechnicalCertificateContenDto> GetConten(SearchIDDto dto);
        public Task<ResponseMessageDto> SaveConten(TechnicalCertificateSaveContenDto dto);
        public Task<ResponseMessageDto> Delete(long Id);
        public Task<ResponseMessageDto> TechCertAssociateDelete(long Id);
        public Task<GetFrontContenOutputDto> GetFrontConten(TechCertGetFrontContenInputDto dto);
    }
}
