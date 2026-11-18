using System;
using System.Linq;

namespace EtheriT.Coker.Application.Shared.Common;

// Names of seeded parameters, not properties/columns on the StoreSet entity.
public static class AiStoreSettingNames
{
    public const string Enabled = "AiEnabled";
    public const string Provider = "AiProvider";
    public const string Model = "AiModel";
    public const string ApiKey = "AiApiKey";
    public static readonly string[] All = [Enabled, Provider, Model, ApiKey];

    public static bool IsManaged(string? key) => All.Contains(key?.Trim(), StringComparer.OrdinalIgnoreCase) || IsLegacy(key);
    public static bool IsSecret(string? key) => string.Equals(key?.Trim(), ApiKey, StringComparison.OrdinalIgnoreCase) || IsLegacy(key);
    // Protect any value saved by the superseded implementation; never migrate or delete it implicitly.
    private static bool IsLegacy(string? key) => string.Equals(key?.Trim(), "__AiSettingsV1", StringComparison.OrdinalIgnoreCase);
}
