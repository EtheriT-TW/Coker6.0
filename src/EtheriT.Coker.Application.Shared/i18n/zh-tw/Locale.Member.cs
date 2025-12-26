using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        /// <summary>
        /// 個人資料
        /// </summary>
        public static string Profile { get; } = "個人資料";

        /// <summary>
        /// 收藏商品
        /// </summary>
        public static string FavoriteProducts { get; } = "收藏商品";

        /// <summary>
        /// 瀏覽紀錄
        /// </summary>
        public static string BrowsingHistory { get; } = "瀏覽紀錄";
        /// <summary>
        /// 姓名
        /// </summary>
        public static string Name { get; } = "姓名";

        /// <summary>
        /// 性別
        /// </summary>
        public static string Gender { get; } = "性別";

        public static string GenderMale { get; } = "男";
        public static string GenderFemale { get; } = "女";
        public static string GenderOther { get; } = "其他";

        /// <summary>
        /// 生日
        /// </summary>
        public static string Birthday { get; } = "生日";

        /// <summary>
        /// 手機號碼
        /// </summary>
        public static string MobilePhone { get; } = "手機號碼";
        public static string PhoneAreaCode { get; } = "電話區碼";
        public static string PhoneNumber { get; } = "聯絡電話";
        public static string PhoneExtension { get; } = "分機";

        public static string City { get; } = "縣市";
        public static string District { get; } = "鄉鎮市區";
        public static string Address { get; } = "地址";
        /// <summary>
        /// 修改儲存
        /// </summary>
        public static string SaveChanges { get; } = "修改儲存";

        /// <summary>
        /// 密碼修改
        /// </summary>
        public static string ChangePassword { get; } = "密碼修改";

        /// <summary>
        /// 電子郵件修改
        /// </summary>
        public static string ChangeEmail { get; } = "電子郵件修改";

        /// <summary>
        /// 登出
        /// </summary>
        public static string Logout { get; } = "登出";
        /// <summary>
        /// 姓名不可為空
        /// </summary>
        public static string ErrorNameRequired { get; } = "姓名不可為空";

        /// <summary>
        /// 歷史訂單
        /// </summary>
        public static string OrderHistory { get; } = "歷史訂單";

        /// <summary>
        /// 新的電子信箱
        /// </summary>
        public static string NewEmail { get; } = "新的電子信箱";

        /// <summary>
        /// 電子郵件不可為空
        /// </summary>
        public static string ErrorEmailRequired { get; } = "電子郵件不可為空";
        /// <summary>
        /// 請輸入區碼
        /// </summary>
        public static string HintEnterAreaCode { get; } = "請輸入區碼";

        /// <summary>
        /// 請輸入聯絡電話
        /// </summary>
        public static string HintEnterPhoneNumber { get; } = "請輸入聯絡電話";

        /// <summary>
        /// 請輸入分機號碼
        /// </summary>
        public static string HintEnterExtension { get; } = "請輸入分機號碼";

        /// <summary>
        /// 請輸入您的地址
        /// </summary>
        public static string HintEnterAddress { get; } = "請輸入您的地址";

        /// <summary>
        /// 地址格式錯誤
        /// </summary>
        public static string ErrorAddressFormatInvalid { get; } = "地址格式錯誤";

        /// <summary>
        /// 查無歷史訂單
        /// </summary>
        public static string EmptyOrderHistory { get; } = "查無歷史訂單";

        /// <summary>
        /// 切換至上一頁
        /// </summary>
        public static string PaginationPrev { get; } = "切換至上一頁";

        /// <summary>
        /// 切換至下一頁
        /// </summary>
        public static string PaginationNext { get; } = "切換至下一頁";

        /// <summary>
        /// 查無收藏紀錄
        /// </summary>
        public static string EmptyFavorites { get; } = "查無收藏紀錄";

        /// <summary>
        /// 圖片
        /// </summary>
        public static string TabImage { get; } = "圖片";

        /// <summary>
        /// 圖文
        /// </summary>
        public static string TabImageText { get; } = "圖文";

        /// <summary>
        /// 分享商品
        /// </summary>
        public static string ShareProduct { get; } = "分享商品";

        /// <summary>
        /// 移除收藏
        /// </summary>
        public static string RemoveFavorite { get; } = "移除收藏";

        /// <summary>
        /// 加入收藏
        /// </summary>
        public static string AddFavorite { get; } = "加入收藏";

        /// <summary>
        /// 查無瀏覽紀錄
        /// </summary>
        public static string EmptyBrowsingHistory { get; } = "查無瀏覽紀錄";

        /// <summary>
        /// ※僅提供六個月紀錄資料
        /// </summary>
        public static string NoteBrowsingHistorySixMonthsOnly { get; } = "※僅提供六個月紀錄資料";

    }
}
