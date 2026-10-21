using System.Text.Json;
using System.Xml.Linq;

namespace EtheriT.Coker.Provisioning.Worker;

// Read only the public source fields; never report passwords or raw renewal JSON.
internal static class WacsRenewalInventory
{
    internal sealed record Renewal(string Id, string[] SiteIds, string[] Hosts, bool AllSites, bool Filtered,
        string SourceProperty);

    internal static IReadOnlyList<Renewal> Read(ProvisioningWorkerOptions options)
    {
        if (!Path.IsPathFullyQualified(options.WacsRenewalDirectory))
            throw new InvalidOperationException("WacsRenewalDirectory 必須是 win-acme 使用的續期設定絕對路徑。");
        var files = Directory.EnumerateFiles(options.WacsRenewalDirectory, "*.renewal.json").Take(1001).ToArray();
        if (files.Length > 1000) throw new InvalidOperationException("續期設定超過 1000 筆。");
        var result = new List<Renewal>();
        foreach (var path in files)
        {
            if (new FileInfo(path).Length > 5_000_000) throw new InvalidOperationException("續期設定檔過大，停止盤點。");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            var id = root.GetProperty("Id").GetString();
            if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("續期設定缺少 ID。");
            if (!string.Equals(Path.GetFileName(path), id + ".renewal.json", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("續期 ID 與檔名不一致，停止盤點。");
            var sourceProperty = root.TryGetProperty("SourcePluginOptions", out _) ? "SourcePluginOptions" : "TargetPluginOptions";
            if (!root.TryGetProperty(sourceProperty, out var source) || source.ValueKind != JsonValueKind.Object
                || (root.TryGetProperty("SourcePluginOptions", out _) && root.TryGetProperty("TargetPluginOptions", out _)))
                throw new InvalidOperationException($"續期檔 {Path.GetFileName(path)} 無法辨識唯一來源區塊（SourcePluginOptions／TargetPluginOptions）。");
            var siteProperty = sourceProperty == "TargetPluginOptions" ? "IncludeSiteIds" : "SiteIds";
            if (!source.TryGetProperty(siteProperty, out _))
                throw new InvalidOperationException($"續期檔 {Path.GetFileName(path)} 的 {sourceProperty} 缺少 {siteProperty}，無法確認 IIS 站台；停止操作以避免重複建立。");
            var sites = Values(source, siteProperty);
            var hosts = Values(source, "IncludeHosts");
            var types = Values(source, "IncludeTypes");
            // Pattern/exclusion filters need manual review before creating overlapping settings.
            var filtered = source.EnumerateObject().Any(property =>
                property.Name != siteProperty
                && property.Name is not ("Plugin" or "IncludeHosts" or "CommonName" or "IncludeTypes")
                && property.Value.ValueKind is not (JsonValueKind.Null or JsonValueKind.False)
                && property.Value.ToString() is not ("" or "[]"));
            // HTTP-only IIS sources are normal: binding synchronization adds HTTP for every desired host.
            filtered |= types.Length > 0 && (!types.Contains("http", StringComparer.OrdinalIgnoreCase)
                || types.Any(type => !string.Equals(type, "http", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(type, "https", StringComparison.OrdinalIgnoreCase)));
            result.Add(new Renewal(id, sites, hosts, sites.Length == 0, filtered, sourceProperty));
        }
        return result;
    }

    private static string[] Values(JsonElement source, string name)
    {
        if (!source.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return [];
        if (value.ValueKind != JsonValueKind.Array) throw new InvalidOperationException("無法辨識續期來源欄位：" + name);
        return value.EnumerateArray().Select(item => item.ToString()).ToArray();
    }

    internal static string SiteId(XElement site) => (string?)site.Attribute("SITE.ID")
        ?? throw new InvalidOperationException("IIS 網站缺少 ID。");

    internal static string[] Hosts(XElement site) => ((string?)site.Attribute("bindings") ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(binding => binding.StartsWith("http/", StringComparison.OrdinalIgnoreCase)
            || binding.StartsWith("https/", StringComparison.OrdinalIgnoreCase))
        .Select(binding => binding[(binding.LastIndexOf(':') + 1)..].ToLowerInvariant())
        .Where(host => !string.IsNullOrWhiteSpace(host)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    internal static bool Matches(Renewal renewal, XElement site) =>
        (renewal.AllSites || renewal.SiteIds.Contains(SiteId(site)))
        && (renewal.Hosts.Length == 0 || Hosts(site).Any(host => renewal.Hosts.Contains(host, StringComparer.OrdinalIgnoreCase)));

    internal static string Advice(Renewal renewal) => renewal.AllSites || renewal.SiteIds.Length > 1
        ? "此續期設定涵蓋多站，安裝／更新時將沿用整筆續期。建議逐站建立獨立設定，確認接手成功後再取消共用設定。"
        : "";
}
