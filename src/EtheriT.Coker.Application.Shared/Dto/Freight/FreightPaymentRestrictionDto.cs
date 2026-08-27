namespace EtheriT.Coker.Application.Shared.Dto.Freight
{
    public class FreightPaymentRestrictionDto
    {
        public long PaymentTypeId { get; set; }
        public string PaymentTypeTitle { get; set; } = string.Empty;
        public string PaymentTypeCode { get; set; } = string.Empty;
        public bool WebsitePaymentEnabled { get; set; }

        public decimal PaymentTypeMinAmount { get; set; }
        public decimal? PaymentTypeMaxAmount { get; set; }

        public bool DefaultIsEnabled { get; set; }
        public decimal DefaultMinAmount { get; set; }
        public decimal? DefaultMaxAmount { get; set; }

        public bool IsCustomized { get; set; }
        public bool IsEnabled { get; set; }
        public decimal? OverrideMinAmount { get; set; }
        public decimal? OverrideMaxAmount { get; set; }

        public decimal EffectiveMinAmount { get; set; }
        public decimal? EffectiveMaxAmount { get; set; }
    }
}
