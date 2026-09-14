namespace EtheriT.Coker.Application.Configuration
{
    public sealed class ECPayPaymentReconciliationOptions
    {
        public bool Enabled { get; set; } = true;
        public int IntervalMinutes { get; set; } = 5;
        public int BatchSize { get; set; } = 50;
        public int LookbackHours { get; set; } = 24;
        public int MinimumAgeMinutes { get; set; } = 2;
        public int RequestTimeoutSeconds { get; set; } = 20;
        public string? FrontBaseUrlOverride { get; set; }
        public long? TargetWebsiteId { get; set; }
    }
}
