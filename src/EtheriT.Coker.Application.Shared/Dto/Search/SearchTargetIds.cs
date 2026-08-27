namespace EtheriT.Coker.Application.Shared.Dto.Search
{
    public static class SearchTargetIds
    {
        public const long Default = 0;
        public const long Article = -1;
        public const long Product = -2;

        // 第一階段保留舊網址 /Search/Get/3/{keyword} 的相容性。
        public const long LegacyProduct = 3;

        public static long Normalize(long searchId)
        {
            return searchId == LegacyProduct ? Product : searchId;
        }
    }
}
