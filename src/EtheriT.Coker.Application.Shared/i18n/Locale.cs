using DevExtreme.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static class Locale
    {
        public static string FindAll { get; } = "找全部"; 
        public static string FindArticle { get; } = "找文章";
        public static string FindProduct { get; } = "找商品";
        public static string FindDescription { get; } = "搜尋 {0}共 {1}筆 資料";
        public static string GridImage { get; } = "圖片";
        public static string GridImageAndText { get; } = "圖文";
        public static string GridText { get; } = "文字";
        public static string SearchProdPlaceholder { get; } = "請輸入商品名稱/型號";
        public static string SearchAllPlaceholder { get; } = "尋找全站資訊";
        public static string FindEmpty { get; } = "查無資料";
        public static string shareTo { get; } = "分享至：{0} (另開新視窗)";
        public static string LinkTo { get; } = "連結至：{0}";
        public static string LinkToAndBlank { get; } = "連結至：{0}(另開新視窗)";
        public static string cookieDeclare{ get; } = "本網站使用cookies。使用我們的網站並同意隱私權政策，即表示您同意我們根據本政策內的條款使用cookie。如果您不同意，請按照說明停用，本網站的cookie即不會安裝於您的裝置上。了解更多請見 隱私權政策。";
        public static string accept { get; } = "接受";
        public static string reject { get; } = "拒絕";
        public static string HomePage { get; } = "首頁";
        public static string accesskeyTop { get; } = "上方自訂區";
        public static string accesskeyJumpMain { get; } = "跳到主要內容區塊";
        public static string accesskeyCenter { get; } = "中間區域";
        public static string noJavascript { get; } = "你的瀏覽器並未啟動 Javascript!";
        public static string noJavascriptMemo { get; } = "請啟動瀏覽器的 JavaScript 或是升級成可執行 JavaScript 的瀏覽器，以便正常使用網頁功能。";
        public static string Error { get; } = "錯誤";
        public static string InformationError { get; } = "資料錯誤";
        public static string FormSubmitMessage { get; } = "須確實填寫表單資料才可送出"; 
        public static string NoSelectSender { get; } = "請選擇寄件人";
        public static string SentSuccessfully { get; } = "成功送出表單！";
        public static string FailedToSend { get; } = "發送失敗！";
        public static string VerificationCodeError { get; } = "驗證碼錯誤";
        public static string WebsiteDataError { get; } = "網站資料錯誤";
        public static string UnknownSource { get; } = "來源不明";
        public static string PathError { get; } = "路徑錯誤";
        public static string SiteSearch { get; } = "站內搜尋";
        public static string Search { get; } = "搜尋";
        public static string GoToSearch { get; } = "前往搜尋頁";
        public static string UnnamedFile { get; } = "未命名檔案";
        public static string ProdEmpty { get; } = "商品已售完";
        public static string ServiceCenter { get; } = "客服中心";
    }
}
