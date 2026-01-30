using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        public static string MailtoHintTitle { get; } = "Email 分享說明";
        public static string MailtoHintBody { get; } = "本功能會使用您電腦的<strong>預設郵件程式</strong>（如 Outlook、Mail、Gmail Web）。<br><br>若點擊後未自動開啟郵件視窗，可能原因包含：<br>• 尚未設定預設郵件程式<br>• 瀏覽器或公司資安政策限制<br>• 裝置未安裝郵件應用程式<br><br>您可確認系統的預設郵件設定，或改用「複製連結」自行貼到 Email。";
        public static string MailtoHintOk { get; } = "我知道了，繼續";
        public static string MailtoHintCancel { get; } = "取消";
        public static string MailtoHintMobileTitle { get; } = "Email 分享提醒";
        public static string MailtoHintMobileBody { get; } = "若未自動開啟郵件 App，請確認是否已設定預設郵件程式，或改用複製連結方式分享。";
    }
}
