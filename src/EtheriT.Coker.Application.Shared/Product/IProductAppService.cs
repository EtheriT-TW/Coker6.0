using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Product
{
    public interface IProductAppService
    {
        public Task<ResponseMessageDto> ProductAddUp(ProdAddUpDto dto);
        public Task<ResponseMessageDto> StockAddUp(long Pid, List<ProductStockDto> dto);
        public Task<ResponseMessageDto> PriceAddUp(List<ProductPriceDto> dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ProdGetDataDto> GetProdDataOne(long Id);
        public Task<List<ProductStockDto>> GetStockDataAll(long PId);
        public Task<List<ProductPriceDto>> GetPriceDataAll(long PSId);
        public Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(List<long> Ids);
        public Task<List<ProdIdTitleDto>> GetSpecType();
        public Task<List<ProdIdTitleDto>> GetSpecDetail(long typeid);
        public Task<ProdGetOneDto> GetDisplayOne(long id);
        public Task<List<ProductStockDto>> GetDisplayStock(long id);
        public Task<ProdGetDisplayDto> GetDisplaySimple(long id);
        public Task<JsonResult> GetRandomDIsplay(long webid, int num);
        public Task<List<ProdDisImgDto>> GetHistoryDisplay(Guid TId);
        public Task<ResponseMessageDto> ProdDelete(long Id);
        public Task<ResponseMessageDto> StockDelete(long Id);
        public Task<ResponseMessageDto> PriceDelete(long Id);
        public Task<ResponseMessageDto> ClickLog(ProductLogDto dto);
    }
}
