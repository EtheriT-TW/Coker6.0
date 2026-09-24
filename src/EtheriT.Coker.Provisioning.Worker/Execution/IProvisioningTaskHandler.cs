namespace EtheriT.Coker.Provisioning.Worker;

public interface IProvisioningTaskHandler
{
    bool CanHandle(ProvisioningTaskType type);
    Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken);
}
