using System.Diagnostics;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class ProcessRunner(ProvisioningWorkerOptions options)
{
    public async Task<string> RunAsync(
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
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(30, options.OperationTimeoutSeconds)));
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
