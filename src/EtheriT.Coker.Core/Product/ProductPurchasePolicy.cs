using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Core.Models;

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
            if (product.IsDeleted || !product.Visible || product.RemovedFromShelves) return "商品已下架";
            if (product.Status == ProdStatusEnum.售完) return "商品已售完";
            if (product.Status == ProdStatusEnum.停產) return "商品已停產";
            if (!product.permanent &&
                (!product.StartTime.HasValue || !product.EndTime.HasValue ||
                 currentTime <= product.StartTime.Value || currentTime >= product.EndTime.Value))
                return "目前不在商品販售期間";
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
            if (stock.IsDeleted) return "此規格已下架";
            if (stock.IsTimePrice) return "此規格為時價商品";
            if (!hasPrice) return "此規格目前沒有可購買價格";
            if (!product.NoStockManagement && GetMaxPurchaseQuantity(product, stock) <= 0)
                return "已售完";
            return null;
        }
    }
}
