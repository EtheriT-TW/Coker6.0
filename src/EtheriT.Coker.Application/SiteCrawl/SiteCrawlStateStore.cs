using System.Text.Json;
using System.Text.Json.Serialization;
using EtheriT.Coker.Application.Shared;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteCrawlStateStore(IUploadPathResolver paths)
{
    private const int Version = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task<SiteCrawlImportState> LoadAsync(string orgName, Uri source, CancellationToken token)
    {
        var path = Path.Combine(paths.GetRootPath(orgName), "_crawl", "import-state.json");
        var state = await ReadAsync(path, token);
        var recovered = false;
        if (state == null)
        {
            state = await ReadAsync(path + ".bak", token);
            recovered = state != null;
        }
        if (state != null && NormalizeHost(state.SourceHost) != NormalizeHost(source.IdnHost))
            throw new SiteCrawlSourceMismatchException(state.SourceHost, source.IdnHost);
        return new SiteCrawlImportState(path, state ?? new SiteCrawlState(Version, source.IdnHost, null, new()), recovered, state != null);
    }

    internal static string NormalizeHost(string host)
    {
        host = host.ToLowerInvariant();
        return host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host;
    }

    private static async Task<SiteCrawlState?> ReadAsync(string path, CancellationToken token)
    {
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var state = await JsonSerializer.DeserializeAsync<SiteCrawlState>(stream, JsonOptions, token);
            return state is { Version: Version, Items: not null } && !string.IsNullOrWhiteSpace(state.SourceHost)
                && state.Items.Values.All(item => item is { NewId: > 0 }) ? state : null;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or JsonException)
        {
            return null;
        }
    }

    internal static async Task WriteAsync(string path, SiteCrawlState state, bool recovered)
    {
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp";
        await using (var stream = File.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, state, JsonOptions);

        if (File.Exists(path))
        {
            // 正式檔損毀時保留仍可用的備份，避免用壞檔蓋掉最後一份有效對照。
            File.Replace(temporary, path, recovered ? null : path + ".bak");
        }
        else
        {
            File.Move(temporary, path);
        }
    }
}

public sealed class SiteCrawlSourceMismatchException(string original, string requested) : InvalidOperationException(
    $"這個網站的匯入來源為「{original}」，無法從「{requested}」匯入。如果舊站確實換了網域，請刪除該站 upload 下的 _crawl 資料夾後重新匯入。")
{
}

public sealed class SiteCrawlImportState
{
    private readonly string path;
    private SiteCrawlState state;
    private bool recovered;
    private bool dirty;

    internal bool HasSavedState { get; }

    internal SiteCrawlImportState(string path, SiteCrawlState state, bool recovered, bool hasSavedState)
    {
        this.path = path;
        this.state = state;
        this.recovered = recovered;
        HasSavedState = hasSavedState;
    }

    internal SiteCrawlStateItem? Find(string key) => state.Items.GetValueOrDefault(key);
    internal IReadOnlyDictionary<string, SiteCrawlStateItem> Items => state.Items;

    internal void RememberList(string key, long id, Uri list)
    {
        if (Find(key) is not { } previous || previous.NewId != id || previous.SourceListUrl == list.AbsoluteUri) return;
        state.Items[key] = previous with { SourceListUrl = list.AbsoluteUri };
        dirty = true;
    }

    internal string MenuKey(Uri url)
    {
        var key = "Menu|" + SiteLayoutScanner.PageKey(url);
        if (Find(key) != null || url.HostNameType != UriHostNameType.Dns) return key;
        var host = SiteCrawlStateStore.NormalizeHost(url.IdnHost);
        var alternate = new UriBuilder(url) { Host = host == url.IdnHost ? "www." + host : host }.Uri;
        var alternateKey = "Menu|" + SiteLayoutScanner.PageKey(alternate);
        return Find(alternateKey) != null ? alternateKey : key;
    }

    public Task RecordTargetAsync(string key, long id, Uri? source, string? title, bool created) =>
        RecordTargetAsync(key, id, source, title, created, null, null);

    internal async Task RecordTargetAsync(string key, long id, Uri? source, string? title, bool created,
        string? writtenTitle, SiteCrawlFields? writtenFields)
    {
        if (state.Items.TryGetValue(key, out var previous) && previous.NewId == id)
        {
            if (writtenFields == null || writtenFields == previous.WrittenFields) return;
            state.Items[key] = previous with { WrittenFields = writtenFields };
        }
        else
        {
            // 先保留已取得的 Id。內容半途失敗時，空指紋可以讓下次接著寫。
            state.Items[key] = new SiteCrawlStateItem(id, null, null, writtenTitle, DateTimeOffset.UtcNow,
                source?.AbsoluteUri, title, writtenFields);
        }
        dirty = true;
        if (created) await SaveAsync();
    }

    internal void RecordWritten(string key, long id, Uri? source, string? title, string? sourceHash, SiteCrawlWritten written,
        SiteCrawlFields? sourceFields = null, string? desktopImage = null, string? sourceListUrl = null)
    {
        var previous = Find(key);
        if (sourceListUrl == null && previous?.NewId == id) sourceListUrl = previous.SourceListUrl;
        state.Items[key] = new SiteCrawlStateItem(id, sourceHash, written.Hash, written.Title, DateTimeOffset.UtcNow,
            source?.AbsoluteUri, title, written.Fields, sourceFields, desktopImage, written.DraftHash, sourceListUrl, true);
        dirty = true;
    }

    public void RememberConflict(string key, long id, Uri source, string title, string sourceHash)
    {
        var previous = Find(key) is { } item && item.NewId == id
            ? item : new SiteCrawlStateItem(id, null, null, null, DateTimeOffset.UtcNow, null, null);
        // 已看過的來源版本可向前推進，寫入基準仍只來自實際匯入。
        state.Items[key] = previous with { SourceHash = sourceHash, SourceUrl = source.AbsoluteUri, SourceTitle = title };
        dirty = true;
    }

    internal void RecordFields(string key, long id, SiteCrawlFields written, SiteCrawlFields? source, Uri? sourceList)
    {
        if (Find(key) is not { } previous || previous.NewId != id) return;
        state.Items[key] = previous with
        {
            WrittenFields = written,
            SourceFields = source,
            SourceListUrl = sourceList?.AbsoluteUri ?? previous.SourceListUrl
        };
        dirty = true;
    }

    internal void Complete()
    {
        state = state with { LastImportedAt = DateTimeOffset.UtcNow };
        dirty = true;
    }

    public async Task SaveAsync()
    {
        if (!dirty) return;
        await SiteCrawlStateStore.WriteAsync(path, state, recovered);
        recovered = false;
        dirty = false;
    }
}

internal sealed record SiteCrawlState(
    [property: JsonRequired] int Version,
    [property: JsonRequired] string SourceHost,
    DateTimeOffset? LastImportedAt,
    [property: JsonRequired] Dictionary<string, SiteCrawlStateItem> Items);

internal sealed record SiteCrawlStateItem(long NewId, string? SourceHash, string? WrittenHash, string? WrittenTitle,
    DateTimeOffset ImportedAt, string? SourceUrl, string? SourceTitle, SiteCrawlFields? WrittenFields = null,
    SiteCrawlFields? SourceFields = null, string? DesktopImage = null, string? WrittenDraftHash = null, string? SourceListUrl = null,
    bool HasVerifiedContentBaseline = false);

internal sealed record SiteCrawlFields(DateTime? NodeDate = null, string? Description = null, string? Introduction = null);
