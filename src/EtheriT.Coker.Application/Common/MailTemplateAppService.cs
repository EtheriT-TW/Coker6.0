using DevExpress.Office.Utils;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.MailTemplate;
using EtheriT.Coker.Application.Shared.Processor;
using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Common
{
    public class MailTemplateAppService : IMailTemplateAppService
    {
        private readonly LoginUserData loginUserData;
        private readonly IHtmlProcessor htmlProcessor;
        public MailTemplateAppService(LoginUserData loginUserData, IHtmlProcessor htmlProcessor)
        {
            this.loginUserData = loginUserData;
            this.htmlProcessor = htmlProcessor;
        }

        public async Task<List<MailTemplateResultDto>> GetTemplateRenderAsync(
            MailTemplateTypeEnum templateType,
            List<MailTemplateInputDto> input,
            string? language = null)
        {
            string lang = string.IsNullOrWhiteSpace(language)
                ? await loginUserData.GetWebsiteLocal()
                : language;
            lang = lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "zh-tw";
            string templateFilePath = string.Empty;
            string? embeddedResourceName = null;

            switch (templateType)
            {
                case MailTemplateTypeEnum.紅利異動:
                    templateFilePath = $"Views/MailTemplate/Bonus/TransactionMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.後台會員建置:
                    templateFilePath = $"Views/MailTemplate/ResetPassword/BackendAddFrontUserMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.密碼重設通知:
                    embeddedResourceName = $"EtheriT.Coker.Application.MailTemplates.Member.ForgetPasswordMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.變更電子信箱:
                    templateFilePath = $"Views/MailTemplate/Member/ChangeEmailMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.註冊驗證通知:
                    templateFilePath = $"Views/MailTemplate/Member/AccountActivationMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.註冊完成通知:
                    templateFilePath = $"Views/MailTemplate/Member/AccountCreatedNoticeMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.後台密碼重設通知:
                    templateFilePath = $"Views/MailTemplate/ResetPassword/BackstageForgetPasswordMailTemplate.{lang}.cshtml";
                    break;
                default:
                    break;
            }

            string templateContent;
            if (!string.IsNullOrEmpty(embeddedResourceName))
            {
                await using var stream = typeof(MailTemplateAppService).Assembly
                    .GetManifestResourceStream(embeddedResourceName);
                if (stream == null)
                    throw new FileNotFoundException($"內嵌 Mail 範本不存在: {embeddedResourceName}");
                using var reader = new StreamReader(stream);
                templateContent = await reader.ReadToEndAsync();
            }
            else
            {
                if (!File.Exists(templateFilePath))
                    throw new FileNotFoundException($"Mail範本檔不存在: {templateFilePath}");
                templateContent = await File.ReadAllTextAsync(templateFilePath);
            }
            // 範本可能來自 Windows 檔案或內嵌資源，須同時支援 CRLF、LF、CR。
            // 若僅以 Environment.NewLine 拆分，LF 範本會整份被視為一行，並因開頭
            // 是 @model 而遭全部移除。
            var templateLines = templateContent.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None);
            templateContent = string.Join(
                Environment.NewLine,
                templateLines.Where(line =>
                    !line.TrimStart('\uFEFF').TrimStart().StartsWith(
                        "@model",
                        StringComparison.Ordinal)));
            if (string.IsNullOrWhiteSpace(templateContent))
            {
                throw new ArgumentException("範本內容不得為空");
            }

            //RazorEngine的範本只需載入一次即可，如每次都重載，會影響效能，大約差10倍以上
            RazorEngine razorEngine = new RazorEngine();
            IRazorEngineCompiledTemplate template = razorEngine.Compile(templateContent, builder =>
            {
                builder.AddAssemblyReferenceByName("System.Text.RegularExpressions");
            });

            List<MailTemplateResultDto> result = new List<MailTemplateResultDto>();
            foreach (var item in input)
            {
                string mailContent = template.Run(item.Model);
                string mailStyles = htmlProcessor.ExtractStyleCss(mailContent);
                mailContent = htmlProcessor.RemoveNode(mailContent, "style");
                result.Add(new MailTemplateResultDto
                {
                    Key = item.Key,
                    Body = mailContent,
                    Style = mailStyles
                });
            }

            return result;
        }
    }
}
