using EtheriT.Coker.Application.Shared.Cdn;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace EtheriT.Coker.Application.Cdn
{
    public sealed record CdnClientIpResolution(
        IPAddress ClientIpAddress,
        string Provider,
        IPAddress TrustedProxyIpAddress);

    public sealed class CdnClientIpResolver
    {
        private const string CdnProviderSettingKey = "cdnProvider";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

        private readonly CokerDbContext db;
        private readonly IMemoryCache cache;
        private readonly IConfiguration configuration;
        private readonly ILogger<CdnClientIpResolver> logger;

        public CdnClientIpResolver(
            CokerDbContext db,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<CdnClientIpResolver> logger)
        {
            this.db = db;
            this.cache = cache;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<CdnClientIpResolution?> ResolveAsync(
            HttpContext context,
            CancellationToken cancellationToken = default)
        {
            var proxyIpAddress = NormalizeAddress(context.Connection.RemoteIpAddress);
            if (proxyIpAddress == null)
                return null;

            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            if (websiteId <= 0)
                return null;

            var configurationCache = await GetConfigurationAsync(websiteId, cancellationToken);
            if (configurationCache == null
                || !configurationCache.Ranges.Any(range => range.Contains(proxyIpAddress)))
            {
                return null;
            }

            if (!context.Request.Headers.TryGetValue(
                    configurationCache.Definition.ClientIpHeader,
                    out var headerValues))
            {
                return null;
            }

            var clientIpAddress = SelectClientIpAddress(
                headerValues.ToString(),
                configurationCache.Definition.ClientIpHeaderSelection);
            if (clientIpAddress == null)
                return null;

            return new CdnClientIpResolution(
                clientIpAddress,
                configurationCache.Definition.Key,
                proxyIpAddress);
        }

        private async Task<CachedCdnConfiguration?> GetConfigurationAsync(
            long websiteId,
            CancellationToken cancellationToken)
        {
            return await cache.GetOrCreateAsync(
                $"CdnClientIpResolver:{websiteId}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                    var provider = await (
                            from detail in db.StoreSetDetail
                            join setting in db.StoreSet on detail.FK_StoreSetId equals setting.Id
                            where detail.FK_WebsiteId == websiteId
                                  && !detail.IsDeleted
                                  && !setting.IsDeleted
                                  && setting.key == CdnProviderSettingKey
                            orderby detail.CreationTime descending, detail.Id descending
                            select detail.value)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (string.IsNullOrWhiteSpace(provider)
                        || provider.Equals(CdnProviderKeys.None, StringComparison.OrdinalIgnoreCase)
                        || !CdnProviderDefinitions.TryGet(provider, out var definition))
                    {
                        return null;
                    }

                    var cidrs = await db.CdnProviderIpRanges
                        .Where(range => !range.IsDeleted
                                        && range.Provider == definition.Key)
                        .Select(range => range.Cidr)
                        .ToListAsync(cancellationToken);

                    var ranges = cidrs
                        .Select(CidrRange.TryParse)
                        .Where(range => range != null)
                        .Cast<CidrRange>()
                        .ToArray();

                    if (ranges.Length == 0)
                    {
                        logger.LogWarning(
                            "CDN is enabled but no trusted proxy IP range is available. WebsiteId={WebsiteId}, Provider={Provider}",
                            websiteId,
                            definition.Key);
                        return null;
                    }

                    return new CachedCdnConfiguration(definition, ranges);
                });
        }

        private static IPAddress? SelectClientIpAddress(
            string headerValue,
            CdnClientIpHeaderSelection selection)
        {
            var values = headerValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var candidates = selection == CdnClientIpHeaderSelection.Last
                ? values.Reverse()
                : values;

            foreach (var value in candidates)
            {
                var candidate = value.Trim().Trim('"');
                if (candidate.StartsWith('[')
                    && candidate.EndsWith(']'))
                {
                    candidate = candidate[1..^1];
                }

                if (IPAddress.TryParse(candidate, out var address))
                    return NormalizeAddress(address);
            }

            return null;
        }

        private static IPAddress? NormalizeAddress(IPAddress? address)
        {
            return address?.IsIPv4MappedToIPv6 == true
                ? address.MapToIPv4()
                : address;
        }

        private sealed record CachedCdnConfiguration(
            CdnProviderDefinition Definition,
            IReadOnlyCollection<CidrRange> Ranges);

        private sealed record CidrRange(
            byte[] NetworkBytes,
            int PrefixLength,
            AddressFamily AddressFamily)
        {
            public static CidrRange? TryParse(string value)
            {
                var parts = value.Split('/', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2
                    || !IPAddress.TryParse(parts[0], out var networkAddress)
                    || !int.TryParse(parts[1], out var prefixLength))
                {
                    return null;
                }

                networkAddress = NormalizeAddress(networkAddress)!;
                var maximumPrefixLength = networkAddress.AddressFamily == AddressFamily.InterNetwork
                    ? 32
                    : networkAddress.AddressFamily == AddressFamily.InterNetworkV6
                        ? 128
                        : 0;

                if (prefixLength < 0 || prefixLength > maximumPrefixLength)
                    return null;

                return new CidrRange(
                    networkAddress.GetAddressBytes(),
                    prefixLength,
                    networkAddress.AddressFamily);
            }

            public bool Contains(IPAddress address)
            {
                address = NormalizeAddress(address)!;
                if (address.AddressFamily != AddressFamily)
                    return false;

                var addressBytes = address.GetAddressBytes();
                var completeBytes = PrefixLength / 8;
                var remainingBits = PrefixLength % 8;

                for (var index = 0; index < completeBytes; index++)
                {
                    if (addressBytes[index] != NetworkBytes[index])
                        return false;
                }

                if (remainingBits == 0)
                    return true;

                var mask = (byte)(0xFF << (8 - remainingBits));
                return (addressBytes[completeBytes] & mask)
                    == (NetworkBytes[completeBytes] & mask);
            }
        }
    }
}
