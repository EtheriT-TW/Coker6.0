namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>Run diagnostic step 1 once on startup, then on the daily sampling schedule.</summary>
public sealed class TlsMonitoringWorker(
    ILogger<TlsMonitoringWorker> logger,
    ProvisioningWorkerOptions options,
    TlsCertificateCollector collector) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.TlsSamplingHour is < 0 or > 23)
        {
            logger.LogError("TlsSamplingHour must be between 0 and 23.");
            return;
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await collector.CollectAsync(stoppingToken);
                var now = DateTimeOffset.Now;
                var nextLocal = now.LocalDateTime.Date.AddHours(options.TlsSamplingHour);
                if (nextLocal <= now.LocalDateTime) nextLocal = nextLocal.AddDays(1);
                await Task.Delay(new DateTimeOffset(nextLocal) - now, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
