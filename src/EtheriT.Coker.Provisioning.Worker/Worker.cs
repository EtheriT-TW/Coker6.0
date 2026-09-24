namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>只負責任務輪詢與生命週期；各設備操作由 IProvisioningTaskHandler 實作。</summary>
public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly ProvisioningWorkerOptions options;
    private readonly PlatformProvisioningClient client;
    private readonly IReadOnlyList<IProvisioningTaskHandler> handlers;

    public Worker(
        ILogger<Worker> logger,
        ProvisioningWorkerOptions options,
        PlatformProvisioningClient client,
        IEnumerable<IProvisioningTaskHandler> handlers)
    {
        this.logger = logger;
        this.options = options;
        this.client = client;
        this.handlers = handlers.ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client.ValidateConfiguration();
        logger.LogInformation(
            "Provisioning Worker {WorkerId} started for server {ServerId}; DryRun={DryRun}",
            client.WorkerId,
            options.ServerId,
            options.DryRun);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var task = await client.ClaimAsync(stoppingToken);
                if (task is not null)
                    await ExecuteAndCompleteAsync(task, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Provisioning polling failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(2, options.PollingSeconds)), stoppingToken);
        }
    }

    private async Task ExecuteAndCompleteAsync(ProvisioningTaskDto task, CancellationToken stoppingToken)
    {
        bool succeeded;
        string message;
        try
        {
            message = options.DryRun
                ? $"Dry Run：已收到 {task.Type} 任務，未執行任何主機命令。參數：{DescribePayload(task.Payload)}"
                : await ResolveHandler(task.Type).ExecuteAsync(task, stoppingToken);
            succeeded = true;
        }
        catch (Exception ex)
        {
            succeeded = false;
            message = ex.Message;
            logger.LogError(ex, "Provisioning task {TaskId} failed", task.Id);
        }

        // 重啟本機 IIS 時 Platform 可能暫時無法回應；只重試結果回報，不重跑主機操作。
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 20 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await client.CompleteAsync(task.Id, succeeded, message, stoppingToken);
                logger.LogInformation("Provisioning task {TaskId} completed: {Succeeded}", task.Id, succeeded);
                return;
            }
            catch (Exception ex) when (attempt < 20)
            {
                lastError = ex;
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
        throw new InvalidOperationException($"Task {task.Id} result could not be reported.", lastError);
    }

    private IProvisioningTaskHandler ResolveHandler(ProvisioningTaskType type) =>
        handlers.FirstOrDefault(handler => handler.CanHandle(type)) ??
        throw new InvalidOperationException($"Unsupported task type: {type}");

    private static string DescribePayload(ProvisioningTaskPayload payload) =>
        $"SiteName={payload.SiteName ?? "-"}, StartSite={payload.StartSite?.ToString() ?? "-"}, " +
        $"DNS={payload.RecordName ?? "-"}.{payload.ZoneName ?? "-"} -> {payload.IPv4Address ?? "-"}, " +
        $"SSL Hosts={string.Join(",", payload.HostNames ?? [])}";
}
