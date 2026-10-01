namespace EtheriT.Coker.Provisioning.Worker;

public sealed partial class TlsCertificateCollector
{
    private async Task<TlsWacsLogs> ReadWacsLogsAsync(CancellationToken cancellationToken)
    {
        var directory = options.WacsLogDirectory;
        logger.LogInformation("TLS step 4 started at {TimeUtc}: read win-acme logs only", DateTime.UtcNow);
        try
        {
            if (string.IsNullOrWhiteSpace(directory) || !Path.IsPathFullyQualified(directory))
                throw new InvalidOperationException("請設定 WacsLogDirectory 的絕對路徑。");
            var candidates = new List<FileInfo>();
            var enumerated = 0;
            foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++enumerated > 10000) throw new InvalidOperationException("日誌目錄超過 10000 個檔案。");
                var extension = Path.GetExtension(path);
                if (extension.Equals(".log", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)) candidates.Add(new FileInfo(path));
            }
            var results = new List<TlsWacsLogFile>();
            var executions = new List<TlsWacsExecution>();
            foreach (var file in candidates.OrderByDescending(x => x.LastWriteTimeUtc).Take(7))
            {
                cancellationToken.ThrowIfCancellationRequested();
                logger.LogInformation("TLS step 4 reading {FileName} at {TimeUtc}", file.Name, DateTime.UtcNow);
                try
                {
                    if (file.Length > 5_000_000) throw new InvalidOperationException("日誌超過 5 MB，此次略過。");
                    using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    using var reader = new StreamReader(stream);
                    var errors = 0;
                    var warnings = 0;
                    var characters = 0;
                    var parser = new WacsLogParser(file.Name, options.TlsPfxPassword, options.ApiKey);
                    while (await reader.ReadLineAsync(cancellationToken) is { } line)
                    {
                        if ((characters += line.Length) > 5_000_000) throw new InvalidOperationException("日誌讀取超過上限。");
                        parser.AddLine(line);
                        if (line.Contains("[EROR]", StringComparison.Ordinal) || line.Contains("[ERR]", StringComparison.Ordinal)
                            || line.Contains("[ERROR]", StringComparison.Ordinal)) errors++;
                        if (line.Contains("[WARN]", StringComparison.Ordinal) || line.Contains("[WRN]", StringComparison.Ordinal)) warnings++;
                    }
                    results.Add(new TlsWacsLogFile(file.Name, file.LastWriteTimeUtc, errors, warnings, null));
                    executions.AddRange(parser.Finish());
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning("TLS step 4 failed {FileName}: {Error}", file.Name, ex.Message);
                    results.Add(new TlsWacsLogFile(file.Name, file.LastWriteTimeUtc, null, null, ex.Message));
                }
            }
            logger.LogInformation("TLS step 4 completed at {TimeUtc}: {Count} log files", DateTime.UtcNow, results.Count);
            return new TlsWacsLogs(DateTime.UtcNow, directory, results, null, executions);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TLS step 4 failed at {TimeUtc}", DateTime.UtcNow);
            return new TlsWacsLogs(null, directory, [], "第四步 win-acme 日誌讀取失敗：" + ex.Message);
        }
    }
}
