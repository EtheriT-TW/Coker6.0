using EtheriT.Coker.Application.Shared;
using HtmlAgilityPack;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.Processor;

/// <summary>可刪除重建的已發布片段；不儲存 nonce、會員或其他 request 資料。</summary>
internal static class PublishedHtmlFiles
{
    private const string PipelineVersion = "published-files-v2";
    internal sealed record Result(string Html, string? FirstImageUrl, bool HasImageMetadata = true);
    private static readonly Regex UploadUrls = new(
        @"(?<![\w:/])/upload/[^\s\""'(),<>]+", RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100));
    // 固定數量，避免每份內容都永久保留一把鎖。
    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, 64)
        .Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    internal sealed class Asset
    {
        public Asset() { }
        public string Url { get; set; } = "";
        public string Hash { get; set; } = "";
        public long Length { get; set; }
        public long ModifiedTicks { get; set; }
    }

    internal sealed class Manifest
    {
        public Manifest() { }
        public string Version { get; set; } = "";
        public string InputHash { get; set; } = "";
        public string OutputHash { get; set; } = "";
        public string? FirstImageUrl { get; set; }
        public List<Asset> Assets { get; set; } = [];
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    public static async Task<Result> GetAsync(IUploadPathResolver paths, string orgName,
        long websiteId, int sourceType, long sourceId, string contentKey, string policy, string html,
        Action<Exception> reportStorageFailure)
    {
        var root = Path.GetFullPath(paths.GetRootPath(orgName));
        var identity = Hash($"{websiteId}\n{sourceType}\n{sourceId}\n{contentKey}\n{policy}");
        var directory = Path.Combine(root, "_html", websiteId.ToString(), sourceType.ToString(), sourceId.ToString());
        var file = Path.Combine(directory, identity + ".html");
        var manifestPath = file + ".json";
        var inputHash = Hash(html);
        async Task<(Manifest? Manifest, Result? Content)> ReadAsync()
        {
            Manifest? previous = null;
            if (File.Exists(manifestPath))
            {
                try
                {
                    previous = JsonSerializer.Deserialize<Manifest>(await ReadTextAsync(manifestPath));
                    if (previous != null) previous.Assets ??= [];
                }
                catch (Exception ex) when (ex is JsonException or FileNotFoundException or DirectoryNotFoundException)
                { /* 損壞或讀取前被移除的中繼資料視為缺檔。 */ }
            }

            if (previous?.Assets.Any(asset => asset == null || string.IsNullOrEmpty(asset.Url)
                || (asset.Length >= 0 && (string.IsNullOrEmpty(asset.Hash)
                    || asset.Hash.Length != 64 || !asset.Hash.All(Uri.IsHexDigit)))) == true)
                previous = null;

            if (previous?.Version == PipelineVersion && previous.InputHash == inputHash && File.Exists(file)
                && previous.Assets.All(asset => IsCurrent(root, orgName, asset)))
            {
                try
                {
                    var cachedHtml = await ReadTextAsync(file);
                    // HTML 與 manifest 分開原子替換，跨程序讀到不同批次時重新產生。
                    if (Hash(cachedHtml) == previous.OutputHash)
                        return (previous, new Result(cachedHtml, previous.FirstImageUrl));
                }
                catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                { /* Exists 與 Read 之間被刪除，也必須走重建。 */ }
            }

            return (previous, null);
        }

        var read = await ReadAsync();
        if (read.Content != null) return read.Content;
        var gate = Gates[(int)(Convert.ToUInt32(identity[..8], 16) % (uint)Gates.Length)];
        await gate.WaitAsync();
        try
        {
            read = await ReadAsync();
            if (read.Content != null) return read.Content;
            var previous = read.Manifest;
            var document = new HtmlDocument();
            document.LoadHtml(html);
            // 與原 GetFrontConten 的第一張 img 語意一致；缺 src 時保留 null。
            var firstImageUrl = document.DocumentNode.Descendants("img").FirstOrDefault()?.Attributes["src"]?.Value;
            var assets = new Dictionary<string, Asset>(StringComparer.Ordinal);
            var oldAssets = (previous?.Assets ?? []).GroupBy(asset => asset.Url, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            async Task<string> VersionUrlAsync(string url)
            {
                var path = url.Split('?', '#')[0];
                if (!TryResolve(root, orgName, path, out var physicalPath)) return url;
                if (!assets.TryGetValue(path, out var asset))
                {
                    if (!File.Exists(physicalPath))
                        asset = new Asset { Url = path, Length = -1 };
                    else if (oldAssets.TryGetValue(path, out var old) && IsCurrent(root, orgName, old))
                        asset = old;
                    else
                    {
                        try { asset = await ReadAssetAsync(path, physicalPath); }
                        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                        {
                            // 單一資源消失不能中止整份 HTML 的版本處理。
                            asset = new Asset { Url = path, Length = -1 };
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            // 暫時鎖定／覆寫中：保留原網址，下次請求重新嘗試。
                            reportStorageFailure(ex);
                            asset = new Asset { Url = path, Length = -2 };
                        }
                    }
                    assets.Add(path, asset);
                }
                return asset.Length < 0 ? url : AddVersion(url, asset.Hash);
            }

            async Task<string> VersionUrlsAsync(string value)
            {
                var builder = new StringBuilder();
                var offset = 0;
                foreach (Match match in UploadUrls.Matches(value))
                {
                    builder.Append(value, offset, match.Index - offset);
                    builder.Append(await VersionUrlAsync(match.Value));
                    offset = match.Index + match.Length;
                }
                return offset == 0 ? value : builder.Append(value, offset, value.Length - offset).ToString();
            }

            foreach (var node in document.DocumentNode.Descendants())
            {
                foreach (var attribute in node.Attributes.ToArray())
                {
                    if (attribute.Name is "style" or "srcset" or "data-srcset")
                    {
                        attribute.Value = System.Net.WebUtility.HtmlEncode(
                            await VersionUrlsAsync(HtmlEntity.DeEntitize(attribute.Value)));
                        continue;
                    }
                    if (attribute.Name is not ("src" or "href" or "data-src" or "data-pdf-url" or "poster"))
                        continue;
                    var url = HtmlEntity.DeEntitize(attribute.Value);
                    attribute.Value = System.Net.WebUtility.HtmlEncode(await VersionUrlAsync(url));
                }
            }
            foreach (var style in document.DocumentNode.Descendants("style").ToArray())
                style.InnerHtml = await VersionUrlsAsync(style.InnerHtml);

            var output = document.DocumentNode.OuterHtml;
            var manifest = new Manifest
            {
                Version = PipelineVersion, InputHash = inputHash, OutputHash = Hash(output),
                Assets = assets.Values.ToList(), FirstImageUrl = firstImageUrl
            };
            try
            {
                System.IO.Directory.CreateDirectory(directory);
                await WriteAtomicAsync(file, output);
                await WriteAtomicAsync(manifestPath, JsonSerializer.Serialize(manifest));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // 即使磁碟無法儲存，仍回傳本次已完成、帶版本的安全內容。
                reportStorageFailure(ex);
            }
            return new Result(output, firstImageUrl);
        }
        finally { gate.Release(); }
    }

    private static bool TryResolve(string root, string orgName, string url, out string physicalPath)
    {
        physicalPath = "";
        if (!url.StartsWith("/upload/", StringComparison.OrdinalIgnoreCase)) return false;
        var relative = Uri.UnescapeDataString(url[8..]).Replace('\\', '/');
        if (relative.StartsWith(orgName + "/", StringComparison.OrdinalIgnoreCase))
            relative = relative[(orgName.Length + 1)..];
        if (relative.Split('/').Any(part => part.StartsWith('_') || part is "." or "..")) return false;
        var candidate = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)) return false;
        physicalPath = candidate;
        return true;
    }

