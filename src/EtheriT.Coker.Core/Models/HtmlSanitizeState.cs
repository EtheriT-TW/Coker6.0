using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;

namespace EtheriT.Coker.Core.Models
{
    public class HtmlSanitizeState : AuditedEntity
    {
        public long FK_WebsiteId { get; set; }

        public HtmlSanitizeSourceType SourceType { get; set; }

        public long FK_Bid { get; set; }

        [StringLength(50)]
        public string ContentKey { get; set; } = "Default";

        [StringLength(50)]
        public string SanitizePolicy { get; set; } = "PublicHtml";

        [StringLength(30)]
        public string SanitizeVersion { get; set; } = "";

        [StringLength(64)]
        public string ContentHash { get; set; } = "";
        public Website? Website { get; set; }
    }
}
