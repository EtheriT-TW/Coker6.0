using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Role;

namespace EtheriT.Coker.Application.Shared.Product
{
    public interface IProductDisplayPriceService
    {
        Task<Dictionary<long, DirectoryPriceResultDto>> GetDirectoryPriceMapAsync(
            List<long> productIds,
            FrontRoleContextDto roleContext,
            bool orderLowToHigh
        );
        Task<DirectoryPriceResultDto?> GetProductDisplayPriceAsync(
            long productId,
            FrontRoleContextDto roleContext,
            bool orderLowToHigh
        );
        Task<List<ProductPriceDto>> GetDisplayPricesByStockAsync(
            List<long> stockIds,
            FrontRoleContextDto roleContext
        );
    }
}
