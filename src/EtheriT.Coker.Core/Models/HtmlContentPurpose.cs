using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class HtmlContentPurpose : FullAuditedEntity
    {
        public long FK_HtmlContentId { get; set; }
        public long FK_ComponentPurposeId { get; set; }

        public Html_Content HtmlContent { get; set; } = null!;
        public ComponentPurpose ComponentPurpose { get; set; } = null!;
    }
}
