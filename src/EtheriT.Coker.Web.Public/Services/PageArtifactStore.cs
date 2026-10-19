using EtheriT.Coker.Application.Shared;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace EtheriT.Coker.Web.Public.Services;

/// <summary>僅儲存可共用的衍生資料，不儲存 request、會員、nonce 或防偽 Token。</summary>
public sealed class PageArtifactStore(IUploadPathResolver paths, IConfiguration configuration, ILogger<PageArtifactStore> logger)
{
    // 修改處理規則時必須升版。固定鎖數，不保留跨請求的頁面／檔案記憶體快取。
    private const string Version = "public-prepared-v1";
    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, 32).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    private static readonly JsonSerializerSettings JsonSettings = new() { TypeNameHandling = TypeNameHandling.None };
    public sealed class Entry<T>
    {
        public string Fingerprint { get; set; } = "";
        public T Value { get; set; } = default!;
    }

    private static string Fingerprint(IEnumerable<string?> parts)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var part in parts.Prepend(Version))
        {
            var bytes = Encoding.UTF8.GetBytes(part ?? "");
            hash.AppendData(BitConverter.GetBytes(bytes.Length));
            hash.AppendData(bytes);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    public async Task<T> GetAsync<T>(string orgName, long websiteId, string slot,
        IEnumerable<string?> dependencies, Func<Task<T>> create, Func<T, bool>? validate = null)
    {
        if (websiteId <= 0) return await create();
        string file;
        var slotHash = Fingerprint([websiteId.ToString(), orgName, slot]);
        try
        {
            var root = string.IsNullOrWhiteSpace(orgName)
                ? configuration.GetValue<string>("VirtualDirectory:upload") : paths.GetRootPath(orgName);
            if (string.IsNullOrWhiteSpace(root)) return await create();
            file = Path.Combine(root, "_html", websiteId.ToString(), "_prepared",
                slotHash + ".json");
        }
        catch (Exception ex) when (IsStorageError(ex))
        {
            logger.LogWarning(ex, "無法取得頁面產物路徑：Website={WebsiteId}", websiteId);
            return await create();
        }
        var fingerprint = Fingerprint(dependencies);
        async Task<Entry<T>?> ReadAsync()
        {
            try
            {
                if (!File.Exists(file)) return null;
                var cached = JsonConvert.DeserializeObject<Entry<T>>(await ReadTextAsync(file), JsonSettings);
                return cached?.Fingerprint == fingerprint && cached.Value is not null
                    && (validate == null || validate(cached.Value)) ? cached : null;
            }
            catch (Exception ex) when (IsStorageError(ex) || ex is JsonException)
            {
                logger.LogWarning(ex, "頁面產物缺失或無法讀取，重新產生：Website={WebsiteId}, Slot={Slot}", websiteId, slot);
                return null;
            }
        }
        // 命中直接讀取；只在缺檔／版本不符時競爭重建鎖。
        var hit = await ReadAsync();
        if (hit != null) return hit.Value;
        var gate = Gates[(int)(Convert.ToUInt32(slotHash[..8], 16) % (uint)Gates.Length)];
        await gate.WaitAsync();
        try
        {
            hit = await ReadAsync();
            if (hit != null) return hit.Value;

            // 只有版本不符／缺檔才執行處理。每個 slot 覆寫原檔，不隨內容版本累積檔案。
            var value = await create();
            if (validate != null && !validate(value)) return value;
            var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                await File.WriteAllTextAsync(temporary,
                    JsonConvert.SerializeObject(new Entry<T> { Fingerprint = fingerprint, Value = value }, JsonSettings),
                    new UTF8Encoding(false));
                File.Move(temporary, file, true);
            }
            catch (Exception ex) when (IsStorageError(ex))
            {
                logger.LogWarning(ex, "頁面產物無法儲存，使用當次結果：Website={WebsiteId}, Slot={Slot}", websiteId, slot);
            }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (Exception ex) when (IsStorageError(ex)) { logger.LogDebug(ex, "無法移除頁面產物暫存檔"); }
            }
            return value;
        }
        finally { gate.Release(); }
    }

    private static async Task<string> ReadTextAsync(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
            4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static bool IsStorageError(Exception ex) => ex is IOException or UnauthorizedAccessException
        or ArgumentException or NotSupportedException or System.Security.SecurityException;
}
