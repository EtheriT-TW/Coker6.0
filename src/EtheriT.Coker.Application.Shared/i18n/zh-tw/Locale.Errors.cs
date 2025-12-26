using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class Locale
    {
        public static string PageNotFound { get; } = "頁面不存在";
        public static string PageNotFoundDescription { get; } = "請確認您的連結是否正確，或";
        public static string NoPermission { get; } = "沒有瀏覽權限";
        public static string NoPermissionDescription { get; } = "請確認您的會員身分，或 ";
        public static string returnHomePage { get; } = "回到首頁";
    }
}
