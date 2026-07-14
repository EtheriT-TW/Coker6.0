namespace EtheriT.Coker.Application.Common
{
    public sealed class BackgroundOperationContext
    {
        public long? WebsiteId { get; private set; }
        public long? UserId { get; private set; }

        public bool IsActive => WebsiteId.HasValue && UserId.HasValue;

        public void Set(long websiteId, long userId)
        {
            WebsiteId = websiteId;
            UserId = userId;
        }
    }
}
