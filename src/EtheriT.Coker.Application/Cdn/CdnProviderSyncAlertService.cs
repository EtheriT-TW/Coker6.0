using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.Cdn
{
    public sealed class CdnProviderSyncAlertService
    {
        private readonly CokerDbContext db;
        private readonly MailAppService mailAppService;
        private readonly ILogger<CdnProviderSyncAlertService> logger;

        public CdnProviderSyncAlertService(
            CokerDbContext db,
            MailAppService mailAppService,
            ILogger<CdnProviderSyncAlertService> logger)
        {
            this.db = db;
            this.mailAppService = mailAppService;
            this.logger = logger;
        }

        public async Task<bool> SendAsync(
            string provider,
            int failureCount,
            string errorMessage)
        {
            var recipients = await (
                    from user in db.Users
                    join mapping in db.MappingUserAndRoles on user.Id equals mapping.UserId
                    join role in db.Roles on mapping.RoleId equals role.Id
                    where role.Type == RoleTypeEnum.系統維護
                          && user.Email != null
                          && user.Email != string.Empty
                    select new
                    {
                        user.Name,
                        Email = user.Email!
                    })
                .Distinct()
                .ToListAsync();

            if (recipients.Count == 0)
            {
                logger.LogError(
                    "CDN IP range synchronization alert was not sent because no system administrator email was found. Provider={Provider}",
                    provider);
                return false;
            }

            var dto = new SenderDto
            {
                Subject = $"[系統通知] {provider} CDN IP 清單同步失敗",
                TextBody = $"CDN 服務商：{provider}{Environment.NewLine}"
                    + $"累計失敗次數：{failureCount}{Environment.NewLine}"
                    + $"發生時間（UTC）：{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}"
                    + $"錯誤內容：{errorMessage}"
            };

            dto.Recipients.AddRange(recipients.Select(x => new MailUserDataDto
            {
                Name = x.Name,
                Email = x.Email
            }));

            var result = await mailAppService.sendSystemMail(dto);

            if (!result.Success)
            {
                logger.LogError(
                    "CDN IP range synchronization alert email failed. Provider={Provider}, Message={Message}, Error={Error}",
                    provider,
                    result.Message,
                    result.Error);
            }

            return result.Success;
        }
    }
}
