using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class BackgroundTaskRecord : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_UserId { get; set; }
        public BackgroundTaskTypeEnum Type { get; set; }
        public BackgroundTaskStatusEnum Status { get; set; }
        public int Progress { get; set; }
        public Guid StorageKey { get; set; } = Guid.NewGuid();

        [StringLength(250)]
        public string? ActiveKey { get; set; }

        [StringLength(100)]
        public string? HangfireJobId { get; set; }

        [StringLength(500)]
        public string Message { get; set; } = "";

        [StringLength(1000)]
        public string? SourceFilePath { get; set; }

        [StringLength(1000)]
        public string? ResultFilePath { get; set; }

        [StringLength(255)]
        public string? ResultFileName { get; set; }

        public string? ResultJson { get; set; }

        [StringLength(4000)]
        public string? Error { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? ExpireTime { get; set; }
    }

    public class UserNotification : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_UserId { get; set; }
        public long? FK_BackgroundTaskId { get; set; }
        public NotificationTypeEnum Type { get; set; }

        [StringLength(250)]
        public string Title { get; set; } = "";

        [StringLength(1000)]
        public string Message { get; set; } = "";

        [StringLength(1000)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; }
    }
}
