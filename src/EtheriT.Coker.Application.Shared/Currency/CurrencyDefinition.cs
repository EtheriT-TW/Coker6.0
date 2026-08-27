using System.Globalization;

namespace EtheriT.Coker.Application.Shared.Currency
{
    public sealed class CurrencyDefinition
    {
        public CurrencyDefinition(string code, string name, string symbol, int decimalDigits)
        {
            Code = code;
            Name = name;
            Symbol = symbol;
            DecimalDigits = decimalDigits;
        }

        public string Code { get; }
        public string Name { get; }
        public string Symbol { get; }
        public int DecimalDigits { get; }

        public string Format(decimal amount)
        {
            return $"{Symbol}{amount.ToString($"N{DecimalDigits}", CultureInfo.InvariantCulture)}";
        }
    }
}
