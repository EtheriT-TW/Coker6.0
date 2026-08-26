namespace EtheriT.Coker.Application.Shared.Currency
{
    public static class CurrencyCatalog
    {
        public static CurrencyDefinition Default { get; } =
            new("TWD", "新臺幣", "NT$", 0);

        private static readonly Dictionary<string, CurrencyDefinition> Definitions =
            new Dictionary<string, CurrencyDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["TWD"] = Default,
                ["USD"] = new("USD", "美元", "US$", 2)
            };

        public static IReadOnlyCollection<CurrencyDefinition> All => Definitions.Values;

        public static CurrencyDefinition? Find(string? code)
        {
            var normalizedCode = code?.Trim();
            return normalizedCode != null && Definitions.TryGetValue(normalizedCode, out var definition)
                ? definition
                : null;
        }

        public static CurrencyDefinition Resolve(string? code)
        {
            return Find(code) ?? Default;
        }
    }
}
