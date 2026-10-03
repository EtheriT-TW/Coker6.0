using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Account_Log
    {
        public virtual long Id { get; set; }
        public Guid UUID { get; set; }
        public long WebsiteId { get; set; }
        public int Status { get; set; } 
        public int ErrorTimes { get; set; }
        public DateTime? LockTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public virtual long CreatorUserId { get; set; }
        public virtual DateTime CreationTime { get; set; } = DateTime.Now;
        public Website? Website { get; set; }
        [MaxLength(80)] public string? EventName { get; set; }
        [MaxLength(80)] public string? VerificationMethod { get; set; }
        public bool? Success { get; set; }
        [MaxLength(100)] public string? FailureReason { get; set; }
        [MaxLength(150)] public string? OldEmail { get; set; }
        [MaxLength(150)] public string? NewEmail { get; set; }
        [MaxLength(150)] public string? RecipientEmail { get; set; }
        [MaxLength(64)] public string? KeyFingerprint { get; set; }
        [MaxLength(64)] public string? ClientIpAddress { get; set; }
        [MaxLength(512)] public string? BrowserInfo { get; set; }
        [MaxLength(128)] public string? CorrelationId { get; set; }
        public int? PreviousStatus { get; set; }
        public int? CurrentStatus { get; set; }
        public string? DetailsJson { get; set; }
    }
}
