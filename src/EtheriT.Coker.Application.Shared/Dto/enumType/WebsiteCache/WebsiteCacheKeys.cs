using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Shared.Dto.enumType
{
    public static class WebsiteCacheKeys
    {
        public const string Menu = "menu";

        public static readonly HashSet<string> FixedKeys = new(StringComparer.Ordinal)
        {
            Menu
        };

        private static readonly Regex CacheKeyRegex =
            new(@"^[a-z0-9]+(?:-[a-z0-9]+)*(?::[a-z0-9]+(?:-[a-z0-9]+)*)*$", RegexOptions.Compiled);

        public static string Normalize(string? cacheKey)
        {
            return (cacheKey ?? string.Empty).Trim().ToLowerInvariant();
        }

        public static bool IsValid(string? cacheKey)
        {
            cacheKey = Normalize(cacheKey);

            if (string.IsNullOrWhiteSpace(cacheKey))
                return false;

            if (cacheKey.Length > 200)
                return false;

            if (!CacheKeyRegex.IsMatch(cacheKey))
                return false;

            return FixedKeys.Contains(cacheKey);
        }
    }
}
