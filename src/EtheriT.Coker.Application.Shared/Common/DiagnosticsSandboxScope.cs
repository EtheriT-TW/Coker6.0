namespace EtheriT.Coker.Application.Shared.Common;

// Only server-side diagnostics code can enter this scope. Never enabled by a
// request header/cookie or client supplied flag.
public sealed class DiagnosticsSandboxScope : IDisposable
{
    private static readonly AsyncLocal<DiagnosticsSandboxScope?> Slot = new();
    private readonly DiagnosticsSandboxScope? previous;
    public static DiagnosticsSandboxScope? Current => Slot.Value;
    public static bool IsActive => Current != null;
    public List<DiagnosticsCapturedMail> Mail { get; } = new();
    public DiagnosticsSandboxScope() { previous = Slot.Value; Slot.Value = this; }
    public void Dispose() => Slot.Value = previous;
}

public sealed record DiagnosticsCapturedMail(string Subject, string[] Recipients, string? Html, string? Text);
