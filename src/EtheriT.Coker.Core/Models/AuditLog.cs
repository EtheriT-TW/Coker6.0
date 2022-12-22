using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class AuditLog
    {
        public long Id { get; set; }
        [StringLength(512)] public string? BrowserInfo { get; set; }
        [StringLength(64)] public string? ClientIpAddress { get; set; }
        [StringLength(128)] public string? ClientName { get; set; }
        [StringLength(2000)] public string? CustomData { get; set; }
        [StringLength(2000)] public string? Exception { get; set; }
        public int ExecutionDuration { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int ImpersonatorTenantId { get; set; }
        public long ImpersonatorUserId { get; set; }
        [StringLength(256)] public string? MethodName { get; set; }
        [StringLength(1024)] public string? Parameters { get; set; }
        [StringLength(256)] public string? ServiceName { get; set; }
        public int WebId { get; set; }
        public long UserId { get; set; }
        public string? ReturnValue { get; set; }
    }
}