    private static bool IsCurrent(string root, string orgName, Asset asset)
    {
        if (!TryResolve(root, orgName, asset.Url, out var path)) return false;
        try
        {
            var info = new FileInfo(path);
            if (asset.Length == -2) return false;
            if (asset.Length < 0) return !info.Exists;
            return info.Exists && info.Length == asset.Length && info.LastWriteTimeUtc.Ticks == asset.ModifiedTicks;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return false;
        }
    }

    private static async Task<Asset> ReadAssetAsync(string url, string path)
    {
        var before = new FileInfo(path);
        var length = before.Length;
        var ticks = before.LastWriteTimeUtc.Ticks;
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();
        var after = new FileInfo(path);
        if (after.Length != length || after.LastWriteTimeUtc.Ticks != ticks)
            throw new IOException("檔案在產生 HTML 版本期間已變更，稍後重試。");
        return new Asset { Url = url, Hash = hash, Length = length, ModifiedTicks = ticks };
    }

    private static string AddVersion(string url, string hash)
    {
        var fragmentIndex = url.IndexOf('#');
        var fragment = fragmentIndex < 0 ? "" : url[fragmentIndex..];
        var address = fragmentIndex < 0 ? url : url[..fragmentIndex];
        var queryIndex = address.IndexOf('?');
        var path = queryIndex < 0 ? address : address[..queryIndex];
        string[] query = queryIndex < 0 ? [] : address[(queryIndex + 1)..].Split('&')
            .Where(value => !string.IsNullOrEmpty(value) &&
                !Uri.UnescapeDataString(value.Split('=')[0]).Equals("v", StringComparison.OrdinalIgnoreCase)).ToArray();
        return path + "?" + string.Join("&", query.Append("v=" + hash)) + fragment;
    }

    private static async Task<string> ReadTextAsync(string path)
    {
        // Windows 上允許原子替換正在被讀取的舊產物；reader 仍持有完整的舊檔 snapshot。
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
            4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static async Task WriteAtomicAsync(string path, string value)
    {
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, value, new UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
