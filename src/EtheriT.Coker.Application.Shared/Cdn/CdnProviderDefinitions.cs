namespace EtheriT.Coker.Application.Shared.Cdn
{
    public static class CdnProviderKeys
    {
        public const string None = "None";
        public const string Cloudflare = "Cloudflare";
        public const string CloudFront = "CloudFront";
        public const string AzureFrontDoor = "AzureFrontDoor";
        public const string GoogleCloudCdn = "GoogleCloudCdn";
        public const string Fastly = "Fastly";
    }

    public enum CdnIpSourceFormat
    {
        PlainText,
        AwsIpRanges,
        AzureServiceTagsDownloadPage,
        FastlyPublicIpList
    }

    public enum CdnClientIpHeaderSelection
    {
        First,
        Last
    }

    public sealed record CdnIpSourceDefinition(
        Uri Url,
        CdnIpSourceFormat Format,
        string? Filter = null);

    public sealed record CdnProviderDefinition(
        string Key,
        string ClientIpHeader,
        IReadOnlyList<CdnIpSourceDefinition> IpSources,
        int MinimumRangeCount,
        CdnClientIpHeaderSelection ClientIpHeaderSelection = CdnClientIpHeaderSelection.First,
        string? AutomaticSyncUnavailableReason = null)
    {
        public bool SupportsAutomaticSync => IpSources.Count > 0;
    }

    public static class CdnProviderDefinitions
    {
        private static readonly IReadOnlyDictionary<string, CdnProviderDefinition> Definitions =
            new Dictionary<string, CdnProviderDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                [CdnProviderKeys.Cloudflare] = new(
                    CdnProviderKeys.Cloudflare,
                    "CF-Connecting-IP",
                    new[]
                    {
                        new CdnIpSourceDefinition(
                            new Uri("https://www.cloudflare.com/ips-v4"),
                            CdnIpSourceFormat.PlainText),
                        new CdnIpSourceDefinition(
                            new Uri("https://www.cloudflare.com/ips-v6"),
                            CdnIpSourceFormat.PlainText)
                    },
                    MinimumRangeCount: 10),

                [CdnProviderKeys.CloudFront] = new(
                    CdnProviderKeys.CloudFront,
                    "X-Forwarded-For",
                    new[]
                    {
                        new CdnIpSourceDefinition(
                            new Uri("https://ip-ranges.amazonaws.com/ip-ranges.json"),
                            CdnIpSourceFormat.AwsIpRanges,
                            "CLOUDFRONT")
                    },
                    MinimumRangeCount: 10,
                    ClientIpHeaderSelection: CdnClientIpHeaderSelection.Last),

                [CdnProviderKeys.AzureFrontDoor] = new(
                    CdnProviderKeys.AzureFrontDoor,
                    "X-Azure-ClientIP",
                    new[]
                    {
                        new CdnIpSourceDefinition(
                            new Uri("https://www.microsoft.com/en-us/download/confirmation.aspx?id=56519"),
                            CdnIpSourceFormat.AzureServiceTagsDownloadPage,
                            "AzureFrontDoor.Backend")
                    },
                    MinimumRangeCount: 1),

                [CdnProviderKeys.GoogleCloudCdn] = new(
                    CdnProviderKeys.GoogleCloudCdn,
                    "X-Forwarded-For",
                    Array.Empty<CdnIpSourceDefinition>(),
                    MinimumRangeCount: 0,
                    ClientIpHeaderSelection: CdnClientIpHeaderSelection.Last,
                    AutomaticSyncUnavailableReason:
                        "Google Cloud CDN 的來源 IP 範圍依 Load Balancer 與 Backend 類型而異，必須先提供部署型態。"),

                [CdnProviderKeys.Fastly] = new(
                    CdnProviderKeys.Fastly,
                    "Fastly-Client-IP",
                    new[]
                    {
                        new CdnIpSourceDefinition(
                            new Uri("https://api.fastly.com/public-ip-list"),
                            CdnIpSourceFormat.FastlyPublicIpList)
                    },
                    MinimumRangeCount: 5)
            };

        public static IReadOnlyCollection<CdnProviderDefinition> All => Definitions.Values.ToArray();

        public static bool TryGet(string? key, out CdnProviderDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(key)
                && Definitions.TryGetValue(key.Trim(), out var found))
            {
                definition = found;
                return true;
            }

            definition = default!;
            return false;
        }
    }
}
