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

        var hostIpAddresses = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses).Select(a => a.Address)
                    .Where(a => !System.Net.IPAddress.IsLoopback(a)).Select(a => a.ToString()).Distinct().ToArray();

        var applicationXml = await RunAppCmdAsync(appCmd, ["list", "app", "/xml"], cancellationToken);
        var siteXml = await RunAppCmdAsync(appCmd, ["list", "site", "/xml"], cancellationToken);
        var workerXml = await RunAppCmdAsync(appCmd, ["list", "wp", "/xml"], cancellationToken);
        var poolXml = await RunAppCmdAsync(appCmd, ["list", "apppool", "/xml"], cancellationToken);
        var vdirXml = await RunAppCmdAsync(appCmd, ["list", "vdir", "/xml"], cancellationToken);
        var rootsByApplication = ParseElements(vdirXml, "VDIR")
            .Where(x => Attribute(x, "path") == "/" || (Attribute(x, "path").Length == 0
                && Attribute(x, "VDIR.NAME").TrimEnd('/') == Attribute(x, "APP.NAME").TrimEnd('/')))
            .ToLookup(x => Attribute(x, "APP.NAME").Trim().TrimEnd('/'),
                x => Environment.ExpandEnvironmentVariables(Attribute(x, "physicalPath")), StringComparer.OrdinalIgnoreCase);
        var rootsByPool = ParseElements(applicationXml, "APP")
            .ToLookup(x => Attribute(x, "APPPOOL.NAME"), x => Attribute(x, "APP.NAME").Trim().TrimEnd('/'), StringComparer.OrdinalIgnoreCase);

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

            var runtime = await ReadRuntimeMetricsAsync(rootsByPool[poolName].SelectMany(app => rootsByApplication[app]),
                processIds, now, cancellationToken);
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
                runtime.Snapshots,
                runtime.Error, hostIpAddresses));
        }

        foreach (var stalePid in previousCpu.Keys.Where(x => !currentPids.Contains(x)).ToList())
            previousCpu.Remove(stalePid);
        return output;
    }

    private sealed record RuntimeReadResult(IReadOnlyList<RuntimeMetricSnapshot>? Snapshots, string? Error);

    private async Task<RuntimeReadResult> ReadRuntimeMetricsAsync(IEnumerable<string> roots,
        IReadOnlyList<int> pids, DateTime now, CancellationToken cancellationToken)
    {
        if (pids.Count == 0) return new(null, "集區目前沒有執行程序；請先開啟網站，等待下一次回報。");
        var directories = roots.Where(root => !string.IsNullOrWhiteSpace(root) && Path.IsPathFullyQualified(root))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (directories.Length == 0) return new(null, "Worker 無法從 IIS 取得網站實體根目錄，無法定位診斷快照。");
        var found = new Dictionary<int, RuntimeMetricSnapshot>();
        var errors = new Dictionary<int, string>();
        foreach (var root in directories)
        foreach (var pid in pids)
        {
            if (found.ContainsKey(pid)) continue;
            var file = Path.Combine(root, "App_Data", "runtime-diagnostics", $"{pid}.json");
            try
            {
                // 直接開檔才能區分不存在與權限不足；File.Exists會把部分權限錯誤也當成false。
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read,
                    FileShare.Read | FileShare.Delete, 4096, FileOptions.Asynchronous);
                if (stream.Length > 16384) { errors[pid] = $"PID {pid}：診斷檔超過16 KiB上限。"; continue; }
                var sample = await JsonSerializer.DeserializeAsync<RuntimeMetricSnapshot>(stream, cancellationToken: cancellationToken);
                if (sample == null || sample.SchemaVersion != 1 || sample.RequestCount < 0 || sample.ManagedBytes < 0 || sample.Gen2 < 0)
                { errors[pid] = $"PID {pid}：診斷檔內容或版本無效，請確認Public與Worker版本。"; continue; }
                if (sample.ProcessId != pid)
                { errors[pid] = $"PID {pid}：檔案內PID為{sample.ProcessId}，與IIS程序不一致。"; continue; }
                if (sample.SampledAtUtc < now.AddSeconds(-90) || sample.SampledAtUtc > now.AddSeconds(5))
                { errors[pid] = $"PID {pid}：診斷檔過期或時間異常，快照時間{sample.SampledAtUtc:O}，Worker時間{now:O}。"; continue; }
                using var process = Process.GetProcessById(pid);
                if (Math.Abs((sample.StartedAtUtc - process.StartTime.ToUniversalTime()).TotalSeconds) > 1)
                { errors[pid] = $"PID {pid}：程序啟動時間不符，可能是舊快照或PID已重用。"; continue; }
                found[pid] = sample;
                errors.Remove(pid);
            }
            catch (FileNotFoundException)
            { errors.TryAdd(pid, $"PID {pid}：找不到 {file}。請確認Public已啟用採樣；若產生的是不同PID檔案，請確認IIS hostingModel是否為inprocess。"); }
            catch (DirectoryNotFoundException)
            { errors.TryAdd(pid, $"PID {pid}：診斷目錄不存在，預期檔案為 {file}。請確認Public版本、採樣開關與寫入權限。"); }
            catch (UnauthorizedAccessException)
            { errors[pid] = $"PID {pid}：Worker沒有讀取 {file} 的權限。"; }
            catch (JsonException)
            { errors[pid] = $"PID {pid}：診斷JSON無法解析，請確認檔案內容與版本。"; }
            catch (System.ComponentModel.Win32Exception)
            { errors[pid] = $"PID {pid}：Worker無法讀取程序啟動時間，請確認程序讀取權限。"; }
            catch (Exception ex) when (ex is IOException or ArgumentException or InvalidOperationException)
            {
                errors[pid] = $"PID {pid}：診斷檔或程序暫時無法讀取（{ex.GetType().Name}）。";
                logger.LogDebug(ex, "Runtime snapshot unavailable for PID {Pid}", pid);
            }
        }
        // 不把只有部分程序的數字當成整個集區總量。
        return found.Count == pids.Count
            ? new(found.Values.OrderBy(x => x.ProcessId).ToArray(), null)
            : new(null, string.Join("\n", errors.OrderBy(x => x.Key).Take(8).Select(x => x.Value)));
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
