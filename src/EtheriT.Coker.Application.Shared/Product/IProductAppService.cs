using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Import;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Role;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EtheriT.Coker.Application.Shared.Product
{
    public interface IProductAppService
    {
        public Task<ResponseMessageDto> ProductAddUp(ProdAddUpDto dto);
        public Task<ResponseMessageDto> StockAddUp(long Pid, List<ProductStockDto> dto);
        public Task<ResponseMessageDto> PriceAddUp(List<ProductPriceDto> dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> SaleQuantityStaging(DataSourceLoadOptions loadOptions);
        public Task<ProdGetDataDto> GetProdDataOne(long Id);
        public Task<List<ProductStockDto>> GetStockDataAll(long PId);
        public Task<JsonResult> GetRolesAll();
        public Task<List<ProductPriceDto>> GetPriceDataAll(long PSId);
        public Task<List<ProductPriceDto>> GetPriceByStock(List<long> PSIds);
        public Task<ProdGetMainDisplayDto> GetMainDisplayOne(long Id);
        public Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(DirectoryReleInfoInputDto dto);
        public Task<ProdGetOneDto> GetDisplayOne(long id);
        public Task<List<ProductStockDto>> GetDisplayStock(long id);
        public Task<ProdGetDisplayDto> GetDisplaySimple(long id);
        public Task<JsonResult> GetRandomDIsplay(long webid, int num);
        public Task<ProdGetHistoryDisplayAllDto> GetHistoryDisplay(int page);
        public Task<ResponseMessageDto> ProdDelete(long Id);
        public Task<ResponseMessageDto> StockDelete(long Id);
        public Task<ResponseMessageDto> PriceDelete(long Id);
        public Task<ResponseMessageDto> ClickLog(long FK_Pid);
        public Task<ImportOutputDto> ProdReplace(IList<IFormFile> files);
        public Task<GetProdContenDto> GetConten(SearchIDDto dto);
        public Task<ResponseMessageDto> ImportConten(ProdSaveContenDto dto);
        public Task<ResponseMessageDto> SaveConten(ProdSaveContenDto dto);
        public Task<GetFrontContenOutputDto> GetFrontConten(ProdGetFrontContenInputDto dto);

    }
}
