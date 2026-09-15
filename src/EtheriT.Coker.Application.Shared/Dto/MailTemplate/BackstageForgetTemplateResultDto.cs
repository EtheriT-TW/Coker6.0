namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class BackstageForgetTemplateResultDto
    {
        public string Name { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string ResetPasswordUrl { get; set; } = string.Empty;
        public DateTime ExpireTime { get; set; }
    }
}
