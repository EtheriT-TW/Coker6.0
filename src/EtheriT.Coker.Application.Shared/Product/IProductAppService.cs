using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Product
{
    public interface IProductAppService
    {
        public Task<ResponseMessageDto> ProductAddUp(ProductDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ProductDto> ProdGetOne(long Id);
        public Task<List<ProductStockDto>> ProdStockGet(long PId);
        public Task<ProdGetOneDto> GetDisplayOne(long id);
        public Task<List<long>> GetRandomId(int num);
        public Task<List<ProdIdTitleDto>> GetSpecType(long webid);
        public Task<List<ProdIdTitleDto>> GetSpecDetail(long typeid);
        public Task<ResponseMessageDto> ProdDelete(long Id);
        public Task<ResponseMessageDto> ClickLog(ProductLogDto dto);
    }
}
