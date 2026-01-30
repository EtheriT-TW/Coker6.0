using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static partial class LocaleEn
    {
        public static string MailtoHintTitle { get; } = "Email sharing notice";
        public static string MailtoHintBody { get; } = "This feature uses your computer’s <strong>default email app</strong> (e.g., Outlook, Mail, Gmail Web).<br><br>If the email window does not open, possible reasons include:<br>• No default email app is set<br>• Browser / corporate security policies restrict external apps<br>• No email application is installed<br><br>Please check your default email settings, or copy the link and paste it into your email.";
        public static string MailtoHintOk { get; } = "Continue";
        public static string MailtoHintCancel { get; } = "Cancel";
        public static string MailtoHintMobileTitle { get; } = "Email sharing tip";
        public static string MailtoHintMobileBody { get; } = "If your email app does not open, please check your default email settings or copy the link and share manually.";
    }
}
