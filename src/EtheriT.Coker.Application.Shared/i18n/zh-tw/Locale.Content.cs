using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        public static string UnknownSource { get; } = "來源不明";
        public static string PathError { get; } = "路徑錯誤";
        public static string UnnamedFile { get; } = "未命名檔案";
        public static string ProdEmpty { get; } = "商品已售完";
        public static string ServiceCenter { get; } = "客服中心";
        public static string Other { get; } = "其他";
        public static string Specification { get; } = "規格";
        public static string ProductDescription { get; } = "產品說明";
        public static string CertificationMark { get; } = "標章認證";
        public static string DownloadFile { get; } = "檔案下載";
        public static string LearnMore { get; } = "了解更多";
        public static string AddtoCart { get; } = "加入購物車";
        public static string MarketPrice { get; } = "時價";
        public static string Bonus { get; } = "紅利";
        public static string Prev { get; } = "上一則";
        public static string Next { get; } = "下一則";
        public static string BackToList { get; } = "回列表頁";
        public static string LastModifiedTime { get; } = "最後編輯時間";
        public static string ViewCount { get; } = "人氣值";
        public static string Detail { get; } = "詳細介紹";
        public static string DirectoryFallback { get; } = "{0}分類頁，彙整相關內容與最新更新。";
        public static string AddCartNeedPrivacy { get; } = "若要進行商品選購，請先同意隱私權政策";
        public static string BonusInsufficient { get; } = "紅利不足";
        public static string AddCartSuccess { get; } = "商品已成功加入購物車";
        public static string AddCartError { get; } = "商品加入購物車發生錯誤";
        public static string StockNotEnough { get; } = "商品庫存不足";
        public static string SpecNotFound { get; } = "查無商品規格";
        public static string ProductUnavailable { get; } = "商品已下架";
        public static string OutOfStock { get; } = "目前無庫存";
        public static string CartNotFound { get; } = "查無購物車資料";
        public static string CartLimitExceeded { get; } = "可購買上限 {0} 件，無法再加入 {1} 件";
        public static string CartLimitExceededWithCart { get; } = "可購買上限 {0} 件（購物車已有 {1} 件），無法再加入 {2} 件";
        public static string BonusNotEnoughExceeded { get; } = "會員紅利不足，目前可用 {0:N0} 點，購物車已需 {1:N0} 點，已超出 {2:N0} 點";
        public static string BonusNotEnoughShortfall { get; } = "會員紅利不足，目前可用 {0:N0} 點，購物車已需 {1:N0} 點，本次新增需 {2:N0} 點，尚差 {3:N0} 點";
        public static string SuggestedPrice { get; } = "建議售價";
        public static string AddCartNeedSelection { get; } = "請確實選擇規格及購買數量";
        public static string BonusApplied { get; } = "含紅利折抵";
        public static string NonMember { get; } = "非會員";
        public static string RolePriceLabel { get; } = "{0}價";
        public static string GoToSlide { get; } = "切換至第 {0} 張";
        public static string Quantity { get; } = "數量";
        public static string Increase { get; } = "增加";
        public static string Decrease { get; } = "減少";
        public static string CartEmpty { get; } = "購物車尚無商品";
        public static string Checkout { get; } = "結帳";
        public static string Buy { get; } = "購買";
    }
}
