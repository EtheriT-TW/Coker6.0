namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>Samples TLS independently of the frequent system heartbeat.</summary>
public sealed class TlsMonitoringWorker(
    ILogger<TlsMonitoringWorker> logger,
    ProvisioningWorkerOptions options,
    PlatformProvisioningClient client,
    TlsCertificateCollector collector) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.TlsSamplingHour is < 0 or > 23)
        {
            logger.LogError("TlsSamplingHour must be between 0 and 23; TLS sampling is disabled.");
            return;
        }

        // Restore the persisted snapshot before deciding whether a scheduled run was missed.
        // A temporary platform outage must not turn into an unnecessary new sample.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                collector.RestoreSnapshot(await client.GetTlsSnapshotAsync(stoppingToken));
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to restore TLS snapshot for {ServerId}; retrying in one minute", options.ServerId);
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            }
        }

        var startup = DateTimeOffset.Now;
        var latestDueLocal = startup.LocalDateTime.Date.AddHours(options.TlsSamplingHour);
        if (latestDueLocal > startup.LocalDateTime) latestDueLocal = latestDueLocal.AddDays(-1);
        var lastCollected = collector.CurrentSnapshot?.CollectedAtUtc;
        var catchUp = !lastCollected.HasValue
            || lastCollected.Value.ToUniversalTime() < new DateTimeOffset(latestDueLocal).UtcDateTime;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            var nextLocal = now.LocalDateTime.Date.AddHours(options.TlsSamplingHour);
            if (nextLocal <= now.LocalDateTime) nextLocal = nextLocal.AddDays(1);
            var next = new DateTimeOffset(nextLocal);
            try
            {
                if (!catchUp) await Task.Delay(next - now, stoppingToken);
                else logger.LogInformation("Catching up missed TLS sampling for {ServerId}", options.ServerId);
                // Even a failed attempt returns to the daily schedule rather than a tight retry loop.
                catchUp = false;
                var snapshot = await collector.CollectAsync(stoppingToken);
                if (!string.IsNullOrWhiteSpace(snapshot.Error))
                    logger.LogWarning("TLS sampling failed for {ServerId}: {Error}", options.ServerId, snapshot.Error);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to sample TLS certificates for {ServerId}", options.ServerId);
            }
        }
    }
}
