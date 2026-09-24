namespace EtheriT.Coker.Provisioning.Worker;

public sealed class SslTaskHandler(
    ProcessRunner runner,
    ProvisioningWorkerOptions options) : IProvisioningTaskHandler
{
    public bool CanHandle(ProvisioningTaskType type) => type == ProvisioningTaskType.InstallSsl;

    public Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        if (!options.SslEnabled) throw new InvalidOperationException("SSL operations are disabled on this worker.");
        var hosts = (task.Payload.HostNames ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (hosts.Count == 0) throw new InvalidOperationException("At least one SSL host name is required.");
        if (string.IsNullOrWhiteSpace(options.AcmeEmail))
            throw new InvalidOperationException("ProvisioningWorker:AcmeEmail is required for win-acme.");

        return runner.RunAsync(
            options.WacsPath,
            ["--source", "iis", "--host", string.Join(',', hosts),
             "--installation", "iis", "--store", "certificatestore",
             "--emailaddress", options.AcmeEmail, "--accepttos", "--closeonfinish"],
            null,
            cancellationToken);
    }
}
