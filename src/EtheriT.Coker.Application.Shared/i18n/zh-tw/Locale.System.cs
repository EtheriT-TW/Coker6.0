using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        public static string Error { get; } = "錯誤";
        public static string InformationError { get; } = "資料錯誤";
        public static string FormSubmitMessage { get; } = "須確實填寫表單資料才可送出";
        public static string NoSelectSender { get; } = "請選擇寄件人";
        public static string SentSuccessfully { get; } = "成功送出表單！";
        public static string FailedToSend { get; } = "發送失敗！";
        public static string VerificationCodeError { get; } = "驗證碼錯誤";
        public static string WebsiteDataError { get; } = "網站資料錯誤";
    }
}
