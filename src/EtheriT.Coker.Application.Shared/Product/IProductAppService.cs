using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Product
{
	public interface IProductAppService
	{
        public Task<ResponseMessageDto> AddUp(ProductDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ProductDto> GetOne(long Id);
        public Task<ProdGetOneDto> GetDisplayOne(long id);
        public Task<List<long>> GetRandomId(int num);
        public Task<ResponseMessageDto> Delete(long Id);
        public Task<ResponseMessageDto> ClickLog(ProductLogDto dto);
    }
}
