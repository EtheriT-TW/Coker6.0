using System.Diagnostics;
using System.Xml.Linq;
using System.Text.Json;

namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>以 IIS 的 PID → Application Pool → Website 關聯收集 w3wp 資源用量。</summary>
public sealed class IisMetricsCollector(ILogger<IisMetricsCollector> logger)
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
        var siteXml = await RunAppCmdAsync(appCmd, ["list", "site", "/xml"], cancellationToken);
        var workerXml = await RunAppCmdAsync(appCmd, ["list", "wp", "/xml"], cancellationToken);
        var poolXml = await RunAppCmdAsync(appCmd, ["list", "apppool", "/xml"], cancellationToken);
        var vdirXml = await RunAppCmdAsync(appCmd, ["list", "vdir", "/xml"], cancellationToken);
        var rootsByApplication = ParseElements(vdirXml, "VDIR")
            .Where(x => Attribute(x, "path") == "/")
            .ToLookup(x => Attribute(x, "APP.NAME"),
                x => Environment.ExpandEnvironmentVariables(Attribute(x, "physicalPath")), StringComparer.OrdinalIgnoreCase);
        var rootsByPool = ParseElements(applicationXml, "APP")
            .ToLookup(x => Attribute(x, "APPPOOL.NAME"), x => Attribute(x, "APP.NAME"), StringComparer.OrdinalIgnoreCase);

        var sitesByPool = ParseElements(applicationXml, "APP")
            .Select(x => new { Pool = Attribute(x, "APPPOOL.NAME"), Site = Attribute(x, "SITE.NAME") })
            .Where(x => x.Pool.Length > 0 && x.Site.Length > 0)
            .GroupBy(x => x.Pool, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<string>)x.Select(y => y.Site).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(y => y, StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.OrdinalIgnoreCase);
        var detailsBySite = ParseElements(siteXml, "SITE")
            .Select(x => new
            {
                Site = Attribute(x, "SITE.NAME"),
                Details = new SiteBindingDetails(
                    ParseBindingHosts(FirstAttribute(x, "bindings", "BINDINGS", "SITE.BINDINGS")),
                    FirstAttribute(x, "state", "STATE", "SITE.STATE"))
            })
            .Where(x => x.Site.Length > 0)
            .ToDictionary(x => x.Site, x => x.Details, StringComparer.OrdinalIgnoreCase);
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
            siteNames ??= [];
            processIds ??= [];
            var siteBindings = siteNames
                .Select(siteName => detailsBySite.TryGetValue(siteName, out var details)
                    ? new IisSiteBindingMetrics(
                        siteName,
                        details.HostNames,
                        string.IsNullOrWhiteSpace(details.State) ? "Unknown" : details.State)
                    : new IisSiteBindingMetrics(siteName, []))
                .ToList();
            var hostNames = siteBindings
                .SelectMany(site => site.HostNames)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
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
                siteNames,
                hostNames,
                string.IsNullOrWhiteSpace(state) ? (processIds.Count > 0 ? "Started" : "Unknown") : state,
                processIds,
                hasCpu ? Math.Round(Math.Clamp(cpu, 0, 100), 1) : null,
                workingSet,
                privateMemory,
                siteBindings,
                await ReadRuntimeMetricsAsync(rootsByPool[poolName].SelectMany(app => rootsByApplication[app]),
                    processIds, now, cancellationToken)));
        }

        foreach (var stalePid in previousCpu.Keys.Where(x => !currentPids.Contains(x)).ToList())
            previousCpu.Remove(stalePid);
        return output;
    }

    private async Task<IReadOnlyList<RuntimeMetricSnapshot>?> ReadRuntimeMetricsAsync(IEnumerable<string> roots,
        IReadOnlyList<int> pids, DateTime now, CancellationToken cancellationToken)
    {
        if (pids.Count == 0) return null;
        var found = new Dictionary<int, RuntimeMetricSnapshot>();
        foreach (var root in roots.Where(Path.IsPathFullyQualified).Distinct(StringComparer.OrdinalIgnoreCase))
        foreach (var pid in pids)
        {
            if (found.ContainsKey(pid)) continue;
            var file = Path.Combine(root, "App_Data", "runtime-diagnostics", $"{pid}.json");
            try
            {
                if (!File.Exists(file) || new FileInfo(file).Length > 16384) continue;
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read,
                    FileShare.Read | FileShare.Delete, 4096, FileOptions.Asynchronous);
                var sample = await JsonSerializer.DeserializeAsync<RuntimeMetricSnapshot>(stream, cancellationToken: cancellationToken);
                if (sample == null || sample.SchemaVersion != 1 || sample.ProcessId != pid
                    || sample.SampledAtUtc < now.AddSeconds(-90) || sample.SampledAtUtc > now.AddSeconds(5)
                    || sample.RequestCount < 0 || sample.ManagedBytes < 0 || sample.Gen2 < 0) continue;
                using var process = Process.GetProcessById(pid);
                if (Math.Abs((sample.StartedAtUtc - process.StartTime.ToUniversalTime()).TotalSeconds) > 1) continue;
                found[pid] = sample;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
                or ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
            { logger.LogDebug(ex, "Runtime snapshot unavailable for PID {Pid}", pid); }
        }
        // 不把只有部分程序的數字當成整個集區總量。
        return found.Count == pids.Count ? found.Values.OrderBy(x => x.ProcessId).ToArray() : null;
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

    private static IReadOnlyList<string> ParseBindingHosts(string bindings) => bindings
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(binding => binding[(binding.LastIndexOf(':') + 1)..].Trim().TrimEnd('.').ToLowerInvariant())
        .Where(host => host.Length > 0 && Uri.CheckHostName(host) == UriHostNameType.Dns)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(host => host, StringComparer.OrdinalIgnoreCase)
        .ToList();

    private async Task<string> RunAppCmdAsync(
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
            var error = await stderr;
            var output = await stdout;
            if (process.ExitCode == 0) return output;

            logger.LogWarning(
                "appcmd failed with exit code {ExitCode}: {Arguments}; Error={Error}",
                process.ExitCode,
                string.Join(' ', arguments),
                string.IsNullOrWhiteSpace(error) ? "<empty>" : error.Trim());
            return string.Empty;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            return string.Empty;
        }
    }

    private sealed record ProcessCpuSample(DateTime SampledAtUtc, TimeSpan ProcessorTime);
    private sealed record SiteBindingDetails(IReadOnlyList<string> HostNames, string State);
}
