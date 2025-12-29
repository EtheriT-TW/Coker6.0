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

        public async Task<List<MailTemplateResultDto>> GetTemplateRenderAsync(MailTemplateTypeEnum templateType, List<MailTemplateInputDto> input)
        {
            string lang = await loginUserData.GetWebsiteLocal();
            string templateFilePath = string.Empty;

            switch (templateType)
            {
                case MailTemplateTypeEnum.紅利異動:
                    templateFilePath = $"Views/MailTemplate/Bonus/TransactionMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.後台會員建置:
                    templateFilePath = $"Views/MailTemplate/ResetPassword/BackendAddFrontUserMailTemplate.{lang}.cshtml";
                    break;
                case MailTemplateTypeEnum.密碼重設通知:
                    templateFilePath = $"Views/MailTemplate/Forget/ForgetPasswordMailTemplate.{lang}.cshtml";
                    break;
                default:
                    break;
            }

            string templateContent = string.Empty;
            if (!File.Exists(templateFilePath))
            {
                throw new FileNotFoundException($"Mail範本檔不存在: {templateFilePath}");
            }

            templateContent = System.IO.File.ReadAllText(templateFilePath);
            // 移除以 "@model" 開頭的所有資料列
            templateContent = string.Join(Environment.NewLine, templateContent.Split(Environment.NewLine).Where(line => !line.TrimStart().StartsWith("@model")));
            if (string.IsNullOrEmpty(templateContent))
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
