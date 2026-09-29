using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models;

public sealed class PlatformRole : FullAuditedEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Sort { get; set; }
    public List<MappingUserAndPlatformRole> Users { get; set; } = [];
}

public static class PlatformRoleCodes
{
    public const string Administrator = "PlatformAdministrator";
    public const string DataManager = "PlatformDataManager";
    public const string ServerOperator = "PlatformServerOperator";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Administrator,
        DataManager,
        ServerOperator
    };
}
