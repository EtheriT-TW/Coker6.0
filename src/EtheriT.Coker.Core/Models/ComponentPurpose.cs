using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class ComponentPurpose : FullAuditedEntity
    {
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int SerNo { get; set; } = 500;
        public bool Visible { get; set; } = true;

        public List<HtmlContentPurpose> HtmlContentPurposes { get; set; } = new();
    }
}
