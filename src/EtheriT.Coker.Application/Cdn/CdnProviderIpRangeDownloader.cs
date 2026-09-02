using EtheriT.Coker.Application.Shared.Cdn;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Cdn
{
    public sealed record CdnDownloadedIpRange(string Cidr, byte IpVersion);

    public sealed class CdnIpSourceDownloadException : Exception
    {
        public CdnIpSourceDownloadException(Uri sourceUrl, int attempts, Exception innerException)
            : base($"下載 CDN IP 清單失敗：{sourceUrl}，已嘗試 {attempts} 次。", innerException)
        {
            SourceUrl = sourceUrl;
            Attempts = attempts;
        }

        public Uri SourceUrl { get; }
        public int Attempts { get; }
    }

    public sealed class CdnProviderIpRangeDownloader
    {
        public const string HttpClientName = "CdnProviderIpRanges";
        private const int MaximumAttempts = 3;

        private static readonly Regex AzureDownloadUrlRegex = new(
            @"https://download\.microsoft\.com/[^\""'<>\s]*ServiceTags_Public_[^\""'<>\s]*\.json",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly IHttpClientFactory httpClientFactory;

        public CdnProviderIpRangeDownloader(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyCollection<CdnDownloadedIpRange>> DownloadAsync(
            CdnProviderDefinition provider,
            CancellationToken cancellationToken = default)
        {
            if (!provider.SupportsAutomaticSync)
                throw new InvalidOperationException(
                    provider.AutomaticSyncUnavailableReason
                    ?? $"{provider.Key} 未設定可自動同步的 IP 來源。");

            var ranges = new Dictionary<string, CdnDownloadedIpRange>(StringComparer.OrdinalIgnoreCase);
            foreach (var source in provider.IpSources)
            {
                var sourceRanges = await DownloadSourceWithRetryAsync(source, cancellationToken);
                foreach (var range in sourceRanges)
                    ranges[range.Cidr] = range;
            }

            if (ranges.Count < provider.MinimumRangeCount)
            {
                throw new InvalidDataException(
                    $"{provider.Key} IP 清單只有 {ranges.Count} 筆，低於安全門檻 {provider.MinimumRangeCount} 筆，取消覆寫現有資料。");
            }

            return ranges.Values
                .OrderBy(x => x.IpVersion)
                .ThenBy(x => x.Cidr, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private async Task<IReadOnlyCollection<CdnDownloadedIpRange>> DownloadSourceWithRetryAsync(
            CdnIpSourceDefinition source,
            CancellationToken cancellationToken)
        {
            Exception? lastException = null;
            for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
            {
                try
                {
                    return await DownloadSourceOnceAsync(source, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt < MaximumAttempts)
                        await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
                }
            }

            throw new CdnIpSourceDownloadException(source.Url, MaximumAttempts, lastException!);
        }

        private async Task<IReadOnlyCollection<CdnDownloadedIpRange>> DownloadSourceOnceAsync(
            CdnIpSourceDefinition source,
            CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            var content = await client.GetStringAsync(source.Url, cancellationToken);

            return source.Format switch
            {
                CdnIpSourceFormat.PlainText => ParsePlainText(content),
                CdnIpSourceFormat.AwsIpRanges => ParseAwsIpRanges(content, source.Filter),
                CdnIpSourceFormat.AzureServiceTagsDownloadPage =>
                    await DownloadAndParseAzureServiceTagsAsync(client, content, source.Filter, cancellationToken),
                CdnIpSourceFormat.FastlyPublicIpList => ParseFastlyPublicIpList(content),
                _ => throw new NotSupportedException($"不支援 CDN IP 清單格式：{source.Format}")
            };
        }

        private static IReadOnlyCollection<CdnDownloadedIpRange> ParsePlainText(string content)
        {
            return NormalizeRanges(content.Split(
                new[] { '\r', '\n', ' ', '\t', ',' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        private static IReadOnlyCollection<CdnDownloadedIpRange> ParseAwsIpRanges(
            string content,
            string? service)
        {
            using var document = JsonDocument.Parse(content);
            var ranges = new List<string>();

            AddAwsRanges(document.RootElement, "prefixes", "ip_prefix", service, ranges);
            AddAwsRanges(document.RootElement, "ipv6_prefixes", "ipv6_prefix", service, ranges);

            return NormalizeRanges(ranges);
        }

        private static void AddAwsRanges(
            JsonElement root,
            string collectionName,
            string prefixName,
            string? service,
            ICollection<string> ranges)
        {
            if (!root.TryGetProperty(collectionName, out var collection))
                return;

            foreach (var item in collection.EnumerateArray())
            {
                if (!item.TryGetProperty("service", out var serviceElement)
                    || !string.Equals(serviceElement.GetString(), service, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (item.TryGetProperty("region", out var regionElement)
                    && !string.Equals(regionElement.GetString(), "GLOBAL", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (item.TryGetProperty(prefixName, out var prefixElement)
                    && !string.IsNullOrWhiteSpace(prefixElement.GetString()))
                {
                    ranges.Add(prefixElement.GetString()!);
                }
            }
        }

        private static async Task<IReadOnlyCollection<CdnDownloadedIpRange>> DownloadAndParseAzureServiceTagsAsync(
            HttpClient client,
            string downloadPage,
            string? serviceTag,
            CancellationToken cancellationToken)
        {
            var decodedPage = WebUtility.HtmlDecode(downloadPage);
            var match = AzureDownloadUrlRegex.Match(decodedPage);
            if (!match.Success || !Uri.TryCreate(match.Value, UriKind.Absolute, out var downloadUrl))
                throw new InvalidDataException("找不到 Microsoft Azure Service Tags JSON 下載網址。");

            if (!downloadUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                || !downloadUrl.Host.Equals("download.microsoft.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Microsoft Azure Service Tags 下載網址不在允許的主機。");
            }

            var json = await client.GetStringAsync(downloadUrl, cancellationToken);
            using var document = JsonDocument.Parse(json);
            var ranges = new List<string>();

            if (document.RootElement.TryGetProperty("values", out var values))
            {
                foreach (var value in values.EnumerateArray())
                {
                    if (!value.TryGetProperty("name", out var name)
                        || !string.Equals(name.GetString(), serviceTag, StringComparison.OrdinalIgnoreCase)
                        || !value.TryGetProperty("properties", out var properties)
                        || !properties.TryGetProperty("addressPrefixes", out var prefixes))
                    {
                        continue;
                    }

                    foreach (var prefix in prefixes.EnumerateArray())
                    {
                        if (!string.IsNullOrWhiteSpace(prefix.GetString()))
                            ranges.Add(prefix.GetString()!);
                    }
                }
            }

            return NormalizeRanges(ranges);
        }

        private static IReadOnlyCollection<CdnDownloadedIpRange> ParseFastlyPublicIpList(string content)
        {
            using var document = JsonDocument.Parse(content);
            var ranges = new List<string>();
            AddStringArray(document.RootElement, "addresses", ranges);
            AddStringArray(document.RootElement, "ipv6_addresses", ranges);
            return NormalizeRanges(ranges);
        }

        private static void AddStringArray(JsonElement root, string propertyName, ICollection<string> values)
        {
            if (!root.TryGetProperty(propertyName, out var collection))
                return;

            foreach (var item in collection.EnumerateArray())
            {
                if (!string.IsNullOrWhiteSpace(item.GetString()))
                    values.Add(item.GetString()!);
            }
        }

        private static IReadOnlyCollection<CdnDownloadedIpRange> NormalizeRanges(IEnumerable<string> values)
        {
            var ranges = new Dictionary<string, CdnDownloadedIpRange>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in values)
            {
                if (!TryNormalizeCidr(value, out var normalized))
                    throw new InvalidDataException($"CDN IP 清單包含無效 CIDR：{value}");

                ranges[normalized.Cidr] = normalized;
            }

            return ranges.Values.ToArray();
        }

        private static bool TryNormalizeCidr(string value, out CdnDownloadedIpRange range)
        {
            range = default!;
            var parts = value.Trim().Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2
                || !IPAddress.TryParse(parts[0], out var address)
                || !int.TryParse(parts[1], out var prefixLength))
            {
                return false;
            }

            byte ipVersion;
            int maximumPrefixLength;
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                ipVersion = 4;
                maximumPrefixLength = 32;
            }
            else if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                ipVersion = 6;
                maximumPrefixLength = 128;
            }
            else
            {
                return false;
            }

            if (prefixLength < 0 || prefixLength > maximumPrefixLength)
                return false;

            range = new CdnDownloadedIpRange($"{address}/{prefixLength}", ipVersion);
            return true;
        }
    }
}
