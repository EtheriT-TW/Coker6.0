using System.Collections.Concurrent;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed class DiagnosticsRunGate : IDisposable
{
    private static readonly ConcurrentDictionary<(long Website, long User), byte> Active = new();
    private readonly (long Website, long User) key;
    private DiagnosticsRunGate((long Website, long User) key) => this.key = key;
    public static DiagnosticsRunGate? TryEnter(long websiteId, long userId)
    {
        var key = (websiteId, userId);
        return Active.TryAdd(key, 0) ? new DiagnosticsRunGate(key) : null;
    }
    public void Dispose() => Active.TryRemove(key, out _);
}
