using System.Diagnostics;
using System.Xml.Linq;

namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>以 IIS 的 PID → Application Pool → Website 關聯收集 w3wp 資源用量。</summary>
public sealed class IisMetricsCollector
{
    private readonly Dictionary<int, ProcessCpuSample> previousCpu = [];

    public async Task<IReadOnlyList<IisApplicationPoolMetrics>> CollectAsync(CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows()) return [];
        var appCmd = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "inetsrv", "appcmd.exe");
        if (!File.Exists(appCmd)) return [];

        var applicationXml = await RunAppCmdAsync(appCmd, ["list", "app", "/xml"], cancellationToken);
        var workerXml = await RunAppCmdAsync(appCmd, ["list", "wp", "/xml"], cancellationToken);
        var poolXml = await RunAppCmdAsync(appCmd, ["list", "apppool", "/xml"], cancellationToken);

        var sitesByPool = ParseElements(applicationXml, "APP")
            .Select(x => new { Pool = Attribute(x, "APPPOOL.NAME"), Site = Attribute(x, "SITE.NAME") })
            .Where(x => x.Pool.Length > 0 && x.Site.Length > 0)
            .GroupBy(x => x.Pool, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<string>)x.Select(y => y.Site).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(y => y, StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.OrdinalIgnoreCase);
        var processIdsByPool = ParseElements(workerXml, "WP")
            .Select(x => new { Pool = Attribute(x, "APPPOOL.NAME"), Pid = ParseInt(Attribute(x, "WP.NAME")) })
            .Where(x => x.Pool.Length > 0 && x.Pid > 0)
            .GroupBy(x => x.Pool, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<int>)x.Select(y => y.Pid).Distinct().OrderBy(y => y).ToList(),
                StringComparer.OrdinalIgnoreCase);
        var states = ParseElements(poolXml, "APPPOOL")
            .Select(x => new
            {
                Name = Attribute(x, "APPPOOL.NAME"),
                State = FirstAttribute(x, "state", "STATE", "APPPOOL.STATE")
            })
            .Where(x => x.Name.Length > 0)
            .ToDictionary(x => x.Name, x => x.State, StringComparer.OrdinalIgnoreCase);

        var poolNames = sitesByPool.Keys.Union(processIdsByPool.Keys, StringComparer.OrdinalIgnoreCase)
            .Union(states.Keys, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var now = DateTime.UtcNow;
        var currentPids = new HashSet<int>();
        var output = new List<IisApplicationPoolMetrics>(poolNames.Count);
        foreach (var poolName in poolNames)
        {
            sitesByPool.TryGetValue(poolName, out var siteNames);
            processIdsByPool.TryGetValue(poolName, out var processIds);
            states.TryGetValue(poolName, out var state);
            processIds ??= [];
            double cpu = 0;
            var hasCpu = false;
            long workingSet = 0;
            long privateMemory = 0;
            foreach (var processId in processIds)
            {
                currentPids.Add(processId);
                try
                {
                    using var process = Process.GetProcessById(processId);
                    process.Refresh();
                    workingSet += Math.Max(0, process.WorkingSet64);
                    privateMemory += Math.Max(0, process.PrivateMemorySize64);
                    var processorTime = process.TotalProcessorTime;
                    if (previousCpu.TryGetValue(processId, out var previous))
                    {
                        var elapsedMs = (now - previous.SampledAtUtc).TotalMilliseconds;
                        var processorMs = (processorTime - previous.ProcessorTime).TotalMilliseconds;
                        if (elapsedMs > 0 && processorMs >= 0)
                        {
                            cpu += processorMs * 100d / elapsedMs / Math.Max(1, Environment.ProcessorCount);
                            hasCpu = true;
                        }
                    }
                    previousCpu[processId] = new ProcessCpuSample(now, processorTime);
                }
                catch (ArgumentException) { }
                catch (InvalidOperationException) { }
                catch (System.ComponentModel.Win32Exception) { }
            }

            output.Add(new IisApplicationPoolMetrics(
                poolName,
                siteNames ?? [],
                string.IsNullOrWhiteSpace(state) ? (processIds.Count > 0 ? "Started" : "Unknown") : state,
                processIds,
                hasCpu ? Math.Round(Math.Clamp(cpu, 0, 100), 1) : null,
                workingSet,
                privateMemory));
        }

        foreach (var stalePid in previousCpu.Keys.Where(x => !currentPids.Contains(x)).ToList())
            previousCpu.Remove(stalePid);
        return output;
    }

    private static IEnumerable<XElement> ParseElements(string xml, string elementName)
    {
        if (string.IsNullOrWhiteSpace(xml)) return [];
        try
        {
            return XDocument.Parse(xml).Descendants()
                .Where(x => string.Equals(x.Name.LocalName, elementName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static string Attribute(XElement element, string name) =>
        element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

    private static string FirstAttribute(XElement element, params string[] names) =>
        names.Select(name => Attribute(element, name)).FirstOrDefault(x => x.Length > 0) ?? string.Empty;

    private static int ParseInt(string value) => int.TryParse(value, out var result) ? result : 0;

    private static async Task<string> RunAppCmdAsync(
        string appCmd,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(appCmd)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments) startInfo.ArgumentList.Add(argument);
        using var process = new Process { StartInfo = startInfo };
        if (!process.Start()) return string.Empty;
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            await stderr;
            return process.ExitCode == 0 ? await stdout : string.Empty;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            return string.Empty;
        }
    }

    private sealed record ProcessCpuSample(DateTime SampledAtUtc, TimeSpan ProcessorTime);
}
