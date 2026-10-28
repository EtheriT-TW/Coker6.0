using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class LocaleEn
    {
        public static string PageNotFound { get; } = "Page Not Found";
        public static string PageNotFoundDescription { get; } = "Please check that the URL is correct, or ";
        public static string NoPermission { get; } = "You do not have permission to view this page.";
        public static string NoPermissionDescriptionPrefix { get; } = "Please";
        public static string NoPermissionLoginLinkText { get; } = "log in";
        public static string NoPermissionDescriptionSuffix { get; } = "and verify your membership status, or ";
        public static string returnHomePage { get; } = "return to the homepage";
    }
}
