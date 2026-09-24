using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class Worker : BackgroundService
{
    private const string ApiKeyHeader = "X-Provisioning-Key";
    private readonly ILogger<Worker> _logger;
    private readonly ProvisioningWorkerOptions _options;
    private readonly HttpClient _client;
    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}";

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _options = configuration.GetSection("ProvisioningWorker").Get<ProvisioningWorkerOptions>() ?? new();
        _client = new HttpClient { Timeout = TimeSpan.FromSeconds(Math.Max(30, _options.OperationTimeoutSeconds)) };
        if (Uri.TryCreate(_options.PlatformBaseUrl, UriKind.Absolute, out var baseUri))
            _client.BaseAddress = baseUri;
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            _client.DefaultRequestHeaders.Add(ApiKeyHeader, _options.ApiKey);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ValidateConfiguration();
        _logger.LogInformation("Provisioning Worker {WorkerId} started for server {ServerId}", _workerId, _options.ServerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var task = await ClaimAsync(stoppingToken);
                if (task is not null)
                    await ExecuteAndCompleteAsync(task, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Provisioning polling failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(2, _options.PollingSeconds)), stoppingToken);
        }
    }

    private void ValidateConfiguration()
    {
        if (_client.BaseAddress is null) throw new InvalidOperationException("ProvisioningWorker:PlatformBaseUrl is invalid.");
        if (string.IsNullOrWhiteSpace(_options.ServerId)) throw new InvalidOperationException("ProvisioningWorker:ServerId is required.");
        if (string.IsNullOrWhiteSpace(_options.ApiKey)) throw new InvalidOperationException("ProvisioningWorker:ApiKey is required.");
    }

    private async Task<ProvisioningTaskDto?> ClaimAsync(CancellationToken cancellationToken)
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/provisioning/agent/tasks/claim",
            new ClaimRequest(_options.ServerId, _workerId), cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProvisioningTaskDto>(cancellationToken: cancellationToken);
    }

    private async Task ExecuteAndCompleteAsync(ProvisioningTaskDto task, CancellationToken stoppingToken)
    {
        bool succeeded;
        string message;
        try
        {
            message = await ExecuteTaskAsync(task, stoppingToken);
            succeeded = true;
        }
        catch (Exception ex)
        {
            succeeded = false;
            message = ex.Message;
            _logger.LogError(ex, "Provisioning task {TaskId} failed", task.Id);
        }

        // 重啟本機 IIS 時 Platform 可能暫時無法回應，因此完成回報需重試，但不重跑主機操作。
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 20 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                using var response = await _client.PostAsJsonAsync(
                    $"/api/provisioning/agent/tasks/{task.Id}/complete",
                    new CompleteRequest(_options.ServerId, _workerId, succeeded, message), stoppingToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Provisioning task {TaskId} completed: {Succeeded}", task.Id, succeeded);
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

    private Task<string> ExecuteTaskAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        if (_options.DryRun)
            return Task.FromResult($"Dry Run：已收到 {task.Type} 任務，未執行任何主機命令。參數：{DescribePayload(task.Payload)}");

        return task.Type switch
        {
            ProvisioningTaskType.RestartIis => RestartIisAsync(cancellationToken),
            ProvisioningTaskType.SetIisSiteState => SetIisSiteStateAsync(task.Payload, cancellationToken),
            ProvisioningTaskType.CreateDnsARecord => ChangeDnsRecordAsync(task.Payload, delete: false, cancellationToken),
            ProvisioningTaskType.DeleteDnsARecord => ChangeDnsRecordAsync(task.Payload, delete: true, cancellationToken),
            ProvisioningTaskType.InstallSsl => InstallSslAsync(task.Payload, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported task type: {task.Type}")
        };
    }

    private static string DescribePayload(ProvisioningTaskPayload payload) =>
        $"SiteName={payload.SiteName ?? "-"}, StartSite={payload.StartSite?.ToString() ?? "-"}, " +
        $"DNS={payload.RecordName ?? "-"}.{payload.ZoneName ?? "-"} -> {payload.IPv4Address ?? "-"}, " +
        $"SSL Hosts={string.Join(",", payload.HostNames ?? [])}";

    private Task<string> RestartIisAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "iisreset.exe");
        return RunAsync(path, ["/restart"], null, cancellationToken);
    }

    private Task<string> SetIisSiteStateAsync(ProvisioningTaskPayload payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload.SiteName)) throw new InvalidOperationException("IIS site name is required.");
        var appCmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "inetsrv", "appcmd.exe");
        var action = payload.StartSite == true ? "start" : "stop";
        return RunAsync(appCmd, [action, "site", $"/site.name:{payload.SiteName}"], null, cancellationToken);
    }

    private Task<string> ChangeDnsRecordAsync(ProvisioningTaskPayload payload, bool delete, CancellationToken cancellationToken)
    {
        if (!_options.DnsEnabled) throw new InvalidOperationException("DNS operations are disabled on this worker.");
        var values = new[] { payload.ZoneName, payload.RecordName, payload.IPv4Address };
        if (values.Any(string.IsNullOrWhiteSpace)) throw new InvalidOperationException("DNS zone, record and IPv4 address are required.");

        const string createScript = "Import-Module DnsServer -ErrorAction Stop; $r=@(Get-DnsServerResourceRecord -ZoneName $env:COKER_DNS_ZONE -Name $env:COKER_DNS_NAME -RRType A -ErrorAction SilentlyContinue); if($r.Count -gt 0){if($r.RecordData.IPv4Address.IPAddressToString -contains $env:COKER_DNS_IP){Write-Output '指定的 DNS A 記錄已存在'; exit 0}; throw '同名 DNS A 記錄已存在，但 IP 不同'}; Add-DnsServerResourceRecordA -ZoneName $env:COKER_DNS_ZONE -Name $env:COKER_DNS_NAME -IPv4Address $env:COKER_DNS_IP -ErrorAction Stop";
        const string deleteScript = "Import-Module DnsServer -ErrorAction Stop; $r=Get-DnsServerResourceRecord -ZoneName $env:COKER_DNS_ZONE -Name $env:COKER_DNS_NAME -RRType A -ErrorAction Stop | Where-Object {$_.RecordData.IPv4Address.IPAddressToString -eq $env:COKER_DNS_IP}; if($null -eq $r){throw '指定的 DNS A 記錄不存在'}; $r | Remove-DnsServerResourceRecord -ZoneName $env:COKER_DNS_ZONE -Force -ErrorAction Stop";
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(delete ? deleteScript : createScript));
        var environment = new Dictionary<string, string?>
        {
            ["COKER_DNS_ZONE"] = payload.ZoneName,
            ["COKER_DNS_NAME"] = payload.RecordName,
            ["COKER_DNS_IP"] = payload.IPv4Address
        };
        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
        return RunAsync(powershell, ["-NoProfile", "-NonInteractive", "-EncodedCommand", encoded], environment, cancellationToken);
    }

    private Task<string> InstallSslAsync(ProvisioningTaskPayload payload, CancellationToken cancellationToken)
    {
        if (!_options.SslEnabled) throw new InvalidOperationException("SSL operations are disabled on this worker.");
        var hosts = (payload.HostNames ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (hosts.Count == 0)
            throw new InvalidOperationException("At least one SSL host name is required.");
        if (string.IsNullOrWhiteSpace(_options.AcmeEmail))
            throw new InvalidOperationException("ProvisioningWorker:AcmeEmail is required for win-acme.");

        return RunAsync(_options.WacsPath,
            ["--source", "iis", "--host", string.Join(',', hosts),
             "--installation", "iis", "--store", "certificatestore", "--emailaddress", _options.AcmeEmail,
             "--accepttos", "--closeonfinish"], null, cancellationToken);
    }

    private async Task<string> RunAsync(
        string executable,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string?>? environment,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(executable)) throw new FileNotFoundException("Required executable was not found.", executable);
        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
        if (environment is not null)
            foreach (var pair in environment) startInfo.Environment[pair.Key] = pair.Value;

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start()) throw new InvalidOperationException($"Unable to start {Path.GetFileName(executable)}.");
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(30, _options.OperationTimeoutSeconds)));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            throw;
        }

        var stdout = (await stdoutTask).Trim();
        var stderr = (await stderrTask).Trim();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{Path.GetFileName(executable)} exited with code {process.ExitCode}: {stderr}");
        return string.IsNullOrWhiteSpace(stdout) ? "操作完成。" : stdout[..Math.Min(stdout.Length, 3500)];
    }
}
