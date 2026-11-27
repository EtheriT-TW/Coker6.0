using System.Globalization;
using System.Text.Json;
using EtheriT.Coker.Application.Shared;

namespace EtheriT.Coker.Application.SiteCrawl;

public enum SiteCrawlConflictKind { Menu, Article, Product, Footer }

public sealed record SiteCrawlPendingItem(long TaskId, SiteCrawlConflictKind Kind, long NewId, string StateKey,
    string SourceUrl, string? NewUrl, string Title, string SourceHash, string Html, string Css,
    string ExpectedHash, string ExpectedDraftHash, string? RenameFrom = null, string? RenameTo = null,
    bool HideGlobalFooter = false)
{
    public string Id => Kind + "-" + NewId.ToString(CultureInfo.InvariantCulture);

    public bool Matches(string? html, string? css, string? draftHtml, string? draftCss) =>
        ExpectedHash == SiteCrawlFingerprint.ComputeWritten(html, css)
        && ExpectedDraftHash == SiteCrawlFingerprint.ComputeWritten(draftHtml, draftCss);
}

public sealed class SiteCrawlPendingStore(IUploadPathResolver paths)
{
    private static readonly JsonSerializerOptions Json = JsonSerializerOptions.Web;

    private string Folder(string orgName) => Path.Combine(paths.GetRootPath(orgName), "_crawl", "pending");

    internal void Clear(string orgName)
    {
        var folder = Folder(orgName);
        if (!System.IO.Directory.Exists(folder)) return;
        foreach (var file in System.IO.Directory.EnumerateFiles(folder))
            if (IsItemFile(file) || Path.GetFileName(file) is "removals.json" or "removals.json.tmp") File.Delete(file);
    }

    public async Task<SiteCrawlRemovalBatch> ReadRemovalsAsync(string orgName, long taskId, CancellationToken token)
    {
        var path = Path.Combine(Folder(orgName), "removals.json");
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var batch = await JsonSerializer.DeserializeAsync<SiteCrawlRemovalBatch>(stream, Json, token);
            if (batch?.TaskId == taskId) return batch;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            // 此任務沒有待處理的來源移除項目。
        }
        return new(taskId, [], []);
    }

    public async Task WriteRemovalsAsync(string orgName, SiteCrawlRemovalBatch batch)
    {
        var folder = Folder(orgName);
        System.IO.Directory.CreateDirectory(folder);
        await WriteJsonAsync(Path.Combine(folder, "removals.json"), batch);
    }

    public async Task WriteAsync(string orgName, SiteCrawlPendingItem item)
    {
        if (item.NewId < 1 || !Enum.IsDefined(item.Kind)) throw new ArgumentException("無效的匯入項目。", nameof(item));
        var folder = Folder(orgName);
        System.IO.Directory.CreateDirectory(folder);
        await WriteJsonAsync(Path.Combine(folder, item.Id + ".json"), item);
    }

    private static async Task WriteJsonAsync<T>(string file, T item)
    {
        var tmp = file + ".tmp";
        await using (var stream = File.Create(tmp))
            await JsonSerializer.SerializeAsync(stream, item, Json);
        File.Move(tmp, file, overwrite: true);
    }

    public async Task<IReadOnlyList<SiteCrawlPendingItem>> ReadAsync(string orgName, long taskId, CancellationToken token)
    {
        var folder = Folder(orgName);
        var items = new List<SiteCrawlPendingItem>();
        if (!System.IO.Directory.Exists(folder)) return items;
        foreach (var file in System.IO.Directory.EnumerateFiles(folder, "*.json").Where(IsItemFile))
        {
            try
            {
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
                var item = await JsonSerializer.DeserializeAsync<SiteCrawlPendingItem>(stream, Json, token);
                if (item != null && item.TaskId == taskId && item.Id + ".json" == Path.GetFileName(file)) items.Add(item);
            }
            catch (FileNotFoundException)
            {
                // 其他分頁可能已套用並移除該筆，重新載入時自然不再顯示。
            }
        }
        return items.OrderBy(x => x.Kind).ThenBy(x => x.Title).ThenBy(x => x.NewId).ToList();
    }

    internal void Remove(string orgName, SiteCrawlPendingItem item) =>
        File.Delete(Path.Combine(Folder(orgName), item.Id + ".json"));

    private static bool IsItemFile(string file)
    {
        var name = Path.GetFileName(file);
        if (name.EndsWith(".json.tmp", StringComparison.Ordinal)) name = name[..^4];
        if (!name.EndsWith(".json", StringComparison.Ordinal)) return false;
        var parts = name[..^5].Split('-', 2);
        return parts.Length == 2 && Enum.TryParse<SiteCrawlConflictKind>(parts[0], out var kind) && kind.ToString() == parts[0]
            && long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0;
    }
}
