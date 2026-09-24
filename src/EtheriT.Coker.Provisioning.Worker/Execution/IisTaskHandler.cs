namespace EtheriT.Coker.Provisioning.Worker;

public sealed class IisTaskHandler(ProcessRunner runner) : IProvisioningTaskHandler
{
    public bool CanHandle(ProvisioningTaskType type) =>
        type is ProvisioningTaskType.RestartIis or ProvisioningTaskType.SetIisSiteState;

    public Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        if (task.Type == ProvisioningTaskType.RestartIis)
        {
            var iisReset = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "System32", "iisreset.exe");
            return runner.RunAsync(iisReset, ["/restart"], null, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(task.Payload.SiteName))
            throw new InvalidOperationException("IIS site name is required.");
        var appCmd = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "inetsrv", "appcmd.exe");
        var action = task.Payload.StartSite == true ? "start" : "stop";
        return runner.RunAsync(
            appCmd,
            [action, "site", $"/site.name:{task.Payload.SiteName}"],
            null,
            cancellationToken);
    }
}
