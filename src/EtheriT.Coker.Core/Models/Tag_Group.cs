using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class Tag_Group : FullAuditedEntity
    {
        [StringLength(100)] public string? Title { get; set; }
        public bool Disp_Opt { get; set; }
        public List<Tag_TagGroup> Tag_TagGroups { get; set; }
    }
}
