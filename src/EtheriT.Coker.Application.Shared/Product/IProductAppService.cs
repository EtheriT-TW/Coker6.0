using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Product
{
    public interface IProductAppService
    {
        public Task<ResponseMessageDto> ProductAddUp(ProductDto dto);
        public Task<ResponseMessageDto> StockAddUp(ProductStockDto dto);
        public Task<ResponseMessageDto> TechCertAddUp(ProductTechCertDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ProductDto> GetProdDataOne(long Id);
        public Task<List<ProductStockDto>> GetStockDataAll(long PId);
        public Task<List<TechnicalCertificateGetAllDto>> GetTechCertDataAll(long PId);
        public Task<ProdGetOneDto> GetDisplayOne(long id);
        public Task<List<ProductStockDto>> GetDisplayStock(long id);
        public Task<ProdGetDisplayDto> GetDisplaySimple(long id);
        public Task<JsonResult> GetRandomDIsplay(long webid, int num);
        public Task<List<ProdIdTitleDto>> GetSpecType();
        public Task<List<ProdIdTitleDto>> GetSpecDetail(long typeid);
        public Task<ResponseMessageDto> ProdDelete(DataDelectDto dto);
        public Task<ResponseMessageDto> StockDelete(DataDelectDto dto);
        public Task<ResponseMessageDto> ClickLog(ProductLogDto dto);
    }
}
