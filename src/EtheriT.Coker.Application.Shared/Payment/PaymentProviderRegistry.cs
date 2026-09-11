using System.Collections.Generic;

namespace EtheriT.Coker.Application.Shared.Payment
{
    public sealed class PaymentProviderDescriptor
    {
        public string ProviderCode { get; init; } = "Default";
        public string RenderMode { get; init; } = "Standard";
        public string EntryTitle { get; init; } = string.Empty;
        public decimal? DefaultMaxAmount { get; init; }
    }

    /// <summary>
    /// Central payment-provider metadata used by page rendering and availability APIs.
    /// Adding a provider should not require provider-specific checks in a page or host.
    /// </summary>
    public static class PaymentProviderRegistry
    {
        private static readonly IReadOnlyDictionary<long, PaymentProviderDescriptor> Providers =
            new Dictionary<long, PaymentProviderDescriptor>
            {
                [2] = new PaymentProviderDescriptor
                {
                    ProviderCode = "PCHomePay",
                    RenderMode = "Standard"
                },
                [3] = new PaymentProviderDescriptor
                {
                    ProviderCode = "LinePay",
                    RenderMode = "Standard"
                },
                [4] = new PaymentProviderDescriptor
                {
                    ProviderCode = "ECPay",
                    RenderMode = "Embedded",
                    EntryTitle = "其他支付方式",
                    DefaultMaxAmount = 20000
                }
            };

        public static PaymentProviderDescriptor Resolve(long thirdPartyId)
        {
            if (Providers.TryGetValue(thirdPartyId, out var provider))
                return provider;

            return new PaymentProviderDescriptor
            {
                ProviderCode = thirdPartyId > 0
                    ? $"ThirdParty:{thirdPartyId}"
                    : "Default",
                RenderMode = "Standard"
            };
        }

        /// <summary>
        /// 是否為真正的線上金流（支付連／LINE Pay／綠界）。
        /// 轉帳、貨到付款、郵政劃撥雖然也有 ThirdParty 資料列，但不走線上金流。
        /// </summary>
        public static bool IsOnlineGateway(long thirdPartyId)
        {
            return Providers.ContainsKey(thirdPartyId);
        }
    }
}
