using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CheckoutBenefitDto
    {
        public CheckoutBenefitTypeEnum BenefitType { get; set; }

        public long? CampaignId { get; set; }

        public long? RuleId { get; set; }

        public string Name { get; set; } = "";

        public string DisplayText { get; set; } = "";
    }
}
