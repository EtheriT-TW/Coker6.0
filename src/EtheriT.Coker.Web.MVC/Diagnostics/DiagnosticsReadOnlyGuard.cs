using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

// The allowlisted application queries use the normal scoped DbContext, but may
// never persist data during a diagnostic run, even after a future service change.
public sealed class DiagnosticsReadOnlySession : IDisposable
{
    private static readonly AsyncLocal<DiagnosticsReadOnlySession?> Slot = new();
    private readonly DiagnosticsReadOnlySession? previous;
    private readonly CancellationToken token;
    private readonly List<CancellationTokenRegistration> cancellations = new();
    public static DiagnosticsReadOnlySession? Current => Slot.Value;
    public bool WriteAttempted { get; private set; }
    public bool QueryFailed { get; private set; }
    public void RecordQueryFailure() => QueryFailed = true;

    public DiagnosticsReadOnlySession(CancellationToken token = default)
    {
        this.token = token;
        previous = Slot.Value;
        Slot.Value = this;
    }

    public void RegisterCancellation(DbCommand command)
    {
        token.ThrowIfCancellationRequested();
        if (token.CanBeCanceled) cancellations.Add(token.Register(() =>
        {
            try { command.Cancel(); }
            catch (Exception) { /* The command may already have been disposed. */ }
        }));
    }

    public void RejectWrite()
    {
        WriteAttempted = true;
        throw new InvalidOperationException("Diagnostic queries cannot write data.");
    }

    public void Dispose()
    {
        foreach (var registration in cancellations) registration.Dispose();
        Slot.Value = previous;
    }
}

public sealed class DiagnosticsSaveGuard : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DiagnosticsReadOnlySession.Current?.RejectWrite();
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        DiagnosticsReadOnlySession.Current?.RejectWrite();
        return ValueTask.FromResult(result);
    }
}

public sealed class DiagnosticsCommandGuard : DbCommandInterceptor
{
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData) =>
        DiagnosticsReadOnlySession.Current?.RecordQueryFailure();

    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        DiagnosticsReadOnlySession.Current?.RecordQueryFailure();
        return Task.CompletedTask;
    }

    private static void Check(DbCommand command)
    {
        var session = DiagnosticsReadOnlySession.Current;
        if (session == null) return;
        // Strip literals, identifiers and comments before inspecting SQL keywords.
        var sql = Regex.Replace(command.CommandText,
            @"'(?:''|[^'])*'|\[(?:\]\]|[^\]])*\]|""(?:""""|[^""])*""|--[^\r\n]*|/\*[\s\S]*?\*/", " ");
        if (!Regex.IsMatch(sql, @"^\s*SELECT\b", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(sql, @"\b(INSERT|UPDATE|DELETE|MERGE|EXEC|EXECUTE|INTO|ALTER|DROP|CREATE|TRUNCATE|DBCC|BACKUP|RESTORE|OPENROWSET|OPENQUERY)\b|\bNEXT\s+VALUE\s+FOR\b", RegexOptions.IgnoreCase))
            session.RejectWrite();
        session.RegisterCancellation(command);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    { Check(command); return result; }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    { Check(command); return ValueTask.FromResult(result); }
    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    { Check(command); return result; }
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<object> result, CancellationToken cancellationToken = default)
    { Check(command); return ValueTask.FromResult(result); }
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    { DiagnosticsReadOnlySession.Current?.RejectWrite(); return result; }
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    { DiagnosticsReadOnlySession.Current?.RejectWrite(); return ValueTask.FromResult(result); }
}
