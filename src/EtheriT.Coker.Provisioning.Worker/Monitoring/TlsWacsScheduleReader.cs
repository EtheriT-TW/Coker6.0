using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed partial class TlsCertificateCollector
{
    private TlsWacsSchedule ReadWacsSchedule(CancellationToken cancellationToken)
    {
        var objects = new List<object>();
        var folderPath = options.WacsTaskFolder;
        logger.LogInformation("TLS step 5 started at {TimeUtc}: read local win-acme task schedule", DateTime.UtcNow);
        try
        {
            if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Task Scheduler requires Windows.");
            if (string.IsNullOrWhiteSpace(folderPath) || !folderPath.StartsWith('\\'))
                throw new InvalidOperationException("WacsTaskFolder 必須是工作排程器資料夾路徑，例如 \\。");
            dynamic Track(object value) { objects.Add(value); return value; }
            var type = Type.GetTypeFromProgID("Schedule.Service")
                ?? throw new InvalidOperationException("Task Scheduler COM service is unavailable.");
            dynamic service = Track(Activator.CreateInstance(type)!);
            service.Connect(); // Local service identity; no credentials or remote server are supplied.
            dynamic folder = Track(service.GetFolder(folderPath));
            dynamic tasks = Track(folder.GetTasks(1)); // Include hidden tasks, only in this folder.
            var results = new List<TlsWacsTask>();
            var count = (int)tasks.Count;
            if (count > 1000) throw new InvalidOperationException("排程資料夾超過 1000 個任務，停止讀取。");
            for (var index = 1; index <= count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                dynamic task = Track(tasks.Item(index));
                dynamic definition = Track(task.Definition);
                dynamic actions = Track(definition.Actions);
                var matches = false;
                for (var actionIndex = 1; actionIndex <= (int)actions.Count; actionIndex++)
                {
                    dynamic action = Track(actions.Item(actionIndex));
                    if ((int)action.Type != 0) continue; // Only executable actions.
                    var executable = Environment.ExpandEnvironmentVariables(((string)action.Path).Trim('"'));
                    var arguments = (string)action.Arguments;
                    if (string.Equals(Path.GetFileName(executable), "wacs.exe", StringComparison.OrdinalIgnoreCase)
                        && Regex.IsMatch(arguments, @"(?:^|\s)--renew(?:\s|$)", RegexOptions.IgnoreCase,
                            TimeSpan.FromMilliseconds(100))) matches = true;
                    // Never log or return action arguments: they may contain secrets.
                }
                if (!matches) continue;
                var enabled = (bool)task.Enabled;
                var state = (int)task.State;
                results.Add(new TlsWacsTask((string)task.Path, enabled,
                    state switch { 1 => "已停用", 2 => "佇列中", 3 => "就緒", 4 => "執行中", _ => "無法確認" },
                    enabled ? TaskTime((DateTime)task.NextRunTime) : null,
                    TaskTime((DateTime)task.LastRunTime), (int)task.LastTaskResult));
            }
            logger.LogInformation("TLS step 5 completed at {TimeUtc}: {Count} renewal tasks", DateTime.UtcNow, results.Count);
            return new TlsWacsSchedule(DateTime.UtcNow, folderPath, results, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TLS step 5 failed at {TimeUtc}", DateTime.UtcNow);
            return new TlsWacsSchedule(null, folderPath, [], "第五步 win-acme 排程讀取失敗：" + ex.Message);
        }
        finally
        {
            for (var index = objects.Count - 1; index >= 0; index--)
            {
                try
                {
                    if (Marshal.IsComObject(objects[index])) Marshal.ReleaseComObject(objects[index]);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "TLS step 5 COM cleanup failed");
                }
            }
        }
    }

    private static DateTime? TaskTime(DateTime value) => value.Year <= 1900 ? null
        : DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
}
