using System.Globalization;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>Conservative sequential renewal parser. Generic errors never establish a failed renewal.</summary>
internal sealed class WacsLogParser(string fileName, params string[] secrets)
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly Regex Entry = new(@"^\s*(?<time>.*?)\s*\[(?<level>INF|INFO|ERR|EROR|ERROR|WRN|WARN|DBG|DBUG|VRB|VERB|FTL|FATL)\]\s*(?<message>.*)$",
        RegexOptions.CultureInvariant, MatchTimeout);
    private static readonly Regex Start = new(@"^(?:Force )?[Rr]enewing (?:certificate for )?(?<task>.+)$",
        RegexOptions.CultureInvariant, MatchTimeout);
    private static readonly Regex End = new(@"^Renewal for (?<task>.+?) (?<result>succeeded(?: with errors)?|failed)(?:[, .].*)?$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, MatchTimeout);
    private static readonly Regex Sensitive = new(@"password|passwd|private.?key|api.?key|secret|token|authorization|credential|cookie|bearer|signature|challenge|connection.?string|BEGIN.*(?:KEY|CERTIFICATE)|(?:command line|parameters|arguments|response content|request body)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout);
    private readonly List<TlsWacsExecution> records = [];
    private Pending? pending;
    private int lineNumber;

    public void AddLine(string line)
    {
        lineNumber++;
        if (line.Length > 16000) return; // No raw stack traces, JSON bodies, or unbounded lines are returned.
        var entry = Entry.Match(line);
        if (!entry.Success) return;
        var message = entry.Groups["message"].Value;
        var level = entry.Groups["level"].Value;
        var timestamp = ParseTime(entry.Groups["time"].Value);
        var start = Start.Match(message);
        var end = End.Match(message);
        if (pending is not null && !start.Success && !end.Success) pending.EndLine = lineNumber;
        if (start.Success)
        {
            Flush();
            pending = new Pending(Clean(start.Groups["task"].Value), timestamp, lineNumber);
        }
        else if (end.Success)
        {
            var task = Clean(end.Groups["task"].Value);
            // A different terminal task must not inherit the previous task's error messages.
            if (pending?.Task != task) { Flush(); pending = new Pending(task, timestamp, lineNumber); }
            var outcome = end.Groups["result"].Value;
            pending!.Result = outcome.StartsWith("failed", StringComparison.OrdinalIgnoreCase) ? "失敗"
                : outcome.Contains("errors", StringComparison.OrdinalIgnoreCase) ? "成功但有錯誤" : "成功";
            pending.Completed = timestamp;
            pending.EndLine = lineNumber;
            AddDetail(message);
            Flush();
        }
        else if (level is "ERR" or "EROR" or "ERROR" or "FTL" or "FATL" or "WARN" or "WRN")
        {
            pending ??= new Pending("未對應任務", timestamp, lineNumber);
            pending.EndLine = lineNumber;
            AddDetail(message);
        }
        else if (pending?.Task == "未對應任務") Flush();
        // Explicit session boundaries prevent carrying task context across separate invocations.
        if (message.StartsWith("A simple Windows ACME", StringComparison.OrdinalIgnoreCase)
            || message.StartsWith("Exiting with status code", StringComparison.OrdinalIgnoreCase)) Flush();
    }

    private void AddDetail(string message)
    {
        if (pending is null || pending.Details.Count >= 3) return;
        var clean = Clean(message);
        if (!pending.Details.Contains(clean)) pending.Details.Add(clean);
    }

    private void Flush()
    {
        if (pending is null) return;
        if (records.Count >= 300) records.RemoveAt(0);
        records.Add(new TlsWacsExecution($"{fileName}:{pending.StartLine}", pending.Started, pending.Completed,
                pending.Task, pending.Result, pending.Details.Count > 0 ? string.Join("；", pending.Details)
                    : "未找到明確的續期結束訊息", fileName, pending.StartLine, pending.EndLine));
        pending = null;
    }

    public IReadOnlyList<TlsWacsExecution> Finish()
    {
        Flush();
        return records;
    }

    private static DateTime? ParseTime(string value)
    {
        // Require a full date; never substitute file modification time for execution time.
        if (!Regex.IsMatch(value, @"^\d{4}[-/]\d{2}[-/]\d{2}[ T]", RegexOptions.CultureInvariant, MatchTimeout)) return null;
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces,
            out var parsed) ? parsed.UtcDateTime : null;
    }

    private string Clean(string message)
    {
        foreach (var secret in secrets.Where(value => !string.IsNullOrEmpty(value)))
            message = message.Replace(secret, "[已遮蔽]", StringComparison.Ordinal);
        if (Sensitive.IsMatch(message)) return "[含敏感欄位或執行參數，訊息已遮蔽]";
        message = Regex.Replace(message, @"https?://\S+", "[URL 已遮蔽]", RegexOptions.IgnoreCase, MatchTimeout);
        message = Regex.Replace(message, @"[A-Za-z0-9_+/=-]{48,}", "[長字串已遮蔽]", RegexOptions.CultureInvariant, MatchTimeout);
        return message.Length > 500 ? message[..500] + "…" : message;
    }

    private sealed class Pending(string task, DateTime? started, int line)
    {
        public string Task { get; } = task;
        public DateTime? Started { get; } = started;
        public DateTime? Completed { get; set; }
        public string Result { get; set; } = "無法確認";
        public int StartLine { get; } = line;
        public int EndLine { get; set; } = line;
        public List<string> Details { get; } = [];
    }
}
