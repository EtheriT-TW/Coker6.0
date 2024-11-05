using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Common
{
    public class MailAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly StringHandler stringHandler;
        public MailAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            StringHandler stringHandler
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.stringHandler = stringHandler;
        }
        public async Task<SenderDto> getDefaultDto() {
            SenderDto dto = new SenderDto();

            return dto;
        }
        public async Task<ResponseMessageDto> sendMail(SenderDto dto)
        {
            var webSiteName = string.IsNullOrEmpty(dto.Sender.Name) ? await loginUserData.GetWebsiteName() : dto.Sender.Name;

            return await sendMail(dto, webSiteName);
        }
        public async Task<ResponseMessageDto> sendMail(SenderDto dto, long siteId)
        {
            var webSiteName = await loginUserData.GetWebsiteOrgName(siteId);

            return await sendMail(dto, webSiteName);
        }
        public async Task<ResponseMessageDto> sendMail(SenderDto dto, string? webSiteName)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            string webUrl = await loginUserData.GetWebsiteUrl();
            string OrgName = await loginUserData.GetWebsiteOrgName();
            // 建立郵件
            var message = new MimeMessage();
            // 添加寄件者
            message.From.Add(new MailboxAddress(webSiteName, dto.Sender.Email));

            // 添加收件者
            foreach (var item in dto.Recipients)
            {
                message.To.Add(new MailboxAddress(item.Name, item.Email));
            }
            // 副本
            foreach (var item in dto.CC)
            {
                message.Cc.Add(new MailboxAddress(item.Name, item.Email));
            }
            // 密件副本
            foreach (var item in dto.Bcc)
            {
                message.Bcc.Add(new MailboxAddress(item.Name, item.Email));
            }

            // 設定郵件標題
            message.Subject = dto.Subject;

            // 使用 BodyBuilder 建立郵件內容
            var bodyBuilder = new BodyBuilder();

            // 設定文字內容
            if (!string.IsNullOrEmpty(dto.TextBody))
                bodyBuilder.TextBody = dto.TextBody;
            // 設定 HTML 內容
            if (!string.IsNullOrEmpty(dto.Body))
            {
                string body = stringHandler.HtmlDecode(dto.Body);
                body = body.Replace("<body>", "").Replace("</body>", "");
                body = Regex.Replace(body, $@"src=""/upload/(?!{OrgName})", $@"src=""{webUrl}/upload/{OrgName}/", RegexOptions.IgnoreCase);
                body = Regex.Replace(body, $@"href=""/upload/(?!{OrgName})", $@"href=""{webUrl}/upload/{OrgName}/", RegexOptions.IgnoreCase);
                body = Regex.Replace(body, $@"href=""/", $@"href=""{webUrl}/", RegexOptions.IgnoreCase);
                body = Regex.Replace(body, $@"draggable[\s]?=""[\w]*""", "", RegexOptions.IgnoreCase);
                body = Regex.Replace(body, $@"custom_block_template[\s]?=""[\w]*""", "", RegexOptions.IgnoreCase);
                body = Regex.Replace(body, $@"block_id[\s]?=""[\w]*""", "", RegexOptions.IgnoreCase);
                body = Regex.Replace(body, $@"<button id=""iad5"".*button>", "", RegexOptions.IgnoreCase);
                body = body.Replace("<nav", "<div").Replace("</nav", "</div");
                dto.Css = (dto.Css ?? "").Replace("background-image:url('/upload/", $"background-image:url('{webUrl}/upload/{OrgName}/");

                bodyBuilder.HtmlBody = $@"<!DOCTYPE html>
                    <html>
                        <head>
                            <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <meta name=""x-apple-disable-message-reformatting"">
                            <style>{dto.Css}</style>
                        </head>
                        <body>{body}</body>
                    </html>";
            }
            // 設定附件
            foreach (var file in dto.FilePath)
            {
                bodyBuilder.Attachments.Add(file);
            }
            // 設定郵件內容
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    //client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    // 連接 Mail Server (郵件伺服器網址, 連接埠, 是否使用 SSL)
                    client.Connect(dto.SMTP.Url, dto.SMTP.Port, dto.SMTP.UseSSL? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);

                    // 如果需要的話，驗證一下
                    if (dto.SMTP.UserName != null)
                        client.Authenticate(dto.SMTP.UserName, dto.SMTP.Password);

                    // 寄出郵件
                    client.Send(message);

                    // 中斷連線
                    client.Disconnect(true);
                }
                response.Success = true;
            }
            catch (SmtpCommandException ex)
            {
                switch (ex.ErrorCode)
                {
                    case SmtpErrorCode.RecipientNotAccepted:
                        /***
                         * 可能錯誤原因
                         * 收件人地址格式錯誤。
                         * 收件人地址的網域無法解析。
                         * 收件人地址被伺服器封鎖。
                         * **/
                        if (ex.ErrorCode == SmtpErrorCode.RecipientNotAccepted &&
                            ex.Message.Contains("Domain not found", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            response.Message = "網域錯誤：無法找到指定的網域";
                        }
                        else
                        {
                            response.Message = "收件人錯誤，可能是因為信箱網域或格式有問題";
                        }
                        break;
                    case SmtpErrorCode.SenderNotAccepted:
                        /***
                         * 寄件人地址格式錯誤。
                         * 寄件人地址未經授權或被列入黑名單。
                         * ****/
                        response.Message = "寄件人錯誤，可能是因為信箱網域或格式有問題";
                        break;
                    case SmtpErrorCode.MessageNotAccepted:
                        /***
                         * 郵件被判定為垃圾郵件。
                         * 郵件內容格式不正確或包含禁止的附件。
                         * **/
                        response.Message = "信件內容格式錯誤";
                        break;
                    case SmtpErrorCode.UnexpectedStatusCode:
                        /***
                         * 伺服器發送了無效或未識別的回應。
                         * 伺服器狀態異常
                         * **/
                        response.Message = "信件內容格式錯誤";
                        break;
                }
                response.Error = ex.Message;
            }
            catch (AuthenticationException ex)
            {
                /***
                 * 用戶名或密碼錯誤。
                 * 帳戶被停用或受限。
                 * 需要兩步驗證但未啟用。
                 * **/
                response.Message = "認證失敗，請檢查用戶名和密碼。";
                response.Error = ex.Message;
            }
            catch (Exception ex)
            {
                // 其他錯誤原因
                response.Message = "信件發送失敗";
                response.Error = ex.Message;
            }
            return response;
        }
    }
}
