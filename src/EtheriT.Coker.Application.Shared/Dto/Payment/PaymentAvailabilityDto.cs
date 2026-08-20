namespace EtheriT.Coker.Application.Shared.Dto.Payment
{
    public class PaymentAvailabilityQueryDto
    {
        public long LogisticsSettingId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentAvailabilityItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public long ThirdPartyId { get; set; }
        public string ProviderCode { get; set; } = string.Empty;
        public string RenderMode { get; set; } = string.Empty;
        public decimal MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool IsAvailable { get; set; }
        public string UnavailableReasonCode { get; set; } = string.Empty;
        public string UnavailableReason { get; set; } = string.Empty;
    }
}
