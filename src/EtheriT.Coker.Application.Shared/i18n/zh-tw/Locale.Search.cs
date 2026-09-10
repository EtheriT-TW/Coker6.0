using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        public static string All { get; } = "全部";
        public static string FindAll { get; } = "找全部";
        public static string FindArticle { get; } = "找文章";
        public static string FindProduct { get; } = "找商品";
        public static string FindDescription { get; } = "搜尋 {0}共 {1}筆 資料";
        public static string ClearAll { get; } = "全部清空";
        public static string GridImage { get; } = "圖片";
        public static string GridImageAndText { get; } = "圖文";
        public static string GridText { get; } = "文字";
        public static string SearchProdPlaceholder { get; } = "請輸入商品名稱/型號";
        public static string SearchAllPlaceholder { get; } = "尋找全站資訊";
        public static string FindEmpty { get; } = "查無資料";
        public static string SiteSearch { get; } = "站內搜尋";
        public static string Search { get; } = "搜尋";
        public static string GoToSearch { get; } = "前往搜尋頁";
        public static string AdvancedFilter { get; } = "進階篩選";
        public static string Category { get; } = "分類";
        public static string SelectCategory { get; } = "請選擇分類";
        public static string SortBy { get; } = "排序";
        public static string SortDefault { get; } = "推薦";
        public static string SortPrice { get; } = "價格";
        public static string SortName { get; } = "名稱";
        public static string SortTitle { get; } = "標題";
        public static string SortModel { get; } = "型號";
        public static string SortPublishDate { get; } = "發布日期";
        public static string SortLastModified { get; } = "最後編輯時間";
        public static string SortOptions { get; } = "選擇排序方式";
        public static string SortAscending { get; } = "升序";
        public static string SortDescending { get; } = "降序";
        public static string SortDirectionUnavailable { get; } = "請先選擇排序欄位";
    }
}
