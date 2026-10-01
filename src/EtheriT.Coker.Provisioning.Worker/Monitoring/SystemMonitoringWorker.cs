namespace EtheriT.Coker.Provisioning.Worker;

public sealed class SystemMonitoringWorker(
    ILogger<SystemMonitoringWorker> logger,
    ProvisioningWorkerOptions options,
    PlatformProvisioningClient client,
    WindowsSystemMetricsCollector metricsCollector,
    IisMetricsCollector iisMetricsCollector,
    TlsCertificateCollector tlsCollector) : BackgroundService
{
    private static readonly string AgentVersion =
        typeof(SystemMonitoringWorker).Assembly.GetName().Version?.ToString() ?? "unknown";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<IisApplicationPoolMetrics> applicationPools = [];
                try
                {
                    applicationPools = await iisMetricsCollector.CollectAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Unable to collect IIS metrics for {ServerId}", options.ServerId);
                }
                var tlsSnapshot = tlsCollector.CurrentSnapshot;
                var metrics = metricsCollector.Collect(applicationPools) with { TlsSnapshot = tlsSnapshot };
                await client.SendHeartbeatAsync(metrics, AgentVersion, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to report provisioning agent metrics for {ServerId}", options.ServerId);
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, options.MonitoringSeconds)), stoppingToken);
        }
    }
}
