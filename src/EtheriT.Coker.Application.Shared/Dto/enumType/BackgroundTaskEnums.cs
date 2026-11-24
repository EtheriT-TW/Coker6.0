namespace EtheriT.Coker.Application.Shared.Dto.enumType
{
    public enum BackgroundTaskTypeEnum
    {
        ProductImport = 1,
        ProductExport = 2,
        FileCleanup = 3,
        SiteCrawl = 4,
        ArticleTemplateApply = 5,
        ArticleImport = 6
    }

    public enum BackgroundTaskStatusEnum
    {
        Queued = 0,
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Expired = 4,
        AwaitingConfirmation = 5,
        CancelRequested = 6,
        Canceled = 7
    }

    public enum NotificationTypeEnum
    {
        BackgroundTask = 1
    }
}
