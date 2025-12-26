using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        public static string Share { get; } = "分享";
        public static string shareTo { get; } = "分享至：{0} (另開新視窗)";
        public static string LinkTo { get; } = "連結至：{0}";
        public static string LinkToAndBlank { get; } = "連結至：{0}(另開新視窗)";
        public static string cookieDeclare { get; } = "本網站使用cookies。使用我們的網站並同意隱私權政策，即表示您同意我們根據本政策內的條款使用cookie。如果您不同意，請按照說明停用，本網站的cookie即不會安裝於您的裝置上。了解更多請見 隱私權政策。";
        public static string accept { get; } = "接受";
        public static string reject { get; } = "拒絕";
        public static string HomePage { get; } = "首頁";
        public static string accesskeyTop { get; } = "上方自訂區";
        public static string accesskeyJumpMain { get; } = "跳到主要內容區塊";
        public static string accesskeyCenter { get; } = "中間區域";
        public static string noJavascript { get; } = "你的瀏覽器並未啟動 Javascript!";
        public static string noJavascriptMemo { get; } = "請啟動瀏覽器的 JavaScript 或是升級成可執行 JavaScript 的瀏覽器，以便正常使用網頁功能。";
        public static string AlertTitle { get; } = "請注意";
        public static string Yes { get; } = "是";
        public static string No { get; } = "否";
        public static string Confirm { get; } = "確定";
        public static string Cancel { get; } = "取消";
    }
}
