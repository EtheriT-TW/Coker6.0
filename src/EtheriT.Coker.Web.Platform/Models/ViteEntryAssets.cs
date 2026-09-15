namespace EtheriT.Coker.Web.Platform.Models;

public sealed record ViteEntryAssets(
    string? ScriptPath,
    IReadOnlyList<string> StylePaths)
{
    public bool IsAvailable => !string.IsNullOrWhiteSpace(ScriptPath);
}
