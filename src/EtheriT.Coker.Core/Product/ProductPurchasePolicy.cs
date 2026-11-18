using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.i18n;

namespace EtheriT.Coker.Core.Product
{
    /// <summary>
    /// 商品與規格是否可購買的唯一領域規則。
    /// 顯示端及購物車驗證皆應使用此 Policy，避免各入口自行解讀狀態與庫存。
    /// </summary>
    public static class ProductPurchasePolicy
    {
        public static bool CanPurchaseProduct(Prod product, DateTime? now = null)
        {
            var currentTime = now ?? DateTime.Now;
            return !product.IsDeleted &&
                   product.Visible &&
                   !product.RemovedFromShelves &&
                   product.Status != ProdStatusEnum.售完 &&
                   product.Status != ProdStatusEnum.停產 &&
                   (product.permanent ||
                    (product.StartTime.HasValue && product.EndTime.HasValue &&
                     currentTime > product.StartTime.Value && currentTime < product.EndTime.Value));
        }

        public static string? GetProductUnavailableReason(Prod product, DateTime? now = null)
        {
            var currentTime = now ?? DateTime.Now;
            if (product.IsDeleted || !product.Visible || product.RemovedFromShelves) return L.get("ProductUnavailable");
            if (product.Status == ProdStatusEnum.售完) return L.get("ProdEmpty");
            if (product.Status == ProdStatusEnum.停產) return L.get("ProductDiscontinued");
            if (!product.permanent &&
                (!product.StartTime.HasValue || !product.EndTime.HasValue ||
                 currentTime <= product.StartTime.Value || currentTime >= product.EndTime.Value))
                return L.get("ProductNotInSalePeriod");
            return null;
        }

        public static int? GetMaxPurchaseQuantity(Prod product, Prod_Stock stock)
        {
            if (product.NoStockManagement) return null;
            var minimum = Math.Max(stock.Min_Qty ?? 1, 1);
            var available = Math.Max(stock.Stock ?? 0, 0);
            return available - available % minimum;
        }

        public static bool CanPurchaseStock(Prod product, Prod_Stock stock, bool hasPrice, DateTime? now = null)
        {
            return CanPurchaseProduct(product, now) &&
                   !stock.IsDeleted &&
                   !stock.IsTimePrice &&
                   hasPrice &&
                   (product.NoStockManagement || GetMaxPurchaseQuantity(product, stock) > 0);
        }

        public static string? GetStockUnavailableReason(Prod product, Prod_Stock stock, bool hasPrice, DateTime? now = null)
        {
            var productReason = GetProductUnavailableReason(product, now);
            if (productReason != null) return productReason;
            if (stock.IsDeleted) return L.get("StockRemoved");
            if (stock.IsTimePrice) return L.get("StockIsTimePrice");
            if (!hasPrice) return L.get("StockNoPrice");
            if (!product.NoStockManagement && GetMaxPurchaseQuantity(product, stock) <= 0)
                return L.get("StockSoldOut");
            return null;
        }
    }
}
