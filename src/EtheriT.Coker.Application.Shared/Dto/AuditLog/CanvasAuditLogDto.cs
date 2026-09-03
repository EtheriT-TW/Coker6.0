using System;
using System.Collections.Generic;

namespace EtheriT.Coker.Application.Dto.AuditLog
{
    public enum CanvasAuditLogSource
    {
        Advertise = 1,
        Article = 2,
        ObjectType = 3,
        WebMenu = 4,
        Product = 5,
        TechnicalCertificate = 6,
        TemplateFooter = 7
    }

    public class CanvasAuditLogInputDto
    {
        public long Id { get; set; }
        public CanvasAuditLogSource Source { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CanvasAuditLogItemDto
    {
        public long Id { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
    }

    public class CanvasAuditLogOutputDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CanvasAuditLogItemDto> Items { get; set; } = new();
    }

    public class CanvasAuditLogDetailInputDto
    {
        public long AuditLogId { get; set; }
        public long Id { get; set; }
        public CanvasAuditLogSource Source { get; set; }
    }

    public class CanvasAuditLogDetailOutputDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string Html { get; set; } = string.Empty;
        public string Css { get; set; } = string.Empty;
        public DateTime ExecutionTime { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
    }
}
