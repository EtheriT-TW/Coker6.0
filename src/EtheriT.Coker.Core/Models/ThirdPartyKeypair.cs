
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class ThirdPartyKeypair : FullAuditedEntity
    {
        [StringLength(50)] public long FK_TPid { get; set; }
        [StringLength(50)] public string? Title { get; set; }
        [StringLength(50)] public string? Code { get; set; }
        [StringLength(50)] public string? PromptText { get; set; }
        public ThirdPartyKeypairInputTypeEnum InputType { get; set; }
        public ThirdParty? ThirdParty { get; set; }
        public List<ThirdPartyKeypairValue> thirdPartyKeypairValues { get; set; }
    }
}
